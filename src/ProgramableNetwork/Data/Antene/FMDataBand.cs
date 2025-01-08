using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class FMDataBand : IDataBandTyped<FMDataBandChannel>
    {
        private static readonly int SerializerVersion = 0;
        private DataBandProto m_proto;
        private Proto.ID m_protoId;

        public FMDataBand(EntityContext context, DataBandProto prototype)
        {
            Prototype = prototype;
            Context = context;
            m_redirected = new Lyst<FMDataBandChannel>();
            m_active = new Lyst<FMDataBandChannel>();
            for (int i = 0; i < prototype.Channels; i++)
            {
                m_active.Add(new FMDataBandChannel() { Index = i, OriginalDataBand = this });
            }
        }

        private FMDataBand()
        {
        }

        public DataBandProto Prototype
        {
            get
            {
                return m_proto;
            }
            private set
            {
                m_proto = value;
                m_protoId = m_proto.Id;
            }
        }
        public EntityContext Context { get; set; }

        public IEnumerable<IDataBandChannel> Channels => m_redirected?.Cast<IDataBandChannel>() ?? new List<IDataBandChannel>();

        private Lyst<FMDataBandChannel> m_redirected;
        private Lyst<FMDataBandChannel> m_active;

        public static void Serialize(FMDataBand dataBand, BlobWriter writer)
        {
            dataBand.SerializeData(writer);
        }

        private void SerializeData(BlobWriter writer)
        {
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(SerializerVersion);
            Lyst<FMDataBandChannel>.Serialize(m_redirected, writer);
            Lyst<FMDataBandChannel>.Serialize(m_active, writer);
        }

        public static FMDataBand Deserialize(BlobReader reader)
        {
            FMDataBand dataBand = new FMDataBand();
            dataBand.DeserializeData(reader);
            return dataBand;
        }

        private void DeserializeData(BlobReader reader)
        {
            m_protoId = new Proto.ID(reader.ReadString());
            int version = reader.ReadInt();
            m_redirected = Lyst<FMDataBandChannel>.Deserialize(reader);
            m_active = Lyst<FMDataBandChannel>.Deserialize(reader);
        }

        public void initContext()
        {
            Log.Info($"Initializing FM BandData");
            var optional = Context.ProtosDb.Get<DataBandProto>(m_protoId);
            if (optional.HasValue)
            {
                Prototype = optional.Value;
                foreach (var channel in m_redirected)
                {
                    channel.UpdateAntenaReference(this, Context.EntitiesManager);
                }
            }
            else
            {
                Mafi.Log.Error($"Prototype not found: {m_protoId}");
                Prototype = Context.ProtosDb.Get<DataBandProto>(DataBands.DataBand_Unknown).ValueOrThrow("Unknown signal not found");
            }
        }

        public void Update()
        {
            foreach (var item in m_active)
            {
                if (item.ValidIterations-- == 0)
                {
                    // After one second reset signal
                    item.Value = new int[0];
                }
            }

            foreach (var item in m_redirected)
            {
                item.Update();
            }
        }

        public void Update(int index, int[] value)
        {
            m_active[index].Value = new int[value.Length];
            Array.Copy(value, m_active[index].Value, value.Length);
            m_active[index].ValidIterations = 60;
        }

        public int[] Read(int index)
        {
            int[] ints = new int[m_active[index].Value.Length];
            if (m_active[index].ValidIterations > 0)
                Array.Copy(m_active[index].Value, ints, ints.Length);
            // else only zeros
            return ints;
        }

        public void CreateChannel()
        {
            m_redirected.Add(new FMDataBandChannel() { OriginalDataBand = this });
        }

        public void RemoveChannel(IDataBandChannel channel)
        {
            m_redirected.Remove(channel as FMDataBandChannel);
        }
    }
}