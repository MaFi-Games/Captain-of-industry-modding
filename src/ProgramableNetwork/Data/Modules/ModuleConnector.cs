using Mafi.Serialization;

namespace ProgramableNetwork
{
    public class ModuleConnector
    {
        public long ModuleId { get; }
        public string OutputId { get; }

        public ModuleConnector(long moduleId, string name)
        {
            ModuleId = moduleId;
            OutputId = name;
        }

        public static void Serialize(ModuleConnector value, BlobWriter writer)
        {
            writer.WriteLong(value.ModuleId);
            writer.WriteString(value.OutputId);
        }

        public static ModuleConnector Deserialize(BlobReader reader)
        {
            return new ModuleConnector(reader.ReadLong(), reader.ReadString());
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleConnector mc && this.ModuleId == mc.ModuleId && this.OutputId == mc.OutputId;
        }
    }
}