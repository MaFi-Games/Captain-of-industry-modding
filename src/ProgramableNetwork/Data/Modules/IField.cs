using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;

namespace ProgramableNetwork
{
    public interface IField
    {
        string Name { get; }
        int Size { get; }

        void Init(StackContainer fieldContainer, UiBuilder uiBuilder, Module module, System.Action updateDialog);
    }
}