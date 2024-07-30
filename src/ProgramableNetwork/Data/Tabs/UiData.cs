using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class UiData : IRefreshable
    {
        public readonly UiBuilder Builder;
        public readonly UiStyle Style;
        public readonly MemoryPointer Input;
        public readonly int InputIndex;
        public readonly InstructionProto Instruction;
        public readonly Computer Computer;
        public readonly InspectorContext Context;

        private readonly List<IRefreshable> m_refreshables;

        public UiData(UiBuilder builder, UiStyle style, InstructionProto instruction, Computer computer, InspectorContext context)
            : this(builder, style, instruction, null, -1, computer, context, new List<IRefreshable>())
        {
        }

        private UiData(UiBuilder builder, UiStyle style, InstructionProto instruction, MemoryPointer input, int index, Computer computer, InspectorContext context, List<IRefreshable> refreshables)
        {
            Builder = builder;
            Style = style;
            Input = input;
            InputIndex = index;
            Instruction = instruction;
            Computer = computer;
            Context = context;
            m_refreshables = refreshables;
        }

        public UiData ForInput(int index, MemoryPointer input)
        {
            return new UiData(Builder, Style, Instruction, input, index, Computer, Context, m_refreshables);
        }

        public void AddToRefreshable(IRefreshable displayTab)
        {
            m_refreshables.Add(displayTab);
        }

        public void Refresh()
        {
            foreach (var item in m_refreshables)
                item.Refresh();
        }
    }
}