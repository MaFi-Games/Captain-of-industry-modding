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
    public class BooleanTab : StackContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;

        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private readonly ToggleBtn tglTrue;
        private readonly ToggleBtn tglFalse;
        private bool m_refresh;

        public BooleanTab(UiBuilder builder, UiStyle style, MemoryPointer input, Action refresh)
            : base(builder, "boolean_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;

            this.SetSize(100, 20);

            tglTrue = Builder
                .NewToggleBtn("toggle-true_" + DateTime.Now.Ticks)
                .SetText(NewIds.Texts.Boolean[true].TranslatedString)
                .SetSize(100, 20)
                .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                .SetIsOn(!m_input.IsNull && m_input.Type == InstructionProto.InputType.Boolean && m_input.Boolean)
                .SetOnToggleAction(active =>
                {
                    if (m_refresh) return;

                    m_input.Type = InstructionProto.InputType.Boolean;
                    m_input.Boolean = active;

                    tglTrue.SetIsOn(m_input.Boolean);
                    tglFalse.SetIsOn(!m_input.Boolean);
                })
                .AppendTo(this);

            tglFalse = Builder
                .NewToggleBtn("toggle-false_" + DateTime.Now.Ticks)
                .SetText(NewIds.Texts.Boolean[false].TranslatedString)
                .SetSize(100, 20)
                .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                .SetIsOn(!m_input.IsNull && m_input.Type == InstructionProto.InputType.Boolean && m_input.Boolean)
                .SetOnToggleAction(active =>
                {
                    if (m_refresh) return;

                    m_input.Type = InstructionProto.InputType.Boolean;
                    m_input.Boolean = !active;

                    tglTrue.SetIsOn(m_input.Boolean);
                    tglFalse.SetIsOn(!m_input.Boolean);

                    refresh();
                })
                .AppendTo(this);

            if (m_input.Type == InstructionProto.InputType.Boolean)
            {
                tglTrue.SetIsOn(m_input.Boolean);
                tglFalse.SetIsOn(!m_input.Boolean);
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.Boolean)
            {
                m_refresh = true;
                tglTrue.SetIsOn(false);
                tglFalse.SetIsOn(false);
                m_refresh = false;
            }
        }
    }
}