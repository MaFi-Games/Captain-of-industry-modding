using Mafi.Base;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProgramableNetwork
{
    public class IntegerTab : StackContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;
        private readonly Action m_refreshAction;
        private readonly TxtField m_field;
        private readonly Btn m_apply;
        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private Regex m_numberFilter;

        public IntegerTab(UiBuilder builder, UiStyle style, MemoryPointer input, Action refresh)
            : base(builder, "number_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;
            this.m_refreshAction = refresh;
            m_numberFilter = new Regex(@"^-?[0-9]+$", RegexOptions.Compiled);

            this.m_field = builder
                .NewTxtField("number_input_" + DateTime.Now.Ticks)
                .SetText("0")
                .SetOnValueChangedAction(TextChanged)
                .SetHeight(30)
                .AppendTo(this);

            this.m_apply = builder
                .NewBtnGeneral("number_apply_" + DateTime.Now.Ticks)
                .SetText(NewIds.Texts.Tools.Apply)
                .SetEnabled(false)
                .SetHeight(30)
                .OnClick(SetValue)
                .AppendTo(this);

            SetStackingDirection(Direction.LeftToRight);
            this.SetHeight(30);

            if (input.Type == InstructionProto.InputType.Integer)
            {
                m_field.SetText(input.Integer.ToString());
            }
        }

        private void TextChanged()
        {
            bool hasValue = m_input.Type == InstructionProto.InputType.Integer;
            int iValue = hasValue ? m_input.Integer : 0;
            string sValue = m_field.GetText();

            if (hasValue && iValue.ToString() == sValue)
            {
                m_apply.SetEnabled(false);
                m_apply.SetButtonStyle(Style.Global.GeneralBtn);
            }
            else if (m_numberFilter.IsMatch(sValue))
            {
                m_apply.SetEnabled(true);
                m_apply.SetButtonStyle(Style.Global.GeneralBtn);
            }
            else
            {
                m_apply.SetEnabled(false);
                m_apply.SetButtonStyle(Style.Global.DangerBtn);
            }
        }

        private void SetValue()
        {
            if (int.TryParse(m_field.GetText(), out int integer))
            {
                m_input.Integer = integer;
                m_apply.SetEnabled(false);
                m_refreshAction();
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.Integer)
            {
                m_field.SetText("0");
            }
        }
    }
}