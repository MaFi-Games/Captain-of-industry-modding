using Mafi.Core.Mods;
using System;

namespace ProgramableNetwork
{
    public static class OperationProtoBuilderExtension
    {
        public static OperationProtoBuilder OperationProtoBuilder(this ProtoRegistrator registrator, InstructionProto.ID id, string name)
        {
            return new OperationProtoBuilder(registrator, id, name);
        }
        public static OperationProtoBuilder AritmeticOperationProtoBuilder(this ProtoRegistrator registrator, InstructionProto.ID id, string name, Func<int, int, int> operation)
        {
            return new OperationProtoBuilder(registrator, id, name)
                .AddInput("a", "A", "Left operand", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("b", "B", "Right operand", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("c", "C", "Result variable", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var a = program.Input[0, InstructionProto.InputType.Integer];
                    var b = program.Input[1, InstructionProto.InputType.Integer];
                    var c = program.Input[2, InstructionProto.InputType.Variable];
                    program.Variable[c].Integer = operation(a, b);
                });
        }
        public static OperationProtoBuilder BooleanOperationProtoBuilder(this ProtoRegistrator registrator, InstructionProto.ID id, string name, Func<bool, bool, bool> operation)
        {
            return new OperationProtoBuilder(registrator, id, name)
                .AddInput("a", "A", "Left operand", InstructionProto.InputType.Boolean, InstructionProto.InputType.Variable)
                .AddInput("b", "B", "Right operand", InstructionProto.InputType.Boolean, InstructionProto.InputType.Variable)
                .AddInput("c", "C", "Result variable", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var a = program.Input[0, InstructionProto.InputType.Boolean];
                    var b = program.Input[1, InstructionProto.InputType.Boolean];
                    var c = program.Input[2, InstructionProto.InputType.Variable];
                    program.Variable[c].Boolean = operation(a, b);
                });
        }
        public static OperationProtoBuilder CompareOperationProtoBuilder(this ProtoRegistrator registrator, InstructionProto.ID id, string name, Func<int, int, bool> operation)
        {
            return new OperationProtoBuilder(registrator, id, name)
                .AddInput("a", "A", "Left operand", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("b", "B", "Right operand", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("c", "C", "Result variable", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var a = program.Input[0, InstructionProto.InputType.Integer];
                    var b = program.Input[1, InstructionProto.InputType.Integer];
                    var c = program.Input[2, InstructionProto.InputType.Variable];
                    program.Variable[c].Boolean = operation(a, b);
                });
        }
    }
}
