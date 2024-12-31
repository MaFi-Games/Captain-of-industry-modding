using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi;
using Mafi.Core.Entities.Dynamic;
using Mafi.Localization;
using System;

namespace ProgramableNetwork
{
    public partial class ControllerView : StaticEntityInspectorBase<Controller>
    {
        private StackContainer m_moduleLayout;
        private StackContainer m_moduleDialog;
        private int m_targetRow = -1;
        private int m_targetColumn = -1;
        private Module m_newModule;
        private Module m_editModule;
        private StackContainer m_newDialog;
        private StackContainer m_editDialog;
        private StackContainer m_container;
        private LocStr? m_decription;

        public ModuleConnector OutputConnection { get; set; }

        private void AddModuleImplementation(StackContainer itemsContainer, UpdaterBuilder updaterBuilder, Action<float, float> setSize)
        {
            m_container = Builder.NewStackContainer("halves")
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetItemSpacing(10)
                .AppendTo(itemsContainer)
                .DynamicSizeListener(itemsContainer, Dynamic.Width);

            m_moduleLayout = Builder
                .NewStackContainer("layout")
                .SetParent(m_container, true)
                .SetItemSpacing(10)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .AppendTo(m_container)
                .DynamicSizeListener(m_container, Dynamic.Height);

            m_moduleDialog = Builder
                .NewStackContainer("dialog")
                .SetParent(m_container, true)
                .SetStackingDirection(StackContainer.Direction.BottomToTop)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetWidth(400)
                .AppendTo(m_container)
                .DynamicSizeListener(m_container, Dynamic.Height);

            updaterBuilder.Observe(WasOrderChanged, new ModuleIdComparator()).Do(RedrawComponents);
            updaterBuilder.Observe(() => (m_targetRow, m_targetColumn)).Do((change) => RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst()));
            updaterBuilder.Observe(() => Entity).Do((entity) => {
                CloseDialogs();
                m_targetRow = -1;
                m_targetColumn = -1;
                m_newModule = null;
                m_editModule = null;
                m_decription = null;
                OutputConnection = null;
                m_controller.EntityHighlighterSelectable.ClearAllHighlights();
            });
        }

        private void CreateNewDialog(int targetRow, int targetColumn)
        {
            CloseDialogs();

            m_targetRow = targetRow;
            m_targetColumn = targetColumn;

            m_newDialog = Builder.NewStackContainer("dialogNew")
                .SetItemSpacing(5)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetWidth(400);

            Builder.NewTxt("dialogNewPick")
                .SetText(NewTr.Tools.Pick)
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .AppendTo(m_newDialog);

            Dropdwn picker = Builder.NewDropdown("dialogNewPicker")
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .AppendTo(m_newDialog);

            FillPicker(picker);

            // Settings
            CreateSettingsPanel(m_newDialog, () => m_newModule, () => CreateNewDialog(targetRow, targetColumn));

            Builder.NewBtnGeneral("dialogNewCreate")
                .SetText(NewTr.Tools.Add)
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .OnClick(() =>
                {
                    if (TryPlaceAt(m_newModule, m_targetRow, m_targetColumn))
                    {
                        CreateEditDialog(m_editModule);
                    }
                })
                .AppendTo(m_newDialog);

            m_newDialog.AppendTo(m_moduleDialog);
            m_newDialog.SetParent(m_moduleDialog, true);
            m_moduleDialog.SetWidth(400);
        }

        private void FillPicker(Dropdwn picker)
        {
            List<ModuleProto> types = Entity.Context.ProtosDb
                .All<ModuleProto>()
                .Where(Entity.Prototype.AllowedModule)
                .ToList();
            var typeStrings = types.Select(t => t.Strings.Name.TranslatedString).ToList();
            var selected = types.SelectIndicesWhere(t => t.Id == m_newModule?.Prototype.Id).FirstOrDefault();
            picker.AddOptions(typeStrings);
            picker.OnValueChange(index =>
            {
                m_newModule = new Module(types[index], Entity.Context, Entity);
                CreateNewDialog(m_targetRow, m_targetColumn);
            });

            picker.SetValueWithoutNotify(selected);

            if (m_newModule == null)
                m_newModule = new Module(types[0], Entity.Context, Entity);
        }

