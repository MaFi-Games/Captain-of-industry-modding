using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Serialization;

namespace ProgramableNetwork
{
    public class FMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public int[] Value { get; set; }
        public int ValidIterations { get; set; }
        public Antena Antena { get => m_antena; set { m_antenaId = value?.Id ?? new EntityId(0) ; m_antena = value; } }

        public FMDataBand OriginalDataBand { get; set; }

        private Antena m_antena;
        private EntityId m_antenaId;

        public static void Serialize(FMDataBandChannel channel, BlobWriter writer)
        {
            writer.WriteByte(/*version*/2);
            writer.WriteInt(channel.Index);
            writer.WriteArray(channel.Value ?? new int[0]);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_antenaId.Value);
        }

        public static FMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            return new FMDataBandChannel()
            {
                Index = reader.ReadInt(),
                Value = reader.ReadArray<int>(),
                ValidIterations = reader.ReadInt(),
                m_antenaId = new EntityId(version > 0 ? reader.ReadInt() : 0)
            };
        }

        public void UpdateAntenaReference(FMDataBand self, IEntitiesManager manager)
        {
            OriginalDataBand = self;
            manager.TryGetEntity(m_antenaId, out m_antena);
        }

        public void Update()
        {
            if (Antena?.DataBand is FMDataBand targetDataBand)
            {
                int[] data = OriginalDataBand.Read(Index);
                targetDataBand.Update(Index, data);
            }
        }
    }
}