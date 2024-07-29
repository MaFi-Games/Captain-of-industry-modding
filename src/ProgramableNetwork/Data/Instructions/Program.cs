using Mafi.Core.Entities;
using Mafi.Localization;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Texts
        {
            public static readonly LocStr2 InvalidPointerType = Loc.Str2("ProgramableNetwork_InvalidPointerType",
                "Invalid pointer type: expected='{0}' != got:'{1}'", "");

            public static readonly LocStr2 IndexOutOfRange = Loc.Str2("ProgramableNetwork_IndexOutOfRange",
                "Index out of range: 0 <= {0} < {1}", "");

            public static readonly LocStr1 EmptyInput = Loc.Str1("ProgramableNetwork_EmptyInput",
                "Input '{0}' is not set", "");

            public static readonly LocStr2 EmptyInputIndexed = Loc.Str2("ProgramableNetwork_EmptyInput",
                "Input '{0}' at index '{1}' is not set", "");

            public static readonly LocStr2 TypeOr = Loc.Str2("ProgramableNetwork_EmptyInput",
                "{0}' or '{1}", "used in empty example argument");

            public static readonly LocStr CanNotPause = Loc.Str("ProgramableNetwork_CanNotPause",
                "Selected entity can not be paused", "");

            public static readonly LocStr WatchDogStop = Loc.Str("ProgramableNetwork_WatchDogStop",
                "Instruction exceeded processable time", "");

            public static readonly LocStr UnknownError = Loc.Str("ProgramableNetwork_UnknownError",
                "Contact captain DeznekCZ, there went something wrong", "");

            public static readonly LocStr1 InstructionCount = Loc.Str1("ProgramableNetwork_InstructionCount",
                "Count of instructions: {0}", "");

            public static readonly LocStr InvalidInstruction = Loc.Str("ProgramableNetwork_InvalidInstruction",
                "Invalid instruction", "");

            public static readonly LocStr1 Variable = Loc.Str1("ProgramableNetwork_Variable",
                "Variable picked: {0}", "");

            public static readonly LocStr True = Loc.Str("ProgramableNetwork_True",
                "True", "");

            public static readonly LocStr False = Loc.Str("ProgramableNetwork_False",
                "False", "");

            public static readonly Dictionary<InstructionProto.InputType, LocStr> PointerTypes
                = new Dictionary<InstructionProto.InputType, LocStr>()
                {
                    { InstructionProto.InputType.None,               Loc.Str("ProgramableNetwork_None"         , "None", "") },
                    { InstructionProto.InputType.Any,                Loc.Str("ProgramableNetwork_Any"          , "Any", "") },
                    { InstructionProto.InputType.Variable,           Loc.Str("ProgramableNetwork_Variable"     , "Variable", "") },
                    { InstructionProto.InputType.Instruction,        Loc.Str("ProgramableNetwork_Instruction"  , "Instruction", "") },
                    { InstructionProto.InputType.Values,             Loc.Str("ProgramableNetwork_Values"       , "Values", "") },
                    { InstructionProto.InputType.ValuesGroup,        Loc.Str("ProgramableNetwork_ValuesGroup"  , "Values", "") },
                    { InstructionProto.InputType.Boolean,            Loc.Str("ProgramableNetwork_Boolean"      , "Boolean", "") },
                    { InstructionProto.InputType.Integer,            Loc.Str("ProgramableNetwork_Integer"      , "Integer", "") },
                    { InstructionProto.InputType.Entity,             Loc.Str("ProgramableNetwork_Entity"       , "Entity", "") },
                    { InstructionProto.InputType.EntityGroup,        Loc.Str("ProgramableNetwork_Entity"       , "Entity", "") },
                    { InstructionProto.InputType.StaticEntity,       Loc.Str("ProgramableNetwork_StaticEntity" , "Static entity (building)", "") },
                    { InstructionProto.InputType.StaticEntityGroup,  Loc.Str("ProgramableNetwork_StaticEntity" , "Static entity (building)", "") },
                    { InstructionProto.InputType.Machine,            Loc.Str("ProgramableNetwork_Machine"      , "Machine", "") },
                    { InstructionProto.InputType.Settlement,         Loc.Str("ProgramableNetwork_Settlement"   , "Settlement", "") },
                    { InstructionProto.InputType.DynamicEntity,      Loc.Str("ProgramableNetwork_DynamicEntity", "Moving entity", "") },
                    { InstructionProto.InputType.DynamicEntityGroup, Loc.Str("ProgramableNetwork_DynamicEntity", "Moving entity", "") },
                    { InstructionProto.InputType.Vehicle,            Loc.Str("ProgramableNetwork_Vehicle"      , "Vehicle", "") },
                    { InstructionProto.InputType.VehicleGroup,       Loc.Str("ProgramableNetwork_Vehicle"      , "Vehicle", "") },
                    { InstructionProto.InputType.Truck,              Loc.Str("ProgramableNetwork_Truck"        , "Truck", "") },
                    { InstructionProto.InputType.Excavator,          Loc.Str("ProgramableNetwork_Excavator"    , "Excavator", "") },

                };

            public static readonly Dictionary<bool, LocStr> Boolean
                = new Dictionary<bool, LocStr>()
                {
                    { true,  True },
                    { false, False },
                };

            public partial class Tools
            {
                public static readonly LocStr Remove = Loc.Str("ProgramableNetwork_Tool_Delete",
                    "Remove", "");
                public static readonly LocStr Add = Loc.Str("ProgramableNetwork_Tool_Add",
                    "Add", "");
                public static readonly LocStr Copy = Loc.Str("ProgramableNetwork_Tool_Add",
                    "Copy", "");
                public static readonly LocStr Paste = Loc.Str("ProgramableNetwork_Tool_Paste",
                    "Paste", "");
                public static readonly LocStr Up = Loc.Str("ProgramableNetwork_Tool_Up",
                    "Up", "");
                public static readonly LocStr Down = Loc.Str("ProgramableNetwork_Tool_Down",
                    "Down", "");
                public static readonly LocStr Pick = Loc.Str("ProgramableNetwork_Tool_Pick",
                    "Pick", "");
            }
        }
    }

    public class Program
    {
        private MemoryPointer[] m_inputs;
        private MemoryPointer[] m_variables;
        private EntityContext m_context;

        public Program(Computer computer)
        {
            m_variables = new MemoryPointer[computer.Prototype.Variables];
            m_context = computer.Context;
            for (int i = 0; i < computer.Prototype.Variables; i++)
            {
                m_variables[i] = new MemoryPointer();
                m_variables[i].Context = computer.Context;
            }
        }

        public class VariableRef
        {
            private readonly Program program;

            public VariableRef(Program program)
            {
                this.program = program;
            }

            public MemoryPointer this[VariableIdx index]
            {
                get => program.GetVariable(index.Data);
                set => program.SetVariable(index.Data, value);
            }
        }

        public VariableRef Variable => new VariableRef(this);

        public class InputRef
        {
            private readonly Program program;

            public InputRef(Program program)
            {
                this.program = program;
            }

            public MemoryPointer this[int index, InstructionProto.InputType type]
            {
                get => program.GetInput(index).IsOrThrow(program, type);
            }
        }

        public InputRef Input => new InputRef(this);

        public long? ContinueInstruction { get; private set; }

        public MemoryPointer GetInput(int index)
        {
            if (index < 0 || index >= m_inputs.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_inputs.Length.ToString()
                    ));

            if (m_inputs[index].Context == null)
                m_inputs[index].Context = m_context;
            return m_inputs[index];
        }

        private void SetVariable(int index, MemoryPointer value)
        {
            if (index < 0 || index >= m_variables.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_inputs.Length.ToString()
                    ));

            m_variables[index].Assign(value);
        }

        private MemoryPointer GetVariable(int index)
        {
            if (index < 0 || index >= m_variables.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_inputs.Length.ToString()
                    ));

            return m_variables[index];
        }

        public void NextInstruction(MemoryPointer instruction)
        {
            if (instruction.Type != InstructionProto.InputType.Instruction)
            {
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                    NewIds.Texts.PointerTypes[InstructionProto.InputType.Instruction],
                    NewIds.Texts.PointerTypes[instruction.Type]
                    ));
            }

            ContinueInstruction = instruction.Data;
        }

        internal void SetInputs(MemoryPointer[] inputs)
        {
            this.m_inputs = inputs;
        }

        internal void SetVariables(MemoryPointer[] variables)
        {
            this.m_variables = variables;
        }
    }
}