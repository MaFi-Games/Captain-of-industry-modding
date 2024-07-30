using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class EntityTab : StackContainer, IRefreshable
    {
        private readonly UiBuilder m_builder;
        private readonly Computer m_computer;
        private readonly MemoryPointer m_input;
        private readonly Func<ProductProto, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly InstructionProto m_instruction;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private Btn m_btnPick;
        private EntityPicker m_entityPicker;

        public EntityTab(UiBuilder builder, Computer computer, InstructionProto instruction,
            MemoryPointer input, Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_computer = computer;
            m_input = input;
            m_filter = instruction.ProductFilter;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;
            m_instruction = instruction;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetStackingDirection(Direction.TopToBottom)
                .SetSizeMode(SizeMode.Dynamic)
                .SetSize(40, 60)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .AppendTo(m_btnPreviewHolder);

            m_btnPick = m_builder
                .NewBtnGeneral("item_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                .SetText(NewIds.Texts.Tools.Pick)
                .SetSize(40, 20)
                .OnClick(FindEntity)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(60);

            Refresh();
        }

        private void FindEntity()
        {
            if (m_entityPicker == null)
            {
                m_entityPicker = new EntityPicker(this);
                m_window.SetupInnerWindowWithButton(m_entityPicker, m_btnPreviewHolder, m_btnPreview, () => {
                    try
                    {
                        m_btnPreviewHolder.ClearAndDestroyAll();
                        m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                            .SetSize(40, 40)
                            .SetIcon(m_builder.Style.Icons.Empty)
                            .OnClick(FindEntity)
                            .AppendTo(m_btnPreviewHolder);
                        m_btnPick = m_builder
                            .NewBtnGeneral("item_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                            .SetText(NewIds.Texts.Tools.Pick)
                            .SetSize(40, 20)
                            .OnClick(FindEntity)
                            .AppendTo(m_btnPreviewHolder);
                    }
                    catch (Exception)
                    {
                        // gui issue
                    }
                }, () => { });
                m_window.OnHide += () =>
                {
                    try
                    {
                        m_entityPicker.Hide();
                    }
                    catch (Exception)
                    {
                        // gui issue
                    }
                };
            }

            m_entityPicker.Show();
        }

        public void Refresh()
        {
            if (m_input.Type == InstructionProto.InputType.Entity)
            {
                m_btnPreview.SetIcon(m_input.Entity.GetIcon());
                m_btnPreview.OnClick(
                    () =>
                    {
                        if (m_input.Entity.HasPosition(out Tile2f position))
                            m_inspectorContext.CameraController.PanTo(position);
                    });
                m_btnPreview.SetOnMouseEnterLeaveActions(
                    () =>
                    {
                        m_inspectorContext.Highlighter.Highlight(
                            (IRenderedEntity)m_input.Entity, ColorRgba.DarkYellow);
                    },
                    () =>
                    {
                        m_inspectorContext.Highlighter.RemoveHighlight(
                            (IRenderedEntity)m_input.Entity);
                    });
            }
            else
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnPreview.OnClick(() => { });
                m_btnPreview.SetOnMouseEnterLeaveActions(() => { }, () => { });
            }
        }

        private class EntityPicker : WindowView
        {
            private EntityTab m_entityTab;
            private GridContainer m_grid;

            public EntityPicker(EntityTab productTab)
                :base("product", FooterStyle.Round, false)
            {
                this.m_entityTab = productTab;
            }

            protected override void BuildWindowContent()
            {
                this.m_headerText.SetText(NewIds.Texts.PointerTypes[InstructionProto.InputType.Entity]);

                SetContentSize(500, 460);

                var scroll = Builder
                    .NewScrollableContainer("items-scroll")
                    .AddVerticalScrollbar()
                    .SetSize(500, 460)
                    .PutTo(GetContentPanel());

                GetContentPanel()
                    .SetSize(500, 460)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg);

                m_grid = Builder
                    .NewGridContainer("items-grid")
                    .SetDynamicHeightMode(10)
                    .SetCellSize(new Vector2(40, 60))
                    .SetCellSpacing(10)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg)
                    .SetInnerPadding(Offset.All(5));

                scroll.AddItem(m_grid);

                OnShowStart += RefreshProducts;
            }

            private void RefreshProducts()
            {
                m_grid.ClearAllAndDestroy();

                var entities = m_entityTab.m_computer.Context.EntitiesManager.GetAllEntitiesOfType<Entity>()
                    .Where(m_entityTab.m_instruction.EntityFilter)
                    .Where(searched =>
                    {

                        Tile2f position = Tile2f.Zero;

                        if (searched is IEntityWithPosition positioned)
                            position = positioned.Position2f;

                        else if (searched is DynamicGroundEntity dynamic)
                            position = dynamic.Position2f;

                        else
                            return false;

                        if (m_entityTab.m_computer.Position2f.DistanceTo(position) > m_entityTab.m_instruction.EntitySearchDistance)
                            return false;

                        return true;
                    })
                    .OrderBy(v => {
                        if (v is IEntityWithPosition positioned)
                            return positioned.Position2f.DistanceTo(m_entityTab.m_computer.Position2f);

                        else if (v is DynamicGroundEntity dynamic)
                            return dynamic.Position2f.DistanceTo(m_entityTab.m_computer.Position2f);

                        return 0;
                    })
                    .ToList();

                foreach (var entity in entities)
                {
                    var stack = Builder
                        .NewStackContainer("item_stack_" + entity.Prototype.Strings.Name.Id)
                        .SetSize(40, 60)
                        .SetSizeMode(SizeMode.Dynamic)
                        .SetStackingDirection(Direction.TopToBottom)
                        .AppendTo(m_grid);

                    Builder
                        .NewBtnGeneral("item_" + entity.Prototype.Strings.Name.Id)
                        .SetButtonStyle(Builder.Style.Global.ImageBtn)
                        .SetSize(40, 40)
                        .SetIcon(entity.GetIcon())
                        .OnClick(
                            () =>
                            {
                                if (entity.HasPosition(out Tile2f position))
                                    m_entityTab.m_inspectorContext.CameraController.PanTo(position);
                            })
                        .SetOnMouseEnterLeaveActions(
                            () =>
                            {
                                m_entityTab.m_inspectorContext.Highlighter.Highlight(
                                    (IRenderedEntity)entity, ColorRgba.DarkYellow);
                            },
                            () =>
                            {
                                m_entityTab.m_inspectorContext.Highlighter.RemoveHighlight(
                                    (IRenderedEntity)entity);
                            })
                        .AppendTo(stack);

                    Builder
                        .NewBtnGeneral("item_" + entity.Prototype.Strings.Name.Id)
                        .SetButtonStyle(Builder.Style.Global.GeneralBtn)
                        .SetText(NewIds.Texts.Tools.Pick)
                        .SetSize(40, 20)
                        .OnClick(
                            () =>
                            {
                                m_entityTab.m_input.Entity = entity;
                                m_entityTab.m_entityPicker.Hide();
                                m_entityTab.m_refresh();
                            })
                        .SetOnMouseEnterLeaveActions(
                            () =>
                            {
                                m_entityTab.m_inspectorContext.Highlighter.Highlight(
                                    (IRenderedEntity)entity, ColorRgba.DarkYellow);
                            },
                            () =>
                            {
                                m_entityTab.m_inspectorContext.Highlighter.RemoveHighlight(
                                    (IRenderedEntity)entity);
                            })
                        .AppendTo(stack);
                }
            }
        }
    }
}