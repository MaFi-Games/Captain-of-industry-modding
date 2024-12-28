using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace ProgramableNetwork
{
    public class StringField : IField
    {
        private string id;
        private string name;
        public string Default { get; }

        public StringField(string id, string name, string defaultValue)
        {
            this.id = id;
            this.name = name;
            this.Default = defaultValue;
        }

        public string Name => name;

        public int Size => 20;

        public void Init(ControllerInspector inspector, WindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(20);

            uiBuilder
                .NewTxt("name")
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(20)
                .SetText(Name)
                .AppendTo(fieldContainer);

            var numberEditor = uiBuilder
                .NewTxtField("value")
                .SetParent(fieldContainer, true)
                .SetWidth(200)
                .SetHeight(20)
                .AppendTo(fieldContainer);

            numberEditor.SetOnValueChangedAction(() =>
            {
                module.StringData[Name] = numberEditor.GetText().ToString();
            });
            
            if (module.StringData.TryGetValue(Name, out var num))
            {
                numberEditor.SetText(num);
            }
            else
            {
                numberEditor.SetText(Default.ToString());
                module.StringData[Name] = Default.ToString();
            }
        }
    }
}