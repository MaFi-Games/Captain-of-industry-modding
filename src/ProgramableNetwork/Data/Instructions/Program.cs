using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

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

            public static readonly LocStr2 EmptyInputIndexed = Loc.Str2("ProgramableNetwork_EmptyInputIndexed",
                "Input '{0}' at index '{1}' is not set", "");

            public static readonly LocStr2 TypeOr = Loc.Str2("ProgramableNetwork_TypeOr",
                "{0}' or '{1}", "used in empty example argument");

            public static readonly LocStr CanNotPause = Loc.Str("ProgramableNetwork_CanNotPause",
                "Selected entity can not be paused", "");

            public static readonly LocStr HasNoStorage = Loc.Str("ProgramableNetwork_HasNoStorage",
                "Selected entity has no storage", "");

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

            public static readonly LocStr End = Loc.Str("ProgramableNetwork_End",
                "End", "");

            public static readonly LocStr DivisionByZero = Loc.Str("ProgramableNetwork_DivisionByZero",
                "Division by zero", "This may happen during operation: A / B");

            public static readonly LocStr CableIsNotConnected = Loc.Str("ProgramableNetwork_CableIsNotConnected",
                "Cable is not connected", "");

            public static readonly Dictionary<InstructionProto.InputType, LocStr> PointerTypes
                = new Dictionary<InstructionProto.InputType, LocStr>()
                {
                    { InstructionProto.InputType.None,               Loc.Str("ProgramableNetwork_PointerType_None"         , "None", "") },
                    { InstructionProto.InputType.Any,                Loc.Str("ProgramableNetwork_PointerType_Any"          , "Any", "") },
                    { InstructionProto.InputType.Variable,           Loc.Str("ProgramableNetwork_PointerType_Variable"     , "Variable", "") },
                    { InstructionProto.InputType.Instruction,        Loc.Str("ProgramableNetwork_PointerType_Instruction"  , "Instruction", "") },
                    { InstructionProto.InputType.Boolean,            Loc.Str("ProgramableNetwork_PointerType_Boolean"      , "Boolean", "") },
                    { InstructionProto.InputType.Integer,            Loc.Str("ProgramableNetwork_PointerType_Integer"      , "Integer", "") },
                    { InstructionProto.InputType.Entity,             Loc.Str("ProgramableNetwork_PointerType_Entity"       , "Entity", "") },
                    { InstructionProto.InputType.StaticEntity,       Loc.Str("ProgramableNetwork_PointerType_StaticEntity" , "Static", "") },
                    { InstructionProto.InputType.DynamicEntity,      Loc.Str("ProgramableNetwork_PointerType_DynamicEntity", "Dynamic", "") },
                    { InstructionProto.InputType.Product,            Loc.Str("ProgramableNetwork_PointerType_Product"      , "Product", "") },

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
                public static readonly LocStr Copy = Loc.Str("ProgramableNetwork_Tool_Copy",
                    "Copy", "");
                public static readonly LocStr Paste = Loc.Str("ProgramableNetwork_Tool_Paste",
                    "Paste", "");
                public static readonly LocStr Up = Loc.Str("ProgramableNetwork_Tool_Up",
                    "Up", "");
                public static readonly LocStr Down = Loc.Str("ProgramableNetwork_Tool_Down",
                    "Down", "");
                public static readonly LocStr Pick = Loc.Str("ProgramableNetwork_Tool_Pick",
                    "Pick", "");
                public static readonly LocStr Apply = Loc.Str("ProgramableNetwork_Tool_Apply",
                    "Apply", "");
                public static readonly LocStr Edit = Loc.Str("ProgramableNetwork_Tool_Edit",
                    "Edit", "");
            }
        }
    }

    public class Program
    {
        private readonly Computer m_computer;
        private MemoryPointer[] m_inputs;
        private MemoryPointer[] m_variables;
        private MemoryPointer[] m_displays;

        public Program(Computer computer)
        {
            m_computer = computer;
            m_variables = new MemoryPointer[computer.Prototype.Variables];
            for (int i = 0; i < computer.Prototype.Variables; i++)
            {
                m_variables[i] = new MemoryPointer();
                m_variables[i].Recontext(computer);
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

        public IORef IO => new IORef(this);

        public class IORef
        {
            private readonly Program program;

            public IORef(Program program)
            {
                this.program = program;
            }

            public MemoryPointer this[Entity cable, int line]
            {
                get => program.GetIO(cable, line);
                set => program.SetIO(cable, line, value);
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

            public MemoryPointer this[int index]
            {
                get => program.GetInput(index);
            }
        }

        public InputRef Input => new InputRef(this);

        public class DisplayRef
        {
            private readonly Program program;

            public DisplayRef(Program program)
            {
                this.program = program;
            }

            public MemoryPointer this[int index]
            {
                set => program.SetDisplay(index, value);
                //get => program.GetDisplay(index);
            }
        }

        public void SetDisplay(int index, MemoryPointer value)
        {
            if (index < 0 || index >= m_displays.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_displays.Length.ToString()
                    ));

            m_displays[index].Assign(value);
        }

        public MemoryPointer GetDisplay(int index)
        {
            if (index < 0 || index >= m_displays.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_displays.Length.ToString()
                    ));

            return m_displays[index];
        }

        public DisplayRef Display => new DisplayRef(this);

        public long? ContinueInstruction { get; set; }

        public MemoryPointer GetInput(int index)
        {
            if (index < 0 || index >= m_inputs.Length)
                throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                    index.ToString(), m_inputs.Length.ToString()
                    ));

            if (m_inputs[index].Context == null)
                m_inputs[index].Recontext(m_computer);
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

        internal void SetDisplays(MemoryPointer[] displays)
        {
            this.m_displays = displays;
        }

        internal void SetVariables(MemoryPointer[] variables)
        {
            this.m_variables = variables;
        }

        private void SetIO(Entity cable, int line, MemoryPointer value)
        {
            if (cable is Transport transport && transport.Prototype.PortsShape.AllowedProductType == ProtocolProductProto.ProductType)
            {
                var connectedShape = transport.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.ConnectedPort.Value.OwnerEntity.Id == m_computer.Id)
                    .First();

                if (connectedShape == null)
                    throw new ProgramException(NewIds.Texts.CableIsNotConnected);

                int maximumOfSignals = transport.Prototype.TransportedProductsSpacing.Value.IntegerPart;
                if (line < 0 || line >= maximumOfSignals)
                    throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                        line.ToString(), maximumOfSignals.ToString()
                        ));

                m_computer.GetIO(connectedShape.ConnectedPort.Value.Id, line).Assign(value);
            }
            else if (cable is Computer connectedComputer)
            {
                var connectedShape = connectedComputer.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.ConnectedPort.Value.OwnerEntity.Id == m_computer.Id)
                    .First();

                if (connectedShape == null)
                    throw new ProgramException(NewIds.Texts.CableIsNotConnected);

                m_computer.GetIO(connectedShape.Id, line).Assign(value);
            }
            else
            {
                throw new ProgramException(NewIds.Texts.CableIsNotConnected);
            }
        }

        private MemoryPointer GetIO(Entity cable, int line)
        {
            if (cable is Transport transport && transport.Prototype.PortsShape.AllowedProductType == ProtocolProductProto.ProductType)
            {
                var connectedShape = transport.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.ConnectedPort.Value.OwnerEntity.Id == m_computer.Id)
                    .First();

                if (connectedShape == null)
                    throw new ProgramException(NewIds.Texts.CableIsNotConnected);

                int maximumOfSignals = transport.Prototype.TransportedProductsSpacing.Value.IntegerPart;
                if (line < 0 || line >= maximumOfSignals)
                    throw new ProgramException(NewIds.Texts.IndexOutOfRange.Format(
                        line.ToString(), maximumOfSignals.ToString()
                        ));

                var connectedTargetShape = transport.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.Id != connectedShape.Id)
                    .Select(portShape => portShape.ConnectedPort.ValueOrNull)
                    .First();

                if (connectedTargetShape == null)
                {
                    var mp = new MemoryPointer();
                    mp.Recontext(m_computer);
                    return mp;
                }

                return ((Computer)connectedTargetShape.OwnerEntity).GetIO(connectedTargetShape.Id, line);
            }
            else if (cable is Computer connectedComputer)
            {
                var connectedShape = connectedComputer.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.ConnectedPort.Value.OwnerEntity.Id == m_computer.Id)
                    .First();

                if (connectedShape == null)
                    throw new ProgramException(NewIds.Texts.CableIsNotConnected);

                // always connected
                var connectedTargetShape = connectedComputer.Ports
                    .Where(portShape => portShape.IsConnected
                        && portShape.ConnectedPort.HasValue
                        && portShape.Id != connectedShape.Id)
                    .Select(portShape => portShape.ConnectedPort.ValueOrNull)
                    .First();

                return ((Computer)connectedTargetShape.OwnerEntity).GetIO(connectedTargetShape.Id, line);
            }
            else
            {
                throw new ProgramException(NewIds.Texts.CableIsNotConnected);
            }
        }
    }
}