        private void CreateEditDialog(Module module)
        {
            CloseDialogs();

            m_editModule = module;
            m_targetRow = -1;
            m_targetColumn = -1;

            for (int i = 0; i < Entity.Rows.Count; i++)
            {
                for (int j = 0; j < Entity.Rows[i].Count; j++)
                {
                    if (Entity.Rows[i][j].Placement && Entity.Rows[i][j].ModuleId == module?.Id)
                    {
                        m_targetRow = i;
                        m_targetColumn = j;
                    }
                }
            }

            m_editDialog = Builder.NewStackContainer("dialogEdit")
                .SetItemSpacing(5)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetWidth(400);

            var moduleName = Builder.NewTxt("dialogEditName")
                .SetParent(m_editDialog, true)
                .SetText(m_editModule.Prototype.Strings.Name)
                .SetHeight(20)
                .AppendTo(m_editDialog);

            CreateSettingsPanel(m_editDialog, () => m_editModule, () => CreateEditDialog(module));

            Builder.NewBtnGeneral("dialogEditRemove")
                .SetText(NewTr.Tools.Remove)
                .SetHeight(20)
                .OnClick(() =>
                {
                    Entity.Modules.Remove(m_editModule);
                    int width = m_editModule.Layout.GetWidth(m_editModule);
                    for (int i = m_targetColumn; i < (m_targetColumn + width); i++)
                    {
                        Entity.Rows[m_targetRow][i] = 0;
                    }
                    m_container.HideItem(m_editDialog);
                })
                .AppendTo(m_editDialog);

            m_editDialog.AppendTo(m_moduleDialog);
            m_editDialog.SetParent(m_moduleDialog, true);
            m_moduleDialog.SetWidth(400);
        }

        private void CloseDialogs()
        {
            try { if (m_editDialog != null) m_moduleDialog.RemoveAndDestroy(m_editDialog); m_editDialog = null; }
            catch (Exception) { Console.WriteLine("Failed to delete edit dialog"); }
            try { if (m_newDialog != null) m_moduleDialog.RemoveAndDestroy(m_newDialog); m_newDialog = null; }
            catch (Exception) { Console.WriteLine("Failed to delete new dialog"); }
            m_moduleDialog.SetWidth(400);
        }

        private IEnumerable<long> WasOrderChanged()
        {
            return Entity?.Rows?.SelectMany(item => item)?.Select(item => item.ModuleId);
        }

        private void RedrawComponents(Lyst<long> modules)
        {
            m_moduleLayout.ClearAndDestroyAll();
            m_moduleLayout.SetSize(Entity.Prototype.Columns * 20, Entity.Prototype.Rows * 80 + (Entity.Prototype.Rows - 1) * 10);
            m_moduleLayout.SetSizeMode(StackContainer.SizeMode.Dynamic);

            for (int i = 0; i < Entity.Rows.Count; i++)
            {
                var rowElement = Builder
                    .NewStackContainer("row_" + i)
                    .SetParent(m_moduleLayout, true)
                    .SetItemSpacing(0)
                    .SetSize(Entity.Prototype.Columns * 20, 80)
                    .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                    .SetStackingDirection(StackContainer.Direction.LeftToRight);

                var row = Entity.Rows[i];
                for (int j = 0; j < row.Count; j++)
                {
                    if (!row[j].Placement) continue;

                    bool selected = i == m_targetRow && j == m_targetColumn;

                    var module = (Entity.Modules ?? new Lyst<Module>())
                        .AsEnumerable()
                        .FirstOrDefault(m => m.Id == row[j].ModuleId);

                    if (module == null)
                    {
                        AddFreeSlot(rowElement, i, j, selected);
                        continue;
                    }
                    new ComputerModuleView(Builder, module, m_controller, this, selected, () => RedrawComponents(modules))
                        .AppendTo(rowElement);
                }

                rowElement.AppendTo(m_moduleLayout);
            }
        }

        private void AddFreeSlot(StackContainer rowElement, int targetRow, int targetColumn, bool selected)
        {
            var toggle = Builder.NewBtnGeneral($"addModule_{targetRow}_{targetColumn}")
                .SetSize(20, 80)
                .SetParent(rowElement, true)
                .SetText("+")
                .AppendTo(rowElement);

            if (selected)
                toggle.SetButtonStyle(Builder.Style.Global.GeneralBtnActive);

            toggle.OnClick(() => {
                CreateNewDialog(targetRow, targetColumn);
            });
        }

