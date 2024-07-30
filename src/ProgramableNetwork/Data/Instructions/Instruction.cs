using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProgramableNetwork
{
    public class Instruction
    {
        private readonly long uniqueId;
        private readonly MemoryPointer[] m_inputs;
        private readonly MemoryPointer[] m_displays;
        private LocStrFormatted? Error;

        private InstructionProto.ID m_prototypeId;
        private InstructionProto m_prototype;
        public InstructionProto Prototype {
            get {
                if (m_prototype == null)
                    m_prototype = Context.ProtosDb.Get<InstructionProto>(m_prototypeId).ValueOrThrow("Invalid instruction prototype");
                return m_prototype;
            }
        }

        public EntityContext Context { get; private set; }

        public void Recontext(EntityContext context)
        {
            Context = context;
            foreach (var item in m_inputs)
                item.Recontext(context);
            foreach (var item in m_displays)
                item.Recontext(context);
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="operationProto"></param>
        /// <param name="inputs"></param>
        public Instruction(long uniqueId, InstructionProto operationProto, MemoryPointer[] inputs, MemoryPointer[] displays)
        {
            this.uniqueId = uniqueId;
            this.m_prototype = operationProto;
            this.m_prototypeId = operationProto.Id;
            this.m_inputs = new MemoryPointer[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                this.m_inputs[i] = inputs[i].Clone();
            this.m_displays = new MemoryPointer[displays.Length];
            for (int i = 0; i < displays.Length; i++)
                this.m_displays[i] = displays[i].Clone();
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="operationProto"></param>
        /// <param name="inputs"></param>
        public Instruction(long uniqueId, InstructionProto.ID operationProto, MemoryPointer[] inputs, MemoryPointer[] displays)
        {
            this.uniqueId = uniqueId;
            this.m_prototypeId = operationProto;
            this.m_inputs = new MemoryPointer[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                this.m_inputs[i] = inputs[i].Clone();
            this.m_displays = new MemoryPointer[displays.Length];
            for (int i = 0; i < displays.Length; i++)
                this.m_displays[i] = displays[i].Clone();
        }

        /// <summary>
        /// Init constructor
        /// </summary>
        private Instruction(InstructionProto.ID operationProto, int pointers)
        {
            this.uniqueId = DateTime.UtcNow.Ticks;
            this.m_prototypeId = operationProto;
            this.m_inputs = new MemoryPointer[pointers];

            for (int i = 0; i < m_inputs.Length; i++)
                m_inputs[i] = new MemoryPointer();

            this.m_displays = new MemoryPointer[0];
        }

        public static Instruction Invalid(EntityContext context = null)
        {
            Instruction instruction = new Instruction(NewIds.Instructions.Invalid, 0);
            if (context != null) instruction.Recontext(context);
            return instruction;
        }

        /// <summary>
        /// Init constructor
        /// </summary>
        public Instruction(InstructionProto operationProto, EntityContext context)
        {
            this.uniqueId = DateTime.UtcNow.Ticks;
            this.m_prototype = operationProto;
            this.m_prototypeId = operationProto.Id;
            this.m_inputs = new MemoryPointer[Prototype.Inputs.Length];
            this.m_displays = new MemoryPointer[Prototype.Displays.Length];

            for (int i = 0; i < m_inputs.Length; i++)
            {
                m_inputs[i] = new MemoryPointer();
                m_inputs[i].Recontext(context);
            }

            for (int i = 0; i < m_displays.Length; i++)
            {
                m_displays[i] = new MemoryPointer();
                m_displays[i].Recontext(context);
            }
        }

        public long UniqueId => uniqueId;

        public MemoryPointer[] Inputs => m_inputs;
        public MemoryPointer[] Displays => m_displays;

        public void SetError(LocStrFormatted? error)
        {
            this.Error = error;
        }

        public int GetLength()
        {
            return Prototype.InstructionCost + m_inputs.Select(i => i.Type.GetInstructionCost()).Sum();
        }

        public void Run(Program m_program)
        {
            m_program.SetInputs(m_inputs);
            m_program.SetDisplays(m_displays);
            Prototype.Runtime(m_program);
        }

        public Instruction Clone()
        {
            return new Instruction(DateTime.UtcNow.Ticks, Prototype, m_inputs, m_displays);
        }

        public static bool operator ==(Instruction a, Instruction b)
        {
            return a?.uniqueId == b?.uniqueId;
        }

        public static bool operator !=(Instruction a, Instruction b)
        {
            return a?.uniqueId != b?.uniqueId;
        }

        private static readonly int SerializerVersion = 0;
        public void SerializeData(BlobWriter writer)
        {
            writer.WriteString(Prototype.Id.Value);
            writer.WriteLong(UniqueId);
            writer.WriteInt(SerializerVersion);

            writer.WriteInt(m_inputs.Length);
            foreach (var input in m_inputs)
            {
                writer.WriteUInt((uint)input.Type);
                writer.WriteLong(input.Data);
                writer.WriteString(input.SData ?? "");
            }

            writer.WriteInt(m_displays.Length);
            foreach (var display in m_displays)
            {
                writer.WriteUInt((uint)display.Type);
                writer.WriteLong(display.Data);
                writer.WriteString(display.SData ?? "");
            }
        }

        public static bool Deserialize(BlobReader reader, out Instruction instruction)
        {
            try
            {
                InstructionProto.ID id = new InstructionProto.ID(reader.ReadString());
                long uniqueId = reader.ReadLong();
                int serializerVersion = reader.ReadInt(); // TODO when change

                MemoryPointer[] m_inputs = new MemoryPointer[reader.ReadInt()];
                for (int i = 0; i < m_inputs.Length; i++)
                {
                    MemoryPointer input = m_inputs[i] = new MemoryPointer();
                    input.Type = (InstructionProto.InputType)reader.ReadUInt();
                    input.Data = reader.ReadLong();
                    input.SData = reader.ReadString();
                }

                MemoryPointer[] m_displays = new MemoryPointer[reader.ReadInt()];
                for (int i = 0; i < m_displays.Length; i++)
                {
                    MemoryPointer display = m_displays[i] = new MemoryPointer();
                    display.Type = (InstructionProto.InputType)reader.ReadUInt();
                    display.Data = reader.ReadLong();
                    display.SData = reader.ReadString();
                }

                instruction = new Instruction(uniqueId, id, m_inputs, m_displays);
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Deserialization error");
                Log.Exception(e);
                instruction = null;
                return false;
            }
        }
    }
}