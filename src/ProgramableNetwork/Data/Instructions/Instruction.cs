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

        public EntityContext Context { get; set; }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="operationProto"></param>
        /// <param name="inputs"></param>
        public Instruction(long uniqueId, InstructionProto operationProto, MemoryPointer[] inputs)
        {
            this.uniqueId = uniqueId;
            this.m_prototype = operationProto;
            this.m_prototypeId = operationProto.Id;
            this.m_inputs = new MemoryPointer[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                this.m_inputs[i] = inputs[i].Clone();
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="operationProto"></param>
        /// <param name="inputs"></param>
        public Instruction(long uniqueId, InstructionProto.ID operationProto, MemoryPointer[] inputs)
        {
            this.uniqueId = uniqueId;
            this.m_prototypeId = operationProto;
            this.m_inputs = new MemoryPointer[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                this.m_inputs[i] = inputs[i].Clone();
        }

        /// <summary>
        /// Init constructor
        /// </summary>
        public Instruction(InstructionProto.ID operationProto, int pointers)
        {
            this.uniqueId = DateTime.UtcNow.Ticks;
            this.m_prototypeId = operationProto;
            this.m_inputs = new MemoryPointer[pointers];

            for (int i = 0; i < m_inputs.Length; i++)
                m_inputs[i] = new MemoryPointer();
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

            for (int i = 0; i < m_inputs.Length; i++)
            {
                m_inputs[i] = new MemoryPointer();
                m_inputs[i].Context = context;
            }
        }

        public long UniqueId => uniqueId;

        public MemoryPointer[] Inputs => m_inputs;

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
            Prototype.Runtime(m_program);
        }

        public Instruction Clone()
        {
            return new Instruction(DateTime.UtcNow.Ticks, Prototype, m_inputs);
        }

        public static bool operator ==(Instruction a, Instruction b)
        {
            return a?.uniqueId == b?.uniqueId;
        }

        public static bool operator !=(Instruction a, Instruction b)
        {
            return a?.uniqueId != b?.uniqueId;
        }

        public void SerializeData(BlobWriter writer)
        {
            writer.WriteString(Prototype.Id.Value);
            writer.WriteLong(UniqueId);

            writer.WriteInt(m_inputs.Length);
            foreach (var input in m_inputs)
            {
                writer.WriteUInt((uint)input.Type);
                writer.WriteLong(input.Data);
            }
        }

        public static bool Deserialize(BlobReader reader, out Instruction instruction)
        {
            try
            {
                InstructionProto.ID id = new InstructionProto.ID(reader.ReadString());
                long uniqueId = reader.ReadLong();

                MemoryPointer[] m_inputs = new MemoryPointer[reader.ReadInt()];
                for (int i = 0; i < m_inputs.Length; i++)
                {
                    MemoryPointer input = m_inputs[i] = new MemoryPointer();
                    input.Type = (InstructionProto.InputType)reader.ReadUInt();
                    input.Data = reader.ReadLong();
                }

                instruction = new Instruction(uniqueId, id, m_inputs);
                return true;
            }
            catch (Exception)
            {
                instruction = null;
                return false;
            }
        }
    }
}