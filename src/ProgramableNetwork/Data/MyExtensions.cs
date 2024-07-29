using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;

namespace ProgramableNetwork
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static T AppendTo<T>(this T element, StackContainer stackContainer) where T : IUiElement
        {
            stackContainer.Append(element, element.GetSize(), default, default, false);
            return element;
        }

        public static T AddToGrid<T>(this T element, GridContainer gridContainer) where T : IUiElement
        {
            element.SetParent(gridContainer);
            gridContainer.Append(element);
            return element;
        }

        public static T SetSize<T>(this T element, int x, int y) where T : IUiElement
        {
            return element.SetSize(new UnityEngine.Vector2(x, y));
        }
    }
}
