using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public class ModuleConnectorProto
    {
        public readonly string Id;
        public readonly Proto.Str String;

        public ModuleConnectorProto(string id, Proto.Str str)
        {
            Id = id;
            String = str;
        }

    }
}