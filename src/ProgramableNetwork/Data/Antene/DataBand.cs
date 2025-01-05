using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork
{
    public class DataBand
    {
        private static readonly int SerializerVersion = 0;
        private int m_validIterations;
        private DataBandProto m_proto;
        private Proto.ID m_protoId;

        public DataBand(EntityContext context, DataBandProto prototype)
        {
            Prototype = prototype;
            Context = context;
            Channels = new DataBandChannel[0];
        }

        private DataBand()
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
                m_protoId = m_proto.Id;
                m_proto = value;
            }
        }
        public int Channel { get; set; }
        public EntityContext Context { get; set; }
        public DataBandChannel[] Channels { get; private set; }

        public int ValidIterations => m_validIterations;

        public static void Serialize(DataBand dataBand, BlobWriter writer)
        {
            writer.WriteString(dataBand.m_protoId.Value);
            writer.WriteInt(SerializerVersion);
            writer.WriteInt(dataBand.Channel);
            writer.WriteInt(dataBand.m_validIterations);
        }

        public static DataBand Deserialize(BlobReader reader)
        {
            DataBand dataBand = new DataBand();
            dataBand.DeserializeData(reader);
            return dataBand;
        }

        private void DeserializeData(BlobReader reader)
        {
            m_protoId = new Proto.ID(reader.ReadString());
            int version = reader.ReadInt();
            Channel = reader.ReadInt();
            m_validIterations = reader.ReadInt();
        }

        public void initContext()
        {
            var optional = Context.ProtosDb.Get<DataBandProto>(m_protoId);
            if (optional.HasValue)
            {
                Prototype = optional.Value;
            }
            else
            {
                Prototype = Context.ProtosDb.Get<DataBandProto>(DataBands.UnknownDataBand).ValueOrNull;
            }
        }
    }

    public class DataBandChannel
    {
        public int Value { get; set; }
    }

    public class DataBandProto : Proto
    {
        public DataBandProto(ID id, Str strings,
            int channels,
            Func<int, string> channelDisplay = null,
            Action<UiBuilder, StackContainer, DataBandChannel, Action> buttons = null,
            IEnumerable<Tag> tags = null
        ) : base(id, strings, tags)
        {
            Channels = channels;
            Display = channelDisplay;
            Buttons = buttons;
        }

        public int Channels { get; }
        public Func<int, string> Display { get; }
        public Action<UiBuilder, StackContainer, DataBandChannel, Action> Buttons { get; }
    }
}