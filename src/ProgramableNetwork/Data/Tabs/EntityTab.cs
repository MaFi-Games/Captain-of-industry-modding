using Mafi;
using Mafi.Core.Entities;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

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
        private readonly ComputerInspector m_inspector;
        private readonly InspectorContext m_inspectorContext;
        private readonly InstructionProto m_instruction;
        private readonly StackContainer m_btnPreviewHolder;
        private readonly Btn m_selectionButton;
        private Btn m_btnPreview;

        public EntityTab(UiBuilder builder, Computer computer, InstructionProto instruction, InstructionProto.InputType type,
            MemoryPointer input, Action refresh, ItemDetailWindowView parentWindow, ComputerInspector inspector)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_computer = computer;
            m_input = input;
            m_filter = instruction.EntityFilter;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspector = inspector;
            m_inspectorContext = inspector.Context;
            m_instruction = instruction;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetStackingDirection(Direction.LeftToRight)
                .SetSizeMode(SizeMode.Dynamic)
                .SetSize(80, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .AppendTo(m_btnPreviewHolder);

            m_selectionButton = m_builder
                .NewBtnGeneral("item_vehicle_" + DateTime.Now.Ticks)
                .SetText(NewIds.Texts.Tools.Pick)
                .SetSize(40, 40)
                .OnClick(PickEntity)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);

            m_window.OnHide += onHide;

            Refresh();
        }

        private void onHide()
        {
            m_window.OnHide -= onHide;
            m_inspector.EntitySelectionInput = null;
        }

        private void PickEntity()
        {
            m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtnActive);
            m_inspector.EntitySelectionInput = new Selector(m_instruction, m_input, m_refresh);
        }

        public void Refresh()
        {
            m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtn);
            m_inspector.EntitySelectionInput = null;
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
                            (IRenderedEntity)m_input.Entity, ColorRgba.LightBlue);
                    },
                    () =>
                    {
                        m_inspectorContext.Highlighter.RemoveHighlight(
                            (IRenderedEntity)m_input.Entity);
                    });
                m_selectionButton.SetOnMouseEnterLeaveActions(
                    () =>
                    {
                        m_inspectorContext.Highlighter.Highlight(
                            (IRenderedEntity)m_input.Entity, ColorRgba.LightBlue);
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
                m_selectionButton.SetOnMouseEnterLeaveActions(() => { }, () => { });
            }
        }
    }
}