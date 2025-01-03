using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public class ModuleConnectorProto
    {
        public readonly string Id;
        public readonly Proto.Str Name;
        public readonly int Width;
        public readonly string DefaultText;

        public ModuleConnectorProto(string id, Proto.Str str, int width = 1, string defaultText = "")
        {
            Id = id;
            Name = str;
            Width = width;
            DefaultText = defaultText;
        }

    }
}