using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;

namespace ProgramableNetwork
{
    public class AntenaView : StaticEntityInspectorBase<Antena>
    {
        private AntenaInspector m_inspector;
        private ISyncer<Antena> selectionchanged;

        public AntenaView(AntenaInspector antenaInspector)
            :base(antenaInspector)
        {
            this.m_inspector = antenaInspector;
        }

        protected override Antena Entity => m_inspector.SelectedEntity;
        protected override void AddCustomItems(StackContainer itemContainer)
        {
            //MakeScrollableWithHeightLimit();
            base.AddCustomItems(itemContainer);

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            var status = AddStatusInfoPanel();
            updaterBuilder.Observe(() =>
                    (m_inspector.SelectedEntity?.ElectricityConsumer.ValueOrNull?.NotEnoughPower ?? false) ||
                    (m_inspector.SelectedEntity?.IsPaused ?? false)
                )
                .Do(noElectricityOrError => {
                    if (!noElectricityOrError)
                        status.SetStatusWorking();
                    else if (m_inspector.SelectedEntity.IsPaused)
                        status.SetStatus(Tr.EntityStatus__Working, StatusPanel.State.Critical);
                    else
                        status.SetStatusWorking();
                });

            AddGeneralPriorityPanel(m_inspector.Context, () => m_inspector.SelectedEntity);

            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            selectionchanged = updaterBuilder.CreateSyncer(() => m_inspector.SelectedEntity);

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
        }
    }
}