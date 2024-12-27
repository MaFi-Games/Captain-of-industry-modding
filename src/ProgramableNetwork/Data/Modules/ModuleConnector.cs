using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public class ModuleConnector
    {
        public ModuleConnector(ModuleConnectorProto prototype, Module module)
        {
            Prototype = prototype;
            OwnerModule = module;
        }

        public ModuleConnectorProto Prototype { get; private set; }
        public readonly Module OwnerModule;
    }

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