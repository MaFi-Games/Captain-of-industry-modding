using Mafi.Serialization;

namespace ProgramableNetwork
{
    public struct ModulePlacement
    {
        public long ModuleId;
        public bool Placement;

        public ModulePlacement(long moduleId, bool placement)
        {
            ModuleId = moduleId;
            Placement = placement;
        }

        public override bool Equals(object obj)
        {
            return obj is ModulePlacement other &&
                   ModuleId == other.ModuleId &&
                   Placement == other.Placement;
        }

        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + ModuleId.GetHashCode();
            hashCode = hashCode * -1521134295 + Placement.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out long item1, out bool item2)
        {
            item1 = ModuleId;
            item2 = Placement;
        }

        public static implicit operator (long, bool)(ModulePlacement value)
        {
            return (value.ModuleId, value.Placement);
        }

        public static implicit operator ModulePlacement((long, bool) value)
        {
            return new ModulePlacement(value.Item1, value.Item2);
        }

        public static implicit operator ModulePlacement(long value)
        {
            return new ModulePlacement(value, true);
        }


        public static void Serialize(ModulePlacement value, BlobWriter writer)
        {
            writer.WriteLong(value.ModuleId);
            writer.WriteBool(value.Placement);
        }

        public static ModulePlacement Deserialize(BlobReader reader)
        {
            return (reader.ReadLong(), reader.ReadBool());
        }
    }
}
