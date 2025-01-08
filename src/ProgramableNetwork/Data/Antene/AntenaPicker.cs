using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using static Mafi.Unity.UiFramework.Components.StackContainer;

namespace ProgramableNetwork
{
    public class AntenaPicker : StackContainer
    {
        private readonly UiBuilder m_builder;
        private readonly Action m_refresh;
        private readonly WindowView m_window;
        private readonly ISelectionInspector<Antena, AntenaSelector, Antena> m_inspector;
        private readonly InspectorContext m_inspectorContext;
        private readonly IDataBandChannel m_module;
        private readonly string m_dataName;
        private readonly Fix32 m_distance;
        private readonly Action<Antena> m_selected;
        private readonly AntenaProto m_prototype;
        private readonly StackContainer m_btnPreviewHolder;
        private readonly Btn m_selectionButton;
        private Btn m_btnPreview;

        public AntenaPicker(UiBuilder builder, AntenaProto antenaProto, IDataBandChannel module, Action<Antena> selected, Fix32 distance, Action refresh, WindowView parentWindow, AntenaInspector inspector)
            : base(builder, "entity_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspector = inspector;
            m_inspectorContext = inspector.Context;
            m_module = module;
            m_distance = distance;
            m_selected = selected;
            m_prototype = antenaProto;

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
                .SetText("Pick")
                .SetSize(40, 40)
                .OnClick(PickEntity)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);

            m_window.OnHide += onHide;

            m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtn);
            m_inspector.EntitySelectionInput = null;

            SelectionChanged(m_module.Antena);
        }

        private void SelectionChanged(Antena entity)
        {
            m_module.Antena = entity;
            if (entity != null)
            {
                m_btnPreview.SetIcon(entity.GetIcon());
                m_btnPreview.OnClick(
                    () => m_inspectorContext.CameraController.PanTo(entity.Position2f));
                m_btnPreview.SetOnMouseEnterLeaveActions(
                    () => m_inspectorContext.Highlighter.Highlight(entity, ColorRgba.LightBlue),
                    () => m_inspectorContext.Highlighter.RemoveHighlight(entity));
                m_selectionButton.SetOnMouseEnterLeaveActions(
                    () => m_inspectorContext.Highlighter.Highlight(entity, ColorRgba.LightBlue),
                    () => m_inspectorContext.Highlighter.RemoveHighlight(entity));
            }
            else
            {
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
                m_btnPreview.OnClick(() => { });
                m_btnPreview.SetOnMouseEnterLeaveActions(() => { }, () => { });
                m_selectionButton.SetOnMouseEnterLeaveActions(() => { }, () => { });
            }
        }

        private void onHide()
        {
            m_window.OnHide -= onHide;
            m_inspector.EntitySelectionInput = null;
        }

        private void PickEntity()
        {
            m_selectionButton.SetButtonStyle(m_builder.Style.Global.GeneralBtnActive);
            m_inspector.EntitySelectionInput = new AntenaSelector(
                m_distance,
                m_refresh,
                (entity) => entity.Prototype == m_prototype && entity != m_inspector.SelectedEntity,
                SelectionChanged);
        }
    }
}