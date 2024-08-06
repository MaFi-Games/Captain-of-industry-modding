using System;

namespace ProgramableNetwork
{
    public class Selector
    {
        public readonly InstructionProto Instruction;
        public readonly MemoryPointer MemoryPointer;
        public readonly Action Refresh;

        public Selector(InstructionProto instruction, MemoryPointer memoryPointer, System.Action refresh)
        {
            this.Instruction = instruction;
            this.MemoryPointer = memoryPointer;
            this.Refresh = refresh;
        }
    }
}