        private void CreateSettingsPanel(StackContainer dialog, Func<Module> moduleProvider, Action updateDialog)
        {
            Builder.NewTxt("dialogEditName")
                .SetText("Settings:")
                .SetHeight(20)
                .AppendTo(dialog);

            var holder = Builder.NewStackContainer("holder")
                .SetParent(dialog, true)
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetItemSpacing(5);

            Module module = moduleProvider();
            if (module == null || module.Prototype.Fields.Count == 0)
            {
                Builder.NewTxt("info")
                    .SetParent(holder, true)
                    .SetText("empty")
                    .SetHeight(20)
                    .AppendTo(holder);

                holder.SetHeight(25);
                holder.AppendTo(dialog);
                return;
            }

            int height = (module.Prototype.Fields.Count - 1) * 5 + 10;
            foreach (IField item in module.Prototype.Fields)
            {
                StackContainer fieldContainer = Builder.NewStackContainer("field_" + item.Name)
                    .SetParent(holder, true)
                    .SetHeight(item.Size)
                    .AppendTo(holder);

                item.Init(m_controller, this, fieldContainer, Builder, module, updateDialog);
                height += item.Size;
            }

            holder.SetHeight(height);
            holder.AppendTo(dialog);
        }

        private bool TryPlaceAt(Module module, int targetRow, int targetColumn)
        {
            var width = module.Layout.GetWidth(module);
            var placeFound = true;
            var row = Entity.Rows[targetRow];

            var end = targetColumn + width;
            for (int columnEnd = targetColumn; columnEnd < end; columnEnd++)
            {
                if (row[columnEnd].ModuleId != 0)
                {
                    placeFound = false;
                    break;
                }
            }

            if (placeFound)
            {
                for (int i = targetColumn; i < end; i++)
                {
                    row[i] = (module.Id, false);
                }
                row[targetColumn] = (module.Id, true);
                Entity.Modules.Add(module);
                m_editModule = module;
                m_targetRow = targetRow;
                m_targetColumn = targetColumn;
                m_newModule = null;
                return true;
            }
            else
            {
                Builder.AudioDb.GetSharedAudio(Builder.Audio.InvalidOp).Play();
                return false;
            }
        }

