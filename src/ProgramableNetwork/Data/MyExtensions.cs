using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Style;

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
            gridContainer.Append(element);
            return element;
        }

        public static T SetSize<T>(this T element, int x, int y) where T : IUiElement
        {
            return element.SetSize(new UnityEngine.Vector2(x, y));
        }

        public static string GetIcon(this IEntity entity)
        {
            if (entity is LayoutEntity positionedForGraphics)
                return positionedForGraphics.Prototype.Graphics.IconPath;

            else if (entity is DynamicGroundEntity dynamicForGraphics)
                return dynamicForGraphics.Prototype.Graphics.IconPath;

            else if (entity is Transport transportForGraphics)
                return transportForGraphics.Prototype.Graphics.IconPath;

            else
                return new IconsPaths().Empty;
        }

        public static bool HasPosition(this IEntity entity, out Tile2f position)
        {
            if (entity is IEntityWithPosition positioned)
            {
                position = positioned.Position2f;
                return true;
            }

            if (entity is DynamicGroundEntity dynamic)
            {
                position = dynamic.Position2f;
                return true;
            }

            position = Tile2f.Zero;
            return false;
        }
    }
}
