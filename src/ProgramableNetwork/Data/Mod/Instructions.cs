using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Instructions
        {
            public partial class Get
            {
                public static readonly InstructionProto.ID Pause = new InstructionProto.ID("ProgramableNetwork_Operation_Get_Pause");
                public static readonly InstructionProto.ID Capacity = new InstructionProto.ID("ProgramableNetwork_Operation_Get_Capacity");
                public static readonly InstructionProto.ID Count = new InstructionProto.ID("ProgramableNetwork_Operation_Get_Count");
            }

            public partial class Set
            {
                public static readonly InstructionProto.ID Variable = new InstructionProto.ID("ProgramableNetwork_Operation_Set_Variable");
                public static readonly InstructionProto.ID Pause = new InstructionProto.ID("ProgramableNetwork_Operation_Set_Pause");
            }

            public partial class Arithmetic
            {
                public static readonly InstructionProto.ID Sum = new InstructionProto.ID("ProgramableNetwork_Operation_Arithmetic_Sum");
                public static readonly InstructionProto.ID Sub = new InstructionProto.ID("ProgramableNetwork_Operation_Arithmetic_Sub");
                public static readonly InstructionProto.ID Mul = new InstructionProto.ID("ProgramableNetwork_Operation_Arithmetic_Mul");
                public static readonly InstructionProto.ID Div = new InstructionProto.ID("ProgramableNetwork_Operation_Arithmetic_Div");
            }

            public partial class Boolean
            {
                public static readonly InstructionProto.ID AND = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_AND");
                public static readonly InstructionProto.ID OR = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_OR");
                public static readonly InstructionProto.ID NOT = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_NOT");
                public static readonly InstructionProto.ID XOR = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_XOR");
                public static readonly InstructionProto.ID NOR = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_NOR");
                public static readonly InstructionProto.ID XNOR = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_XNOR");
                public static readonly InstructionProto.ID NAND = new InstructionProto.ID("ProgramableNetwork_Operation_Boolean_NAND");
            }

            public partial class Compare
            {
                public static readonly InstructionProto.ID Eql = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Eql");
                public static readonly InstructionProto.ID Neq = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Neq");
                public static readonly InstructionProto.ID Gtt = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Gtt");
                public static readonly InstructionProto.ID Ltt = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Ltt");
                public static readonly InstructionProto.ID Gte = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Gte");
                public static readonly InstructionProto.ID Lte = new InstructionProto.ID("ProgramableNetwork_Operation_Compare_Lte");
            }

            public partial class IO
            {
                public static readonly InstructionProto.ID Read = new InstructionProto.ID("ProgramableNetwork_Operation_IO_Read");
                public static readonly InstructionProto.ID Write = new InstructionProto.ID("ProgramableNetwork_Operation_IO_Write");
            }

            public static readonly InstructionProto.ID Goto = new InstructionProto.ID("ProgramableNetwork_Operation_Goto");
            public static readonly InstructionProto.ID Nop = new InstructionProto.ID("ProgramableNetwork_Operation_Nop");
            public static readonly InstructionProto.ID Invalid = new InstructionProto.ID("ProgramableNetwork_Operation_Invalid");
            public static readonly InstructionProto.ID Display = new InstructionProto.ID("ProgramableNetwork_Operation_Display");
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

                    building.Entity.SetPaused(active);
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

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Get.Capacity, "Get (capacity)")
                .Description("Returns capacity of selected entity (storage or transport)")
                .AddInput("storage", "Storage", InstructionProto.InputType.Entity, InstructionProto.InputType.Variable)
                .AddInput("value", "Read to", InstructionProto.InputType.Variable)
                .EntityFilter(entity => entity is StorageBase || entity is Transport)
                .Runtime((program) =>
                {
                    var building = program.Input[0, InstructionProto.InputType.Entity].Entity;
                    var variable = program.Input[1, InstructionProto.InputType.Variable];

                    if (building is StorageBase storage)
                        program.Variable[variable].Integer = storage.Capacity.Value;
                    else if (building is Transport transport)
                        program.Variable[variable].Integer = transport.Trajectory.MaxProducts;
                    else
                        throw new ProgramException(NewIds.Texts.HasNoStorage);
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Display, "Display")
                .Description("Displays content of variable")
                .AddInput("variable", "Varible", InstructionProto.InputType.Variable)
                .AddDisplay("value", "Value")
                .Runtime((program) =>
                {
                    program.Display[0] = program.Variable[program.Input[0, InstructionProto.InputType.Variable]];
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Get.Count, "Get (count)")
                .Description("Returns count of stored products in selected entity (storage or transport)")
                .AddInput("storage", "Storage", InstructionProto.InputType.Entity, InstructionProto.InputType.Variable)
                .AddInput("value", "Read to", InstructionProto.InputType.Variable)
                .EntityFilter(entity => entity is StorageBase || entity is Transport)
                .Runtime((program) =>
                {
                    var building = program.Input[0, InstructionProto.InputType.Entity].Entity;
                    var variable = program.Input[1, InstructionProto.InputType.Variable];

                    if (building is StorageBase storage)
                        program.Variable[variable].Integer = storage.CurrentQuantity.Value;
                    else if (building is Transport transport)
                        program.Variable[variable].Integer = transport.TransportedProducts
                                                                 .AsEnumerable()
                                                                 .Select(p => p.Quantity.Value)
                                                                 .Sum();
                    else
                        throw new ProgramException(NewIds.Texts.HasNoStorage);
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.IO.Read, "IO read")
                .Description("Read IO signal on selected or computer cable line")
                .AddInput("cable", "Cable", InstructionProto.InputType.Entity)
                .AddInput("line", "Line", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("variable", "Value", InstructionProto.InputType.Variable)
                .EntityFilter(Mafi.Fix32.MaxValue)
                .EntityFilter(entity => (entity is Transport tr && tr.Prototype.PortsShape.AllowedProductType == ProtocolProductProto.ProductType))
                .Runtime((program) =>
                {
                    var cable = program.Input[0, InstructionProto.InputType.Entity];
                    var line = program.Input[1, InstructionProto.InputType.Integer];
                    var variable = program.Input[2, InstructionProto.InputType.Variable];
                    program.Variable[variable] = program.IO[cable, line];
                })
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.IO.Write, "IO write")
                .Description("Writes IO signal on selected cable or computer cable line")
                .AddInput("cable", "Cable", InstructionProto.InputType.Entity, InstructionProto.InputType.Variable)
                .AddInput("line", "Line", InstructionProto.InputType.Integer, InstructionProto.InputType.Variable)
                .AddInput("variable", "Value", InstructionProto.InputType.Any, InstructionProto.InputType.Variable, InstructionProto.InputType.None)
                .EntityFilter(Mafi.Fix32.MaxValue)
                .EntityFilter(entity => (entity is Transport tr && tr.Prototype.PortsShape.AllowedProductType == ProtocolProductProto.ProductType))
                .Runtime((program) =>
                {
                    var cable = program.Input[0, InstructionProto.InputType.Entity];
                    var line = program.Input[1, InstructionProto.InputType.Integer];
                    var variable = program.Input[2, InstructionProto.InputType.Any];
                    program.IO[cable, line] = variable;
                })
                .BuildAndAdd();

            registrator
                .AritmeticOperationProtoBuilder(NewIds.Instructions.Arithmetic.Sum, "Arithmetic (a + b)", (a, b) => a + b)
                .Description("C is A + B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .AritmeticOperationProtoBuilder(NewIds.Instructions.Arithmetic.Sub, "Arithmetic (a - b)", (a, b) => a - b)
                .Description("C is A - B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .AritmeticOperationProtoBuilder(NewIds.Instructions.Arithmetic.Mul, "Arithmetic (a x b)", (a, b) => a * b)
                .Description("C is A x B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .AritmeticOperationProtoBuilder(NewIds.Instructions.Arithmetic.Div, "Arithmetic (a / b)", (a, b) =>
                {
                    if (b == 0)
                        throw new ProgramException(NewIds.Texts.DivisionByZero);
                    return a / b;
                })
                .Description("C is A / B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.AND, "Boolean (a and b)", (a, b) => a && b)
                .Description("C is A and B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.OR, "Boolean (a or b)", (a, b) => a && b)
                .Description("C is A or B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .OperationProtoBuilder(NewIds.Instructions.Boolean.NOT, "Boolean (not a)")
                .AddInput("a", "A", "Left operand", InstructionProto.InputType.Boolean, InstructionProto.InputType.Variable)
                .AddInput("b", "B", "Result variable", InstructionProto.InputType.Variable)
                .Runtime((program) =>
                {
                    var a = program.Input[0, InstructionProto.InputType.Boolean];
                    var b = program.Input[1, InstructionProto.InputType.Variable];
                    program.Variable[b].Boolean = !a;
                })
                .Description("B is not A")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.NOR, "Boolean (a xor b)", (a, b) => !(a || b))
                .Description("C is not A or B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.NAND, "Boolean (a nand b)", (a, b) => !(a && b))
                .Description("C is not A and B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.XOR, "Boolean (a xor b)", (a, b) => a != b)
                .Description("C is A not equal B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .BooleanOperationProtoBuilder(NewIds.Instructions.Boolean.XNOR, "Boolean (a xnor b)", (a, b) => a == b)
                .Description("C is A equal B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Eql, "Compare (a = b)", (a, b) => a == b)
                .Description("C is A equal B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Neq, "Compare (a ≠ b)", (a, b) => a != b)
                .Description("C is A not equal B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Gtt, "Compare (a > b)", (a, b) => a > b)
                .Description("C is A greater B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Gte, "Compare (a ≥ b)", (a, b) => a >= b)
                .Description("C is A greater or equal to B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Ltt, "Compare (a < b)", (a, b) => a < b)
                .Description("C is A lower than B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

            registrator
                .CompareOperationProtoBuilder(NewIds.Instructions.Compare.Lte, "Compare (a ≤ b)", (a, b) => a <= b)
                .Description("C is A lower or equal to B")
                .Cost(0)
                .Level(0)
                .BuildAndAdd();

        }
    }
}
