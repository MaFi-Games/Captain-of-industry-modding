using Mafi.Localization;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class MyTabContainer : StackContainer
    {
        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private readonly Action m_refresh;
        private readonly List<IUiElement> m_tabs;
        private readonly List<ToggleBtn> m_buttons;
        private readonly StackContainer m_tabsHolder;

        public MyTabContainer(UiBuilder builder, UiStyle style, string name, Action refresh)
            : base(builder, name)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_refresh = refresh;
            this.m_tabs = new List<IUiElement>();
            this.m_buttons = new List<ToggleBtn>();

            SetBackground(Mafi.ColorRgba.DarkDarkGray);
            SetStackingDirection(Direction.TopToBottom);
            SetSizeMode(SizeMode.Dynamic);

            this.m_tabsHolder = Builder
                .NewStackContainer("tabs")
                .SetStackingDirection(Direction.LeftToRight)
                .SetSizeMode(SizeMode.Dynamic)
                .SetHeight(20)
                .AppendTo(this);
        }

        public void AddTab(LocStrFormatted locStr, IUiElement tab)
        {
            var tabButton = Builder
                .NewToggleBtn(locStr.Value)
                .SetHeight(20)
                .SetText(locStr.Value)
                .SetButtonStyleWhenOff(Style.Global.GeneralBtnToToggle)
                .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                .AppendTo(m_tabsHolder);

            m_buttons.Add(tabButton);
            tabButton.SetIsOn(m_buttons.Count == 1);

            tabButton
                .SetOnToggleAction(active =>
                {
                    if (!active) return;
                    for (int i = 0; i < m_buttons.Count; i++)
                    {
                        ToggleBtn button = m_buttons[i];
                        IUiElement atab = m_tabs[i];

                        bool current = button == tabButton;
                        if (current)
                            this.ShowItem(atab);
                        else
                            this.HideItem(atab);
                        button.SetIsOn(current);
                    }
                });

            m_tabs.Add(tab);

            tab.AppendTo(this);
            if (m_buttons.Count != 1)
                this.HideItem(tab);
        }

        public void SwitchToTab(IUiElement variableTab)
        {
            for (int i = 0; i < m_tabs.Count; i++)
            {
                bool isThat = m_tabs[i] == variableTab;
                m_buttons[i].SetIsOn(isThat);
                SetItemVisibility(m_tabs[i], isThat);
            }
        }
    }
}