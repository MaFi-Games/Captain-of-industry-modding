using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace ProgramableNetwork
{
    public class StringField : IField
    {
        public string Id { get; }
        private string name;
        public string Default { get; }

        public StringField(string id, string name, string defaultValue)
        {
            this.Id = id;
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
                module.Field[Id] = numberEditor.GetText().ToString();
            });

            string value = module.Field[Id, Default];
            numberEditor.SetText(value);
            module.Field[Id] = value;
        }

        public void Validate(Module module)
        {
            // nothing to do
        }
    }
}