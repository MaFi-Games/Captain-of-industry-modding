using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public class ModuleConnector
    {
        public ModuleConnector(ModuleConnectorProto prototype, ComputerModule module)
        {
            Prototype = prototype;
            OwnerModule = module;
        }

        public ModuleConnectorProto Prototype { get; private set; }
        public readonly ComputerModule OwnerModule;
    }

    public class ModuleConnectorProto
    {
        public readonly Proto.Str String;
        public readonly InstructionProto.InputType[] Types;

        public ModuleConnectorProto(Proto.Str str, InstructionProto.InputType[] types)
        {
            String = str;
            Types = types;
        }

    }
}