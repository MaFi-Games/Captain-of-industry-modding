using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace ProgramableNetwork
{
    public class NumberField : IField
    {
        public NumberField(string id, string name, int defaultValue)
        {
            Id = id;
            Name = name;
            Default = defaultValue;
        }

        public string Id { get; }
        public string Name { get; }

        public int Size => 20;

        public int Default { get; }

        public void Init(StackContainer fieldContainer, UiBuilder uiBuider, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(20);

            uiBuider
                .NewTxt("name")
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(20)
                .SetText(Name)
                .AppendTo(fieldContainer);

            var numberEditor = uiBuider
                .NewTxtField("value")
                .SetParent(fieldContainer, true)
                .SetWidth(200)
                .SetHeight(20)
                .SetText("0")
                .AppendTo(fieldContainer);

            numberEditor.SetOnValueChangedAction(() =>
            {
                if (int.TryParse(numberEditor.GetText(), out int value))
                {
                    module.NumberData[Name] = value;
                }
            });
            numberEditor.SetText(
                module.NumberData.TryGetValue(Name, out var num)
                    ? num.ToString()
                    : Default.ToString()
            );
        }
    }
}