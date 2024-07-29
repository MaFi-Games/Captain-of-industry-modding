using Mafi.Base;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class InstructionTab : GridContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;
        private readonly List<ToggleBtn> allButton;

        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private bool m_setting;

        public InstructionTab(UiBuilder builder, UiStyle style, MemoryPointer input, List<Instruction> instructions, Action refresh)
            : base(builder, "instruction_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;

            SetCellSize(new UnityEngine.Vector2(30, 20));
            SetCellSpacing(5);
            SetDynamicHeightMode(10);

            allButton = new List<ToggleBtn>();

            for (int i = 0; i < instructions.Count; i++)
            {
                int o = i;

                var btn = Builder
                    .NewToggleBtn("item-" + i + "_" + DateTime.Now.Ticks)
                    .SetText($"{o:D3}")
                    .SetSize(30, 20)
                    .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                    .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                    .AppendTo(this);

                allButton.Add(btn);

                btn.SetIsOn(m_input.Type == InstructionProto.InputType.Instruction && m_input.Instruction == instructions[o].UniqueId);

                btn.SetOnToggleAction(active =>
                {
                    if (m_setting) return;
                    m_setting = true;
                    if (active)
                    {
                        foreach (var otherBtn in allButton)
                            otherBtn.SetIsOn(otherBtn == btn);

                        m_input.Type = InstructionProto.InputType.Instruction;
                        m_input.Instruction = instructions[o].UniqueId;
                        refresh();
                    }
                    m_setting = false;
                });
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.Instruction)
                foreach (var otherBtn in allButton)
                    otherBtn.SetIsOn(false);
        }
    }
}