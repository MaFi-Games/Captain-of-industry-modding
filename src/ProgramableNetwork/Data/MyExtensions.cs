using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;

namespace ProgramableNetwork
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static T AppendTo<T>(this T element, StackContainer stackContainer, ContainerPosition? containerPosition) where T : IUiElement
        {
            stackContainer.Append(element, element.GetSize(), containerPosition, default, false);
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


        public static T DynamicSizeListener<T, S>(this T element, S container, Dynamic dynamicAxis)
            where T : IDynamicSizeElement
            where S : StackContainer
        {
            element.SizeChanged += (item) => {
                try {
                    container.UpdateItemSize(item, item.GetSize());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to update size of element: {item.GameObject?.name ?? "<no-name>"}" +
                        $" to container: {container.GameObject?.name ?? "<no-name>"}");
                    Console.WriteLine(e);
                }
             };
            return element;
        }

        public static StackContainer AllignRight<T>(this T element, UiBuilder builder) where T : IUiElement
        {
            return builder.NewStackContainer(element.GameObject.name + "_invert")
                .SetStackingDirection(StackContainer.Direction.RightToLeft)
                .SetSizeMode(StackContainer.SizeMode.Dynamic);
        }

        public static string GetIcon(this IEntity entity)
        {
            if (entity is LayoutEntityBase positionedForGraphics)
                return positionedForGraphics.Prototype.Graphics.IconPath;

            else if (entity is DynamicGroundEntity dynamicForGraphics)
                return dynamicForGraphics.Prototype.Graphics.IconPath;

            else if (entity is Transport transportForGraphics)
                return transportForGraphics.Prototype.Graphics.IconPath;

            else
                return new IconsPaths().Empty;
        }

        public static bool HasPosition(this IEntity entity, out Tile3f position)
        {
            if (entity is IEntityWithPosition positioned)
            {
                position = positioned.Position3f;
                return true;
            }

            if (entity is DynamicGroundEntity dynamic)
            {
                position = dynamic.Position3f;
                return true;
            }

            position = Tile3f.Zero;
            return false;
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

        public static string SerializationInfo(this IEntity entity, Controller computer)
        {
            if (!entity.HasPosition(out Tile3f position))
                return "";

            RelTile3f offset = position - computer.Position3f;
            return $"{entity.Prototype.Id.Value}:{offset}";
        }

        public static string ModuleId(this string id)
        {
            return "ProgramableNetwork_Module_" + id;
        }
    }
}
