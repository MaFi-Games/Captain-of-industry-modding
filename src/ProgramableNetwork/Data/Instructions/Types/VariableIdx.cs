namespace ProgramableNetwork
{
#pragma warning disable CS0660
#pragma warning disable CS0661
    public struct VariableIdx
#pragma warning restore CS0660
#pragma warning restore CS0661
    {
        public readonly int Data;

        public VariableIdx(int data)
        {
            this.Data = data;
        }

        public static bool operator ==(VariableIdx idx, int value)
        {
            return idx.Data == value;
        }

        public static bool operator !=(VariableIdx idx, int value)
        {
            return idx.Data != value;
        }

        public static bool operator ==(int value, VariableIdx idx)
        {
            return idx.Data == value;
        }

        public static bool operator !=(int value, VariableIdx idx)
        {
            return idx.Data != value;
        }
    }
}