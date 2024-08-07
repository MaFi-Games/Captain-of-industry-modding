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
        private Computer m_computer;
        private LocStrFormatted? Error;

        private InstructionProto.ID m_prototypeId;
        private InstructionProto m_prototype;
        public InstructionProto Prototype {
            get {
                if (m_prototype == null)
                    m_prototype = m_computer.Context.ProtosDb.Get<InstructionProto>(m_prototypeId).ValueOrThrow("Invalid instruction prototype");
                return m_prototype;
            }
        }

        public void Recontext(Computer computer)
        {
            m_computer = computer;
            foreach (var item in m_inputs)
                item.Recontext(computer);
            foreach (var item in m_displays)
                item.Recontext(computer);
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

        public void ValidateEntities(Computer computer)
        {
            foreach (MemoryPointer mp in m_inputs)
            {
                if (mp.Type == InstructionProto.InputType.Entity)
                {
                    mp.ValidateEntity(computer);
                }
            }
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

        public static Instruction Invalid(Computer computer)
        {
            Instruction instruction = new Instruction(NewIds.Instructions.Invalid, 0);
            instruction.Recontext(computer);
            return instruction;
        }

        public static Instruction Invalid()
        {
            return new Instruction(NewIds.Instructions.Invalid, 0);
        }

        /// <summary>
        /// Init constructor
        /// </summary>
        public Instruction(InstructionProto operationProto, Computer computer)
        {
            this.uniqueId = DateTime.UtcNow.Ticks;
            this.m_prototype = operationProto;
            this.m_prototypeId = operationProto.Id;
            this.m_inputs = new MemoryPointer[Prototype.Inputs.Length];
            this.m_displays = new MemoryPointer[Prototype.Displays.Length];
            this.m_computer = computer;

            for (int i = 0; i < m_inputs.Length; i++)
            {
                m_inputs[i] = new MemoryPointer();
                m_inputs[i].Recontext(computer);
            }

            for (int i = 0; i < m_displays.Length; i++)
            {
                m_displays[i] = new MemoryPointer();
                m_displays[i].Recontext(computer);
            }
        }

        public long UniqueId => uniqueId;

        public MemoryPointer[] Inputs => m_inputs;
        public MemoryPointer[] Displays => m_displays;

        public string Note { get; set; }

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

        private static readonly int SerializerVersion = 1;
        public void SerializeData(BlobWriter writer, bool clearEntities = false)
        {
            writer.WriteString(Prototype.Id.Value);
            writer.WriteLong(UniqueId);
            writer.WriteInt(SerializerVersion);

            writer.WriteInt(m_inputs.Length);
            foreach (var input in m_inputs)
            {
                input.SerializeData(writer, clearEntities);
            }

            writer.WriteInt(m_displays.Length);
            for (int i = 0; i < m_displays.Length; i++)
            {
                new MemoryPointer().SerializeData(writer, clearEntities);
            }

            // a programmers note for user
            writer.WriteString(Note ?? "");
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
                    m_inputs[i] = MemoryPointer.Deserialize(reader);
                }

                MemoryPointer[] m_displays = new MemoryPointer[reader.ReadInt()];
                for (int i = 0; i < m_displays.Length; i++)
                {
                    m_displays[i] = MemoryPointer.Deserialize(reader);
                }

                instruction = new Instruction(uniqueId, id, m_inputs, m_displays);
                if (serializerVersion > 0)
                {
                    instruction.Note = reader.ReadString();
                }
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