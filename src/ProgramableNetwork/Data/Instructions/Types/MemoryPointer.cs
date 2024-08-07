using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Serialization;
using System;
using System.Text.RegularExpressions;

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

        public void Recontext(Computer computer)
        {
            Context = computer.Context;
        }

        public void ValidateEntity(Computer computer)
        {
            if (Data != -1 && !Context.EntitiesManager.TryGetEntity(new Mafi.Core.EntityId((int)Data), out Entity _))
            {
                return;
            }

            if (SData?.Length == 0)
            {
                this.Type = InstructionProto.InputType.None;
                return;
            }

            try
            {
                Regex r = new Regex(@"^\w+:\((-?\d+(?:.\d+)?), *(-?\d+(?:.\d+)?), *(-?\d+(?:.\d+)?)\).*");
                Match m = r.Match(this.SData);
                Tile3f toBeFound = computer.Position3f + new RelTile3f(
                    float.Parse(m.Groups[1].Value).ToFix32(),
                    float.Parse(m.Groups[2].Value).ToFix32(),
                    float.Parse(m.Groups[3].Value).ToFix32()
                );

                foreach (IEntity e in computer.Context.EntitiesManager.Entities)
                {
                    if (e.HasPosition(out Tile3f newPos) && newPos == toBeFound)
                    {
                        this.Data = e.Id.Value;
                        return;
                    }
                }

                // when not found
                this.Type = InstructionProto.InputType.None;
            }
            catch (Exception e)
            {
                this.Type = InstructionProto.InputType.None;
                Log.Error($"Cannot get entity: {SData}");
                Log.Exception(e);
            }
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

        public static implicit operator Entity(MemoryPointer pointer)
        {
            if (pointer.Type != InstructionProto.InputType.Entity)
                throw new ProgramException(NewIds.Texts.InvalidPointerType.Format(
                        NewIds.Texts.PointerTypes[InstructionProto.InputType.Entity],
                        NewIds.Texts.PointerTypes[pointer.Type]
                    ));
            return (Entity)pointer.Entity;
        }

        internal void SerializeData(BlobWriter writer, bool clearEntities)
        {
            writer.WriteUInt((uint)Type);
            writer.WriteLong(
                clearEntities && Type == InstructionProto.InputType.Entity
                    ? -1
                    : Data);
            writer.WriteString(SData ?? "");
        }

        internal static MemoryPointer Deserialize(BlobReader reader)
        {
            var input = new MemoryPointer();
            input.Type = (InstructionProto.InputType)reader.ReadUInt();
            input.Data = reader.ReadLong();
            input.SData = reader.ReadString();
            return input;
        }
    }
}