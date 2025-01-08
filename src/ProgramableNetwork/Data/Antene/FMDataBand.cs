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
            m_channels = new Lyst<FMDataBandChannel>();
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

        public IEnumerable<IDataBandChannel> Channels => m_channels?.Cast<IDataBandChannel>() ?? new List<IDataBandChannel>();

        private Lyst<FMDataBandChannel> m_channels;

        public static void Serialize(FMDataBand dataBand, BlobWriter writer)
        {
            dataBand.SerializeData(writer);
        }

        private void SerializeData(BlobWriter writer)
        {
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(SerializerVersion);
            Lyst<FMDataBandChannel>.Serialize(m_channels, writer);
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
            m_channels = Lyst<FMDataBandChannel>.Deserialize(reader);
        }

        public void initContext()
        {
            Log.Info($"Initializing FM BandData");
            var optional = Context.ProtosDb.Get<DataBandProto>(m_protoId);
            if (optional.HasValue)
            {
                Prototype = optional.Value;
                foreach (var channel in m_channels)
                {
                    channel.UpdateAntenaReference(Context.EntitiesManager);
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

        }

        public void CreateChannel()
        {
            m_channels.Add(new FMDataBandChannel());
        }

        public void RemoveChannel(IDataBandChannel channel)
        {
            m_channels.Remove(channel as FMDataBandChannel);
        }
    }
}