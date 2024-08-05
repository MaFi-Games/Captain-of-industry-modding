using Mafi;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Factory;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.InputControl.Tools;
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
        private readonly Func<Entity, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly InstructionProto m_instruction;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;

        public EntityTab(UiBuilder builder, Computer computer, InstructionProto instruction, InstructionProto.InputType type,
            MemoryPointer input, Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_computer = computer;
            m_input = input;
            m_filter = instruction.EntityFilter;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;
            m_instruction = instruction;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetStackingDirection(Direction.LeftToRight)
                .SetSizeMode(SizeMode.Dynamic)
                .SetSize(140, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .AppendTo(m_btnPreviewHolder);

            var m_btns = m_builder
                .NewStackContainer("item_" + DateTime.Now.Ticks)
                .SetStackingDirection(Direction.TopToBottom)
                .SetSize(100, 40)
                .AppendTo(m_btnPreviewHolder);

            if (type == InstructionProto.InputType.Entity || type.HasFlag(InstructionProto.InputType.StaticEntity))
                m_builder
                    .NewBtnGeneral("item_building_" + DateTime.Now.Ticks)
                    .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                    .SetText(NewIds.Texts.PointerTypes[InstructionProto.InputType.StaticEntity])
                    .SetSize(100, 20)
                    .OnClick(FindBuilding)
                    .AppendTo(m_btns);

            if (type == InstructionProto.InputType.Entity || type.HasFlag(InstructionProto.InputType.DynamicEntity))
                m_builder
                    .NewBtnGeneral("item_vehicle_" + DateTime.Now.Ticks)
                    .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                    .SetText(NewIds.Texts.PointerTypes[InstructionProto.InputType.DynamicEntity])
                    .SetSize(100, 20)
                    .OnClick(FindDynamic)
                    .AppendTo(m_btns);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);

            Refresh();
        }

        private void FindDynamic()
        {
            StaticEntitySelectionController.Instance.Activate(this);
        }

        private void FindBuilding()
        {
            VehicleEntitySelectionController.Instance.Activate(this);
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

        [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
        private class StaticEntitySelectionController : AEntitySelectionController<StaticEntity>
        {
            public static StaticEntitySelectionController Instance { get; private set; }

            public StaticEntitySelectionController(ProtosDb protosDb, UiBuilder builder, UnlockedProtosDbForUi unlockedProtosDb, ShortcutsManager shortcutsManager, IUnityInputMgr inputManager, CursorPickingManager cursorPickingManager, CursorManager cursorManager, AreaSelectionToolFactory areaSelectionToolFactory, IEntitiesManager entitiesManager, NewInstanceOf<EntityHighlighter> highlighter, Option<NewInstanceOf<TransportTrajectoryHighlighter>> transportTrajectoryHighlighter)
                : base(protosDb, builder, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, transportTrajectoryHighlighter, NewIds.Tools.SelectStaticEntity)
            {
                Instance = this;
            }
        }

        [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
        private class VehicleEntitySelectionController : AEntitySelectionController<DynamicGroundEntity>
        {
            public static VehicleEntitySelectionController Instance { get; private set; }

            public VehicleEntitySelectionController(ProtosDb protosDb, UiBuilder builder, UnlockedProtosDbForUi unlockedProtosDb, ShortcutsManager shortcutsManager, IUnityInputMgr inputManager, CursorPickingManager cursorPickingManager, CursorManager cursorManager, AreaSelectionToolFactory areaSelectionToolFactory, IEntitiesManager entitiesManager, NewInstanceOf<EntityHighlighter> highlighter, Option<NewInstanceOf<TransportTrajectoryHighlighter>> transportTrajectoryHighlighter)
                : base(protosDb, builder, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, transportTrajectoryHighlighter, NewIds.Tools.SelectDynamicEntity)
            {
                Instance = this;
            }
        }

        private class AEntitySelectionController<T> : BaseEntityCursorInputController<T> where T : Entity, IRenderedEntity, IAreaSelectableEntity
        {
            private EntityTab m_entityTab;

            public AEntitySelectionController(ProtosDb protosDb, UiBuilder builder, UnlockedProtosDbForUi unlockedProtosDb, ShortcutsManager shortcutsManager, IUnityInputMgr inputManager, CursorPickingManager cursorPickingManager, CursorManager cursorManager, AreaSelectionToolFactory areaSelectionToolFactory, IEntitiesManager entitiesManager, NewInstanceOf<EntityHighlighter> highlighter, Option<NewInstanceOf<TransportTrajectoryHighlighter>> transportTrajectoryHighlighter, Proto.ID? lockByProto)
                : base(protosDb, builder, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, transportTrajectoryHighlighter, lockByProto)
            {
                SetPartialTransportsSelection(false);
            }

            public void Activate(EntityTab entityTab) {
                m_entityTab = entityTab;
                Activate();
            }

            protected override bool Matches(T entity, bool isAreaSelection, bool isLeftClick)
            {
                return !isAreaSelection && !isLeftClick && m_entityTab.m_filter(entity);
            }

            protected override void OnEntitiesSelected(IIndexable<T> selectedEntities, IIndexable<SubTransport> selectedPartialTransports, bool isAreaSelection, bool isLeftMouse, RectangleTerrainArea2i? area)
            {
                if (selectedEntities.Count == 0) return;
                if (isAreaSelection) return;
                if (!isLeftMouse) return;

                // TODO select the first
                m_entityTab.m_input.Entity = selectedEntities[0];
                Deactivate();
                m_entityTab.m_refresh();
            }

            protected override bool OnFirstActivated(T hoveredEntity, Lyst<T> selectedEntities, Lyst<SubTransport> selectedPartialTransports)
            {
                return false;
            }

            protected override void RegisterToolbar(ToolbarController controller)
            {
                // no toolbar required
            }
        }
    }
}