using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Serialization;

namespace ProgramableNetwork
{
    public class FMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public int Value { get; set; }
        public int ValidIterations { get; set; }
        public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0) ; m_antena = value; } }

        private Antena m_antena;
        private EntityId m_antenaId;

        public static void Serialize(FMDataBandChannel channel, BlobWriter writer)
        {
            writer.WriteByte(/*version*/1);
            writer.WriteInt(channel.Index);
            writer.WriteInt(channel.Value);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_antenaId.Value);
        }

        public static FMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            return new FMDataBandChannel()
            {
                Index = reader.ReadInt(),
                Value = reader.ReadInt(),
                ValidIterations = reader.ReadInt(),
                m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0)
            };
        }

        public void UpdateAntenaReference(IEntitiesManager manager)
        {
            manager.TryGetEntity(m_antenaId, out m_antena);
        }
    }
}