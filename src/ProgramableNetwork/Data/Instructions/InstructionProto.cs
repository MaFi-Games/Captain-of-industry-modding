
using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProgramableNetwork
{
    public class InstructionProto : Proto
    {
        [DebuggerStepThrough]
        [DebuggerDisplay("{Value,nq}")]
        [ManuallyWrittenSerialization]
        public new readonly struct ID : IEquatable<ID>, IComparable<ID>
        {
            //
            // Souhrn:
            //     Underlying string value of this Id.
            public readonly string Value;

            public ID(string value)
            {
                Value = value;
            }

            public static bool operator ==(ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public override bool Equals(object other)
            {
                if (other is ID)
                {
                    ID other2 = (ID)other;
                    return Equals(other2);
                }

                return false;
            }

            public bool Equals(ID other)
            {
                return string.Equals(Value, other.Value, StringComparison.Ordinal);
            }

            public int CompareTo(ID other)
            {
                return string.CompareOrdinal(Value, other.Value);
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }

            public static void Serialize(ID value, BlobWriter writer)
            {
                writer.WriteString(value.Value);
            }

            public static ID Deserialize(BlobReader reader)
            {
                return new ID(reader.ReadString());
            }

            public static implicit operator Proto.ID(ID id)
            {
                return new Proto.ID(id.Value);
            }
        }

        public new ID Id { get; }
        public int InstructionLevel { get; }
        public int InstructionCost { get; }
        public Action<Program> Runtime { get; }
        public InstructionInput[] Inputs { get; }
        public Proto.Str[] Displays { get; }
        public Action<UiData> CustomUI { get; }
        public Func<Entity, bool> EntityFilter { get; }
        public Fix32 EntitySearchDistance { get; set; }
        public Func<ProductProto, bool> ProductFilter { get; }

        [Flags]
        public enum InputType : uint {
            // Description
            //  0-15 : single type
            // 16-23 : group type
            // 28-31 : category type

            None = 0,
            // anything that is not a variable
            Any = 0xEFFFFFFF,
            // variable type - masking
            Control = 0x10000000,
            Variable = 0x10000001,
            Instruction = 0x10000002,
            // values
            Value = 0x20000000,
            Boolean = 0x20000002,
            Integer = 0x20000003,
            // entities
            Entity = 0x40000000,
            Product = 0x80000000,
        }

        public InstructionProto(ID id, Str strings,
            Action<Program> runtime,
            Action<UiData> customUI = null,
            Func<Entity, bool> entityFilter = null,
            Fix32? entitySearchDistance = null,
            Func<ProductProto, bool> productFilter = null,
            IEnumerable<Tag> tags = null,
            InstructionInput[] inputs = null,
            Proto.Str[] displays = null,
            int instructionLevel = 1, int instructionCost = 1) : base(id, strings, tags)
        {
            this.Id = id;
            this.InstructionLevel = instructionLevel;
            this.InstructionCost = instructionCost;
            this.Runtime = runtime;
            this.Inputs = inputs ?? new InstructionInput[0];
            this.Displays = displays ?? new Proto.Str[0];
            this.CustomUI = customUI ?? ((_) => { });
            this.EntityFilter = entityFilter ?? (entity => true);
            this.EntitySearchDistance = entitySearchDistance ?? 10.ToFix32();
            this.ProductFilter = productFilter ?? (entity => true);
        }
    }

    public static class InputExtensions
    {
        public static MemoryPointer IsOrThrow(this MemoryPointer pointer, Program program, InstructionProto.InputType expectedType)
        {
            if (pointer.Type == InstructionProto.InputType.Variable 
                && expectedType != InstructionProto.InputType.Variable)
                pointer = program.Variable[pointer];

            if (pointer.IsNull)
                throw new ProgramException(NewIds.Texts.EmptyInput.Format(
                    NewIds.Texts.PointerTypes[expectedType]));

            if (expectedType == InstructionProto.InputType.Any)
                return pointer;

            if (pointer.Type != expectedType)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                    NewIds.Texts.PointerTypes[expectedType],
                    NewIds.Texts.PointerTypes[pointer.Type]
                    ));

            return pointer;
        }

        public static int GetInstructionCost(this InstructionProto.InputType type)
        {
            return type.HasFlag(InstructionProto.InputType.Control) ? 0 :
                   type.HasFlag(InstructionProto.InputType.Value) ? 1 : // bool or number
                   type == InstructionProto.InputType.Entity ? 2 :
                   type == InstructionProto.InputType.Product ? 3 :
                   0;
        }

        public static bool IsGroup(this InstructionProto.InputType type, InstructionProto.InputType category)
        {
            return (category & type) != 0;
        }

        public static InstructionProto.InputType Filter(this InstructionProto.InputType type, InstructionProto.InputType category)
        {
            return type & category;
        }

        public static InstructionProto.InputType OrVariable(this InstructionProto.InputType type)
        {
            return type | InstructionProto.InputType.Variable;
        }

        public static Proto.Str Input(this InstructionProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__input__" + name), text, description);
        }

        public static Proto.Str Display(this InstructionProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__display__" + name), text, description);
        }

        public static InstructionProto.InputType FillRightSide(this InstructionProto.InputType type)
        {
            uint uType = (uint)type;

            for (int i = 0; i < 32; i+=4)
            {
                uint a = 0xFU & (uType >> i);
                if (a == 0)
                    uType |= 0xFU << i;
                else
                    break;
            }

            return (InstructionProto.InputType)uType;
        }
    }
}