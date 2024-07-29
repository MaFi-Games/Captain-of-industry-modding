using Mafi.Localization;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public class InstructionInput
    {
        public InstructionInput(Proto.Str strings, params InstructionProto.InputType[] types)
        {
            this.Strings = strings;
            this.Types = types;
        }

        public Proto.Str Strings { get; }
        public InstructionProto.InputType[] Types { get; }
    }
}