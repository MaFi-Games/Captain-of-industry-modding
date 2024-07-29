using Mafi.Core.Entities;
using System;

namespace ProgramableNetwork
{
    public class MemoryPointer
    {
        public EntityContext Context { get; set; }

        public InstructionProto.InputType Type { get; set; }
        public bool IsNull => Type == InstructionProto.InputType.None;

        public IEntity Entity {
            get =>
                Context.EntitiesManager.TryGetEntity(new Mafi.Core.EntityId((int)Data), out Entity entity) ? entity : null;
            set
            {
                Type = InstructionProto.InputType.Entity;
                Data = value.Id.Value;
            }
        }
        public bool Boolean {
            get => Data != 0;
            set {
                Type = InstructionProto.InputType.Boolean;
                Data = value ? 1 : 0;
            }
        }
        public int Integer { get => (int)Data; set
            {
                Type = InstructionProto.InputType.Integer;
                Data = value;
            }
        }
        public VariableIdx Variable {
            get => new VariableIdx((int)Data);
            set {
                Type = InstructionProto.InputType.Variable;
                Data = value.Data;
            }
        }
        public long Instruction { get => Data; set => Data = value; }
        public long Data { get; set; }

        public MemoryPointer()
        {
            Type = InstructionProto.InputType.None;
        }

        public MemoryPointer Clone()
        {
            return new MemoryPointer()
            {
                Context = Context,
                Type = Type,
                Data = Data,
            };
        }

        public void Assign(MemoryPointer value)
        {
            this.Type = value.Type;
            this.Data = value.Data;
        }

        public static implicit operator int(MemoryPointer pointer)
        {
            if (pointer.Type != InstructionProto.InputType.Integer)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                        NewIds.Texts.PointerTypes[InstructionProto.InputType.Integer],
                        NewIds.Texts.PointerTypes[pointer.Type]
                    ));

            return pointer.Integer;
        }

        public static implicit operator VariableIdx(MemoryPointer pointer)
        {
            if (pointer.Type != InstructionProto.InputType.Variable)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                        NewIds.Texts.PointerTypes[InstructionProto.InputType.Variable],
                        NewIds.Texts.PointerTypes[pointer.Type]
                    ));

            return pointer.Variable;
        }

        public void None()
        {
            Type = InstructionProto.InputType.None;
        }

        public static implicit operator bool(MemoryPointer pointer)
        {
            if (pointer.Type != InstructionProto.InputType.Boolean)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                        NewIds.Texts.PointerTypes[InstructionProto.InputType.Boolean],
                        NewIds.Texts.PointerTypes[pointer.Type]
                    ));
            return pointer.Boolean;
        }
    }
}