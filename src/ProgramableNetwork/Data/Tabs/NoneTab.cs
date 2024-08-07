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
    public class NoneTab : StackContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;

        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private readonly ToggleBtn tglNone;
        private bool m_refresh;

        public NoneTab(UiBuilder builder, UiStyle style, MemoryPointer input, Action refresh)
            : base(builder, "none_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;

            this.SetSize(200, 20);
            SetSizeMode(SizeMode.Dynamic);

            tglNone = Builder
                .NewToggleBtn("toggle-none_" + DateTime.Now.Ticks)
                .SetText(NewIds.Texts.Boolean[true].TranslatedString)
                .SetSize(100, 20)
                .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                .SetIsOn(!m_input.IsNull && m_input.Type == InstructionProto.InputType.Boolean && m_input.Boolean)
                .SetOnToggleAction(active =>
                {
                    if (m_refresh) return;

                    m_input.Type = InstructionProto.InputType.None;
                    refresh();
                })
                .AppendTo(this);

            if (m_input.Type == InstructionProto.InputType.None)
            {
                tglNone.SetIsOn(m_input.Boolean);
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.None)
            {
                m_refresh = true;
                tglNone.SetIsOn(false);
                m_refresh = false;
            }
        }
    }
}