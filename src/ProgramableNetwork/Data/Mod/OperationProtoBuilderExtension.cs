using Mafi.Core.Mods;

namespace ProgramableNetwork
{
    public static class OperationProtoBuilderExtension
    {
        public static OperationProtoBuilder OperationProtoBuilder(this ProtoRegistrator registrator, InstructionProto.ID id, string name)
        {
            return new OperationProtoBuilder(registrator, id, name);
        }
    }
}
