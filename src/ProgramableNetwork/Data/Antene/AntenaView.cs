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

            AddBandDisplay(itemContainer, updaterBuilder, (width, height) =>
            {
                SetContentSize(width, height + 80);
            });

            AddUpdater(updaterBuilder.Build(SyncFrequency.Critical));
        }

        private void AddBandDisplay(StackContainer itemContainer, UpdaterBuilder updaterBuilder, Action<int, int> resize)
        {
            itemContainer.SetStackingDirection(StackContainer.Direction.TopToBottom);
            itemContainer.SetSizeMode(StackContainer.SizeMode.Dynamic);

            var bandSelector = Builder.NewStackContainer("bandSelector")
                .SetParent(itemContainer, true)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetHeight(40)
                .AppendTo(itemContainer);

            itemContainer.AddDivider(itemContainer.ItemsCount, 10, Builder.Style.Global.ControlsBgColor);

            Builder.NewTxt("bandInfo")
                .SetText("Redirected signals:");

            var bandList = Builder.NewStackContainer("bandList")
                .SetParent(itemContainer, true)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .AppendTo(itemContainer);

            var addButton = Builder.NewBtnGeneral("band_channel_new")
                .SetParent(itemContainer, true)
                .SetHeight(20)
                .SetText("+")
                .OnClick(() =>
                {
                    Entity.DataBand.CreateChannel();
                })
                .AppendTo(itemContainer);

            //void Resize(IUiElement element)
            //{
            //    var fullSize = bandSelector.GetSize() + bandDisplay.GetSize() + bandList.GetSize() + addButton.GetSize();
            //    itemContainer.UpdateItemSize(element, element.GetSize());
            //    resize((int)fullSize.x, (int)fullSize.y);
            //};
            //bandSelector.SizeChanged += Resize;
            //bandDisplay.SizeChanged += Resize;
            //bandList.SizeChanged += Resize;

            updaterBuilder.Observe(() => Entity?.DataBand)
                .Do(band =>
                {
                    bandSelector.ClearAndDestroyAll();
                    if (band == null)
                    {
                        addButton.SetEnabled(false);
                        return;
                    }
                    addButton.SetEnabled(true);

                    List<DataBandProto> dataBandProtos = Entity.Context.ProtosDb.All<DataBandProto>().ToList();
                    if (band.Prototype.Id != DataBands.DataBand_Unknown)
                    {
                        dataBandProtos.RemoveAll(p => p.Id == DataBands.DataBand_Unknown);
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
                                Entity.DataBand = current.Constructor(Entity.Context, current);
                            });
                        }
                    }
                });

            updaterBuilder.Observe(() => Entity?.DataBand?.Channels, new ChannelComparator(() => Entity?.DataBand?.Prototype?.Comparator))
                .Do(list =>
                {
                    bandList.ClearAndDestroyAll();
                    bandList.SetHeight(0);
                    itemContainer.UpdateSizesFromItems();
                    if (list == null || Entity?.DataBand == null)
                    {
                        return;
                    }

                    var proto = Entity.DataBand.Prototype;

                    for (int i = 0; i < list.Count; i++)
                    {
                        IDataBandChannel item = list[i];
                        var row = Builder.NewStackContainer("band_channel_" + i)
                            .SetStackingDirection(StackContainer.Direction.LeftToRight)
                            .SetHeight(40);

                        new AntenaPicker(Builder, Entity.Prototype, item, value => item.Antena = value, proto.Distance, () => { }, this, m_inspector)
                            .AppendTo(row);

                        var text = Builder.NewTxt("band_channel_value_" + i)
                            .SetHeight(40)
                            .SetText(proto.Display(Entity.Context, item))
                            .SetAlignment(UnityEngine.TextAnchor.MiddleLeft)
                            .AppendTo(row);

                        proto.Buttons(Builder, row, item, () => {
                            text.SetText(proto.Display(Entity.Context, item));
                        });

                        Builder.NewBtnGeneral("band_channel_remove_" + i)
                            .SetSize(20, 20)
                            .SetText("X")
                            .OnClick(() => Entity.DataBand.RemoveChannel(item))
                            .AppendTo(row);

                        row.AppendTo(bandList);
                    }

                    bandList.SetHeight(40 * (1 + list.Count));
                    itemContainer.UpdateSizesFromItems();
                });
        }
    }
}