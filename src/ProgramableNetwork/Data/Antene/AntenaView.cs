using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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

            AddBandDisplay(itemContainer, updaterBuilder);

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
        }

        private void AddBandDisplay(StackContainer itemContainer, UpdaterBuilder updaterBuilder)
        {
            var bandSelector = Builder.NewStackContainer("bandSelector")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetHeight(40)
                .AppendTo(itemContainer);

            var bandDisplay = Builder.NewStackContainer("bandDisplay")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(itemContainer);

            updaterBuilder.Observe(() => Entity.DataBand)
                .Do(band =>
                {
                    bandSelector.ClearAndDestroyAll();
                    if (band == null) return;

                    List<DataBandProto> dataBandProtos = Entity.Context.ProtosDb.All<DataBandProto>().ToList();
                    if (band.Prototype.Id != DataBands.UnknownDataBand)
                    {
                        dataBandProtos.RemoveAll(p => p.Id == DataBands.UnknownDataBand);
                    }

                    int index = dataBandProtos.IndexOf(band.Prototype);
                    // Add selector
                    for (int i = 0; i < dataBandProtos.Count; i++)
                    {
                        var tooltip = new Tooltip(Builder).SetText(dataBandProtos[i].Strings.DescShort);
                        var selectionButton = Builder
                            .NewBtnGeneral("band_"+i)
                            .SetText(dataBandProtos[i].Strings.Name)
                            .SetButtonStyle(i == index ? Style.Global.GeneralBtnActive : Style.Global.GeneralBtn)
                            .SetHeight(40)
                            .AppendTo(bandSelector);
                        tooltip.AttachTo(selectionButton);
                        if (i != index)
                        {
                            var current = dataBandProtos[i];
                            selectionButton.OnClick(() =>
                            {
                                Entity.DataBand = new DataBand(Entity.Context, current, 0);
                            });
                        }
                    }

                    // Add display

                });
                
        }
    }
}