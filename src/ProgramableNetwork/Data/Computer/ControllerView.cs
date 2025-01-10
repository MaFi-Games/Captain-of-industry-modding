using Mafi;
using Mafi.Core;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiToolkit;
using Mafi.Core.Syncers;
using System.Linq;
using Mafi.Unity.UserInterface;

namespace ProgramableNetwork
{
    public partial class ControllerView : StaticEntityInspectorBase<Controller>
    {
        private readonly ControllerInspector m_controller;
        private readonly Dictionary<IUiElement, Action> m_displayChanges;
        private ISyncer<Controller> selectionchanged;
        private bool m_repaintInstructions;
        public ControllerView(ControllerInspector controller)
            : base(controller)
        {
            m_controller = controller;
            m_displayChanges = new Dictionary<IUiElement, Action>();
        }

        protected override Controller Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            //MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            var instruction = AddStatusInfoPanel();
            updaterBuilder.Observe(() => m_controller.SelectedEntity?.Modules?.Count ?? 0)
                .Do(count => instruction.SetStatus(NewTr.ModulesCount.Format(count.ToString()).Value, count == 0 ? StatusPanel.State.Warning : StatusPanel.State.Ok));

            var status = AddStatusInfoPanel();
            updaterBuilder.Observe(() =>
                    (m_controller.SelectedEntity?.ElectricityConsumer.ValueOrNull?.NotEnoughPower ?? false) ||
                    !(m_controller.SelectedEntity?.ErrorMessage?.IsNullOrEmpty() ?? true) ||
                    (m_controller.SelectedEntity?.IsPaused ?? false)
                )
                .Do(noElectricityOrError => {
                    if (!noElectricityOrError)
                        status.SetStatusWorking();
                    else if (!m_controller.SelectedEntity.ErrorMessage.IsEmpty())
                        status.SetStatus(m_controller.SelectedEntity.ErrorMessage, StatusPanel.State.Critical);
                    else if (m_controller.SelectedEntity.IsPaused)
                        status.SetStatus(Tr.EntityStatus__Working, StatusPanel.State.Critical);
                    else
                        status.SetStatusWorking();
                });

            AddGeneralPriorityPanel(m_controller.Context, () => m_controller.SelectedEntity);

            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            AddModuleImplementation(itemContainer, updaterBuilder, (width, height) =>
            {
                SetContentSize(width, height + 80);
            });

            selectionchanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity);

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
            m_repaintInstructions = true;
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            base.SyncUpdate(gameTime);
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (m_controller.SelectedEntity != null && Builder != null && (m_repaintInstructions || selectionchanged.HasChanged))
            {
                selectionchanged.GetValueAndReset();

                m_repaintInstructions = false;
            }

            if (m_controller.SelectedEntity != null)
            {
                foreach (var item in m_updaters)
                    item.Update();

                foreach (var d in new List<Action>(m_displayChanges.Values))
                    d.Invoke();
            }
        }
    }
}
