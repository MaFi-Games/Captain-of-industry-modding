using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class DataBandProto : Proto
    {
        public static DataBandProto Create<TDataBand, TDataBandChannel>(
            ID id, Str strings,
            Func<EntityContext, DataBandProto, TDataBand> constructor,
            int channels,
            Func<TDataBandChannel, TDataBandChannel, bool> comparator,
            Action<TDataBand, BlobWriter> serializer,
            Func<BlobReader, TDataBand> deserializer,
            Mafi.Fix32? distance = null,
            Func<EntityContext, TDataBandChannel, string> channelDisplay = null,
            Action<UiBuilder, StackContainer, TDataBandChannel, Action> buttons = null,
            IEnumerable<Tag> tags = null
        )
            where TDataBand : IDataBandTyped<TDataBandChannel>
            where TDataBandChannel : IDataBandChannel
        {
            return new DataBandProto(
                id, strings,
                constructor: (context, proto) => constructor(context, proto),
                channels,
                comparator: (a,b) => comparator((TDataBandChannel)a,(TDataBandChannel)b),
                serializer: (dataBand, reader)=>serializer((TDataBand)dataBand, reader),
                deserializer: (reader)=>deserializer(reader),
                distance ?? 1000.ToFix32(),
                channelDisplay: (context, channel) => channelDisplay?.Invoke(context, (TDataBandChannel)channel),
                buttons: (builder, container, channel, refresh) => buttons?.Invoke(builder, container, (TDataBandChannel)channel, refresh),
                tags);
        }

        private DataBandProto(ID id, Str strings,
            Func<EntityContext, DataBandProto, IDataBand> constructor,
            int channels,
            Func<IDataBandChannel, IDataBandChannel, bool> comparator,
            Action<IDataBand, BlobWriter> serializer,
            Func<BlobReader, IDataBand> deserializer,
            Fix32 distance,
            Func<EntityContext, IDataBandChannel, string> channelDisplay,
            Action<UiBuilder, StackContainer, IDataBandChannel, Action> buttons,
            IEnumerable<Tag> tags) : base(id, strings, tags)
        {
            Constructor = constructor;
            Channels = channels;
            Display = channelDisplay;
            Buttons = buttons;
            Distance = distance;
            Comparator = comparator;
            Serializer = serializer;
            Deserializer = deserializer;
        }

        public Func<EntityContext, DataBandProto, IDataBand> Constructor { get; }
        public int Channels { get; }
        public Func<EntityContext, IDataBandChannel, string> Display { get; }
        public Action<UiBuilder, StackContainer, IDataBandChannel, Action> Buttons { get; }
        public Fix32 Distance { get; }
        public Func<IDataBandChannel, IDataBandChannel, bool> Comparator { get; }
        public Action<IDataBand, BlobWriter> Serializer { get; }
        public Func<BlobReader, IDataBand> Deserializer { get; }
    }
}