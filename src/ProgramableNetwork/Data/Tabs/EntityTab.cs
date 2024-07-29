using Mafi;
using Mafi.Base;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class EntityTab : StackContainer, IRefreshable
    {
        private readonly MemoryPointer m_input;
        private readonly InstructionProto.InputType m_type;
        private readonly InspectorContext m_context;

        private readonly UiBuilder Builder;
        private readonly UiStyle Style;
        private readonly List<ToggleBtn> pickable;

        public EntityTab(UiBuilder builder, UiStyle style, Computer computer, InstructionProto.InputType entityType,
            MemoryPointer input, InspectorContext context, Fix32 distance, Action refresh)
            : base(builder, "entity_" + DateTime.Now.Ticks)
        {
            this.Builder = builder;
            this.Style = style;
            this.m_input = input;
            this.m_type = entityType;
            this.m_context = context;
            this.pickable = new List<ToggleBtn>();

            if (computer == null)
                throw new InvalidCastException($"Missing computer entity");

            if (computer.Context == null)
                throw new InvalidCastException($"Missing context of entity");

            var entities = computer.Context.EntitiesManager.GetAllEntitiesOfType<Entity>()
                .Where(searched =>
                {

                    Tile2f position = Tile2f.Zero;

                    if (/*entityType.HasFlag(OperationProto.InputType.StaticEntity)
                    && */searched is IEntityWithPosition positioned)
                        position = positioned.Position2f;

                    else if (/*entityType.HasFlag(OperationProto.InputType.DynamicEntity)
                    && */searched is DynamicGroundEntity dynamic)
                        position = dynamic.Position2f;

                    else
                        return false;

                    if (computer.Position2f.DistanceTo(position) > distance)
                        return false;

                    return true;
                })
                .OrderBy(v => {
                    if (/*entityType.HasFlag(OperationProto.InputType.StaticEntity)
                    && */v is IEntityWithPosition positioned)
                        return positioned.Position2f.DistanceTo(computer.Position2f);

                    else if (/*entityType.HasFlag(OperationProto.InputType.DynamicEntity)
                    && */v is DynamicGroundEntity dynamic)
                        return dynamic.Position2f.DistanceTo(computer.Position2f);

                    return 0;
                })
                .ToList();

            var grid = Builder
                .NewGridContainer("grid_" + DateTime.Now.Ticks)
                .SetHeight(80)
                .SetCellSpacing(5)
                .SetDynamicHeightMode(18)
                .AppendTo(this);

            Log.Debug($"Entities found: {entities.Count}");
            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];

                var layout = Builder
                    .NewStackContainer("item_" + i + "_" + DateTime.Now.Ticks)
                    .SetStackingDirection(Direction.TopToBottom)
                    .SetSize(40, 60)
                    .AppendTo(grid);

                var btnPreview = new Btn(Builder, "value_" + i + "_" + DateTime.Now.Ticks)
                    .SetButtonStyle(Style.Global.ImageBtn)
                    .SetSize(new Vector2(40, 40))
                    .OnClick(
                        () =>
                        {
                            if (entity is IEntityWithPosition positioned)
                                m_context.CameraController.PanTo(positioned.Position2f);

                            if (entity is DynamicGroundEntity dynamic)
                                m_context.CameraController.PanTo(dynamic.Position2f);
                        })
                    .SetOnMouseEnterLeaveActions(
                        () =>
                        {
                            m_context.Highlighter.Highlight(
                                (IRenderedEntity)entity, ColorRgba.DarkYellow);
                        },
                        () =>
                        {
                            m_context.Highlighter.RemoveHighlight(
                                (IRenderedEntity)entity);
                        })
                    .AppendTo(layout);

                if (entity is LayoutEntity positionedForGraphics)
                    btnPreview.SetIcon(positionedForGraphics.Prototype.Graphics.IconPath);

                else if (entity is DynamicGroundEntity dynamicForGraphics)
                    btnPreview.SetIcon(dynamicForGraphics.Prototype.Graphics.IconPath);

                else if (entity is Transport transportForGraphics)
                    btnPreview.SetIcon(transportForGraphics.Prototype.Graphics.IconPath);

                else
                    btnPreview.SetIcon(Style.Icons.Empty);

                var pick = Builder
                    .NewToggleBtn("pick_" + i + "_" + DateTime.Now.Ticks)
                    .SetButtonStyleWhenOn(Style.Global.GeneralBtnActive)
                    .SetButtonStyleWhenOff(Style.Global.GeneralBtn)
                    .SetText(NewIds.Texts.Tools.Pick.TranslatedString)
                    .SetSize(new Vector2(40, 20));
                pick.SetIsOn(m_input.Type == InstructionProto.InputType.Entity
                    && m_input.Entity.Id == entity.Id);
                pick.SetOnToggleAction(
                        active =>
                        {
                            if (active)
                            {
                                if (entity is IEntityWithPosition positioned)
                                {
                                    //m_input.Type = OperationProto.InputType.StaticEntity;
                                    m_input.Type = InstructionProto.InputType.Entity;
                                    m_input.Entity = entity;

                                }

                                else if (entity is DynamicGroundEntity dynamic)
                                {
                                    //m_input.Type = OperationProto.InputType.DynamicEntity;
                                    m_input.Type = InstructionProto.InputType.Entity;
                                    m_input.Entity = entity;
                                }

                                else
                                {
                                    m_input.Type = InstructionProto.InputType.Entity;
                                    m_input.Entity = entity;
                                }

                                foreach (var tgl in pickable)
                                {
                                    tgl.SetIsOn(tgl == pick);
                                }
                                refresh();
                            }
                        })
                    .SetOnMouseEnterLeaveActions(
                        () =>
                        {
                            m_context.Highlighter.Highlight(
                                (IRenderedEntity)entity, ColorRgba.DarkYellow);
                        },
                        () =>
                        {
                            m_context.Highlighter.RemoveHighlight(
                                (IRenderedEntity)entity);
                        })
                    .AppendTo(layout);

                pickable.Add(pick);
            }
        }

        public void Refresh()
        {
            if (m_input.Type != InstructionProto.InputType.Entity)
            //if (!m_input.Type.HasFlag(m_type))
                foreach (var pick in pickable)
                    pick.SetIsOn(false);
        }
    }
}