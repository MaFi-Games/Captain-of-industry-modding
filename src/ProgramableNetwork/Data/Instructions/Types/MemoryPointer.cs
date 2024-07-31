using Mafi.Core.Entities;
using Mafi.Core.Products;
using System;

namespace ProgramableNetwork
{
    public class MemoryPointer
    {
        public EntityContext Context { get; private set; }

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
        public ProductProto Product {
            get => Context.ProtosDb.Get<ProductProto>(new Mafi.Core.Prototypes.Proto.ID(SData)).ValueOrNull;
            set {
                Type = InstructionProto.InputType.Product;
                SData = value.Id.Value;
            }
        }

        public long Instruction {
            get => Data;
            set {
                Type = InstructionProto.InputType.Instruction;
                Data = value;
            }
        }
        public long Data { get; set; }
        public string SData { get; set; }

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
                SData = SData,
            };
        }

        public void Assign(MemoryPointer value)
        {
            this.Type = value.Type;
            this.Data = value.Data;
            this.SData = value.SData;
        }

        public void Recontext(EntityContext context)
        {
            Context = context;
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

        public static implicit operator ProductProto(MemoryPointer pointer)
        {
            if (pointer.Type != InstructionProto.InputType.Product)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                        NewIds.Texts.PointerTypes[InstructionProto.InputType.Product],
                        NewIds.Texts.PointerTypes[pointer.Type]
                    ));
            return pointer.Product;
        }
    }
}