        private class ModuleIdComparator : ICollectionComparator<long, IEnumerable<long>>
        {
            public bool AreSame(IEnumerable<long> collectionC, Lyst<long> lastKnown)
            {
                if ((lastKnown == null && collectionC != null) || (lastKnown != null && collectionC == null))
                {
                    return false;
                }

                Lyst<long> collection = collectionC?.ToLyst();
                if (lastKnown?.Count != collection?.Count)
                {
                    return false;
                }

                int length = lastKnown.Count;
                for (int i = 0; i < length; i++)
                {
                    if (collection[i] != lastKnown[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class ComputerModuleView : StackContainer
        {
            private readonly Module m_module;
            private readonly ControllerInspector m_controller;
            private readonly ControllerView m_computerView;
            private readonly List<IUiUpdater> m_updaters = new List<IUiUpdater>();

            public ComputerModuleView(UiBuilder uiBuilder, Module module, ControllerInspector controller, ControllerView computerView, bool selected, Action refresh)
                : base(uiBuilder, "moduleView_" + module.Id)
            {
                this.m_module = module;
                this.m_controller = controller;
                this.m_computerView = computerView;
                string name = "moduleView_" + module.Id;
                var updater = UpdaterBuilder.Start();
                int width = module.Layout.GetWidth(module);

                this.SetSize(width * 20, 80);
                this.SetStackingDirection(Direction.TopToBottom);
                this.SetItemSpacing(0);
                this.SetSizeMode(SizeMode.StaticDirectionAligned);

                // Add Input panel
                StackContainer inputsPanel = uiBuilder.NewStackContainer(name + "_inputs")
                    .SetParent(this, true)
                    .SetSize(width * 20, 20)
                    .SetBackground(ColorRgba.DarkGreen)
                    .SetSizeMode(SizeMode.StaticCenterAligned)
                    .SetStackingDirection(Direction.RightToLeft);
                AddInputs(uiBuilder, inputsPanel, module, updater, refresh);

                inputsPanel.AppendTo(this);

                // Add Field panel
                Btn fieldsPanel = uiBuilder.NewBtnGeneral(name + "_edit")
                    .SetParent(this, true)
                    .SetSize(width * 20, 40)
                    .SetText(module.Prototype.Symbol)
                    .OnClick(() =>
                    {
                        m_computerView.CreateEditDialog(module);
                    });

                if (selected)
                    fieldsPanel.SetButtonStyle(uiBuilder.Style.Global.GeneralBtnActive);

                fieldsPanel.AppendTo(this);

                // Add Ouptut panel
                StackContainer outputsPanel = uiBuilder.NewStackContainer(name + "_outputs")
                    .SetParent(this, true)
                    .SetSize(width * 20, 20)
                    .SetBackground(ColorRgba.DarkRed)
                    .SetSizeMode(SizeMode.StaticCenterAligned)
                    .SetStackingDirection(Direction.RightToLeft);
                AddOutputs(uiBuilder, outputsPanel, module, updater, refresh);

                outputsPanel.AppendTo(this);

                var updaterBuilt = updater.Build();
                m_updaters.Add(updaterBuilt);
                computerView.AddUpdater(updaterBuilt);

                this.RectTransform.ForceUpdateRectTransforms();
            }

            private void AddInputs(UiBuilder builder, StackContainer inputsPanel, Module module, UpdaterBuilder updater, Action refresh)
            {
                var inputs = module.Prototype.Inputs;
                for (int i = inputs.Count - 1; i >= 0; i--)
                {
                    var input = inputs[i];
                    bool isConnected = module.InputModules.ContainsKey(input.Id);

                    builder.NewBtnGeneral($"{module.Id}_input_{i}")
                        .SetText("O")
                        .SetButtonStyle((
                                isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn
                            ).Extend(backgroundClr: ColorRgba.DarkGreen))
                        .SetSize(20, 20)
                        .OnClick(() =>
                        {
                            if (m_computerView.OutputConnection == null)
                            {
                                builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp).Play();
                            }
                            else
                            {
                                module.InputModules[input.Id] = m_computerView.OutputConnection;
                                m_computerView.OutputConnection = null;
                                refresh();
                            }
                        })
                        .SetOnMouseEnterLeaveActions(
                            () => { m_computerView.m_decription = input.String.Name; },
                            () => { }
                        )
                        .AppendTo(inputsPanel);
                }
            }

            private void AddOutputs(UiBuilder builder, StackContainer inputsPanel, Module module, UpdaterBuilder updater, Action refresh)
            {
                var outputs = module.Prototype.Outputs;
                for (int i = outputs.Count - 1; i >= 0; i--)
                {
                    var output = outputs[i];
                    bool isConnected = module.Controller.Modules
                        .AsEnumerable()
                        .Where(m => m.InputModules.Count > 0)
                        .SelectMany(m => m.InputModules)
                        .Select(p => p.Value)
                        .FirstOrDefault(c => c.ModuleId == module.Id
                                          && c.OutputId == output.Id) != null;

                    builder.NewBtnGeneral($"{module.Id}_output_{i}")
                        .SetText("O")
                        .SetButtonStyle((
                                isConnected ? builder.Style.Global.GeneralBtnActive : builder.Style.Global.GeneralBtn
                            ).Extend(backgroundClr: ColorRgba.DarkRed))
                        .SetSize(20, 20)
                        .OnClick(() => m_computerView.OutputConnection = new ModuleConnector(module.Id, output.Id))
                        .SetOnMouseEnterLeaveActions(
                            () => { m_computerView.m_decription = output.String.Name; },
                            () => { }
                        )
                        .AppendTo(inputsPanel);
                }
            }

            public void Remove()
            {
                var updaterHolder = ((IUiUpdater)m_computerView.GetType().GetField("m_updater", System.Reflection.BindingFlags.NonPublic).GetValue(m_computerView));

                foreach (var updater in m_updaters)
                {
                    updaterHolder.RemoveChildUpdater(updater);
                }

                //m_controller.Context.EntitiesManager.RemoveAndDestroyEntityNoChecks(m_module, Mafi.Core.Entities.EntityRemoveReason.Remove);
                m_controller.SelectedEntity.Modules.Remove(m_module);
            }
        }
    }
}
