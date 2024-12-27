namespace ProgramableNetwork
{
    public class ControllerLayout
    {
        public int Devices { get; }
        public int Memory { get; }
        public int Columns { get; }
        public int Rows { get; }

        public ControllerLayout(int devices, int memory, int columns, int rows)
        {
            this.Devices = devices;
            this.Memory = memory;
            this.Columns = columns;
            this.Rows = rows;
        }
    }
}