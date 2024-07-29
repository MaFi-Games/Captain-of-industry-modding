using Mafi.Core.Mods;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Instructions
        {
            public partial class Get
            {
                public static readonly InstructionProto.ID Pause = new InstructionProto.ID("ProgramableNetwork_Operation_Get_Pause");
            }

            public partial class Set
            {
                public static readonly InstructionProto.ID Variable = new InstructionProto.ID("ProgramableNetwork_Operation_Set_Variable");
                public static readonly InstructionProto.ID Pause = new InstructionProto.ID("ProgramableNetwork_Operation_Set_Pause");
            }

            public static readonly InstructionProto.ID Goto = new InstructionProto.ID("ProgramableNetwork_Operation_Goto");
            public static readonly InstructionProto.ID Nop = new InstructionProto.ID("ProgramableNetwork_Operation_Nop");
            public static readonly InstructionProto.ID Invalid = new InstructionProto.ID("ProgramableNetwork_Operation_Invalid");
        }
    }

    internal class Instructions : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator
                .OperationProtoBuilder(NewIds.Instructions.Invalid, "Invalid")
                .Description("This istruction failed to load due to invalid version or existance")
                .Level(int.MaxValue)
                .Cost(0)
                .Runtime((program) => throw new ProgramException(NewIds.Texts.InvalidInstruction))
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Nop, "Nop")
                .Description("There is nothing to do, this operation is usable as label")
                .Cost(0)
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Set.Variable, "Set (variable)")
                .Description("Assigned any selectable or variable to specified variable")
                .Level(0)
                .Cost(2)
                .AddInput("value", "Value", InstructionProto.InputType.Any, InstructionProto.InputType.Instruction)
                .AddInput("variable", "Variable", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var building = program.Input[0, InstructionProto.InputType.Any];
                    var variable = program.Input[1, InstructionProto.InputType.Variable];
                    program.Variable[variable] = program.Input[0, InstructionProto.InputType.Any];
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Set.Pause, "Set (pause)")
                .Description("Pause selected entity")
                .Cost(2)
                .AddInput("entity", "Entity", InstructionProto.InputType.Entity, InstructionProto.InputType.Variable)
                .AddInput("pause", "Pause", InstructionProto.InputType.Boolean, InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var building = program.Input[0, InstructionProto.InputType.Entity];
                    var active = program.Input[1, InstructionProto.InputType.Boolean];

                    if (building.Entity == null)
                        throw new ProgramException(NewIds.Texts.EmptyInput.Format(0.ToString()));

                    if (!building.Entity.CanBePaused)
                        throw new ProgramException(NewIds.Texts.CanNotPause);

                    building.Entity.SetPaused(active.Boolean);
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Get.Pause, "Is (paused)")
                .Description("Pause selected entity")
                .Cost(2)
                .AddInput("entity", "Entity", InstructionProto.InputType.Entity, InstructionProto.InputType.Variable)
                .AddInput("pause", "Pause", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var building = program.Input[0, InstructionProto.InputType.Entity];
                    var variable = program.Input[1, InstructionProto.InputType.Variable];

                    if (building.Entity == null)
                        throw new ProgramException(NewIds.Texts.EmptyInput.Format(0.ToString()));

                    program.Variable[variable].Boolean = building.Entity.IsPaused;
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Goto, "Goto")
                .Description("Jumps when condition is fullfilled")
                .AddInput("when", "When", InstructionProto.InputType.Boolean, InstructionProto.InputType.Variable)
                .AddInput("toto", "Goto", InstructionProto.InputType.Instruction, InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    if (program.Input[0, InstructionProto.InputType.Boolean])
                        program.NextInstruction(program.Input[1, InstructionProto.InputType.Instruction]);
                })
                .BuildAndAdd();
        }
    }
}
