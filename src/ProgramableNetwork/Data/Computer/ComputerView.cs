using Mafi;
using Mafi.Core;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mafi.Unity.UiFramework;
using Mafi.Core.Syncers;
using System.Linq;
using Mafi.Unity.UserInterface;

namespace ProgramableNetwork
{
    public partial class ComputerView : StaticEntityInspectorBase<Computer>
    {
        private readonly ComputerInspector m_controller;
        private readonly Dictionary<IUiElement, Action> m_displayChanges;
        private ISyncer<Computer> selectionchanged;
        private StackContainer instructionListHolder;
        private Instruction m_copiedInstruction;
        private bool m_repaintInstructions;
        public ComputerView(ComputerInspector controller)
            : base(controller)
        {
            m_controller = controller;
            m_displayChanges = new Dictionary<IUiElement, Action>();
        }

        protected override Computer Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            var instruction = AddStatusInfoPanel();
            updaterBuilder.Observe(() => m_controller.SelectedEntity?.Instructions?.Count ?? 0)
                .Do(count => instruction.SetStatus(NewIds.Texts.InstructionCount.Format(count.ToString()).Value, count == 0 ? StatusPanel.State.Warning : StatusPanel.State.Ok));

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

            var instructionIndex = AddStatusInfoPanel();
            updaterBuilder.Observe(() => m_controller.SelectedEntity?.CurrentInstruction ?? 0)
                .Do(instr => instructionIndex.SetStatus($"{instr:D3}", StatusPanel.State.Ok));

            AddGeneralPriorityPanel(m_controller.Context, () => m_controller.SelectedEntity);

            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            MyTabContainer tabContainer = new MyTabContainer(Builder, Style, "computerType", () => { })
                .SetWidth(720)
                .SetTabCellSize(new Vector2(310, 40))
                .AppendTo(itemContainer);

            StackContainer newType = Builder
                .NewStackContainer("newType")
                .SetWidth(720)
                .SetItemSpacing(5);

            tabContainer.AddTab(new Mafi.Localization.LocStrFormatted("New implementation"), newType);
            AddModuleImplementation(newType, updaterBuilder);

            StackContainer oldType = Builder
                .NewStackContainer("oldType")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetWidth(720)
                .SetItemSpacing(5);

            tabContainer.AddTab(new Mafi.Localization.LocStrFormatted("Old implementation (change required)"), oldType);

            instructionListHolder = Builder
                .NewStackContainer("instructionHolder")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetBackground(Style.EntitiesMenu.MenuBg)
                .SetInnerPadding(Offset.All(5))
                .SetWidth(720)
                .SetItemSpacing(5)
                .AppendTo(oldType);

            this.SetWidth(760);

            itemContainer.SetSizeMode(StackContainer.SizeMode.Dynamic);

            selectionchanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity);

            AddInstructionAdder(oldType, updaterBuilder);

            AddUpdater(updaterBuilder.Build());
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

                instructionListHolder.ClearAndDestroyAll();

                UpdateUI(m_displayChanges);
            }

            if (m_controller.SelectedEntity != null)
            {
                foreach (var r in refreshed)
                    r.Refresh();
                foreach (var d in new List<Action>(m_displayChanges.Values))
                    d.Invoke();
            }
        }
    }
}
