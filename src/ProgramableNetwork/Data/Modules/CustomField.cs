using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace ProgramableNetwork
{
    internal class CustomField : IField
    {
        private string id;
        private string name;
        private string shortDesc;
        private Action<UiBuilder, StackContainer, Reference, Action> ui;
        private Func<int> size;

        public CustomField(string id, string name, string shortDesc, Func<int> size, Action<UiBuilder, StackContainer, Reference, Action> ui)
        {
            this.id = id;
            this.name = name;
            this.shortDesc = shortDesc;
            this.ui = ui;
            this.size = size;
        }

        public string Name => name;

        public int Size => size();

        public void Init(ControllerInspector inspector, WindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            ui.Invoke(uiBuilder, fieldContainer, new Reference((v) => module.Field[id] = v, () => module.Field[id, 0]), updateDialog);
        }

        public void Validate(Module module)
        {
            // do nothing
        }
    }
}