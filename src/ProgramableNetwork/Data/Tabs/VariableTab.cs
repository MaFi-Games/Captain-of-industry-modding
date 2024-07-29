using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class VariableTab : GridContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;
        private readonly Computer m_computer;

        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private readonly List<ToggleBtn> allButton;
        private bool m_isSetting;

        public VariableTab(UiBuilder builder, UiStyle style, Computer computer, MemoryPointer input, Action refresh)
            : base(builder, "variable_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;
            this.m_computer = computer;

            SetCellSize(new UnityEngine.Vector2(20, 20));
            SetDynamicHeightMode(10);

            allButton = new List<ToggleBtn>();

            for (int i = 0; i < m_computer.Prototype.Variables; i++)
            {
                int o = i;

                var btn = Builder
                    .NewToggleBtn("item-" + i + "_" + DateTime.Now.Ticks)
                    .SetText(o.ToString())
                    .SetSize(20, 20)
                    .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                    .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                    .AppendTo(this);

                if (m_input.Type == InstructionProto.InputType.Variable && m_input.Variable == o)
                    btn.SetIsOn(true);

                btn.SetOnToggleAction(active =>
                {
                    if (m_isSetting) return;
                    m_isSetting = true;

                    if (active)
                    {
                        foreach (var otherBtn in allButton)
                            otherBtn.SetIsOn(otherBtn == btn);

                        m_input.Variable = new VariableIdx(o);
                        refresh();
                    }
                    else
                    {   // unset
                        m_input.None();
                        refresh();
                    }

                    m_isSetting = false;
                });

                allButton.Add(btn);
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.Variable)
                foreach (var otherBtn in allButton)
                    otherBtn.SetIsOn(false);
        }
    }
}