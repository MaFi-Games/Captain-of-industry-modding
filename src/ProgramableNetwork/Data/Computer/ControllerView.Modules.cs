using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
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
        private ModuleProto m_newModule;
        private Module m_editModule;
        private StackContainer m_newDialog;
        private StackContainer m_editDialog;
        private StackContainer m_container;
        private LocStr? m_decription;
        private List<IDataUpdater> m_updaters = new List<IDataUpdater>();
        private Category m_category;

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

            updaterBuilder.Observe(() => Entity).Do((entity) => {
                CloseDialogs();
                m_targetRow = -1;
                m_targetColumn = -1;
                m_newModule = null;
                m_editModule = null;
                m_decription = null;
                OutputConnection = null;
                m_controller.EntityHighlighterSelectable.ClearAllHighlights();
                m_updaters.Clear();

                if (entity != null)
                {
                    RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst());
                }
            });
            updaterBuilder.Observe(WasOrderChanged, new ModuleIdComparator()).Do(RedrawComponents);
            updaterBuilder.Observe(() => (m_targetRow, m_targetColumn)).Do((change) => RedrawComponents(Entity.Modules.Select(m => m.Id).ToLyst()));

            //updaterBuilder.Observe(WasUpdated).Do(UpdateChanged);
        }

        private List<IDataUpdater> WasUpdated()
        {
            List<IDataUpdater> dataUpdaters = new List<IDataUpdater>();
            foreach (IDataUpdater item in m_updaters)
                if (item.WasChanged())
                    dataUpdaters.Add(item);
            return dataUpdaters;
        }

        private void UpdateChanged(List<IDataUpdater> obj)
        {
            foreach (var item in obj)
                item.Update();
        }

        private void CreateNewDialog(int targetRow, int targetColumn)
        {
            CloseDialogs();

            m_targetRow = targetRow;
            m_targetColumn = targetColumn;
            Action refresh = () => CreateNewDialog(targetRow, targetColumn);

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

            Dropdwn catPicker = Builder.NewDropdown("dialogNewCatPicker")
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .AppendTo(m_newDialog);

            FillCategoryPicker(catPicker, refresh);

            Dropdwn typePicker = Builder.NewDropdown("dialogNewTypePicker")
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .AppendTo(m_newDialog);

            FillModulePicker(typePicker, m_category, refresh);

            Builder.NewBtnGeneral("dialogNewCreate")
                .SetText(NewTr.Tools.Add)
                .SetParent(m_newDialog, true)
                .SetHeight(20)
                .SetEnabled(m_newModule != null)
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

        private void FillCategoryPicker(Dropdwn picker, Action refresh)
        {
            List<Category> types = Category.Categories(Entity.Context.ProtosDb, Entity);
            types.Insert(0, new Category("all", "All"));

            var typeStrings = types.Select(t => t.Name).ToList();

            var selected = types.SelectIndicesWhere(t => t.Id == m_category?.Id).FirstOrDefault();
            picker.AddOptions(typeStrings);
            picker.OnValueChange(index =>
            {
                m_category = types[index];
                m_newModule = null;
                refresh();
            });

            picker.SetValueWithoutNotify(selected);

            if (m_category == null)
                m_category = types[0];
        }

        private void FillModulePicker(Dropdwn picker, Category category, Action refresh)
        {
            var typesAll = Entity.Context.ProtosDb.All<ModuleProto>();
            List<ModuleProto> types = 
                (category.Id == "all"
                    ? Entity.Context.ProtosDb.All<ModuleProto>()
                    : Entity.Context.ProtosDb.All<ModuleProto>()
                          .Where(module => module.Categories.Contains(category)))
                .Where(Entity.Prototype.AllowedModule)
                .ToList();

            var typeStrings = types.Select(t => t.Strings.Name.TranslatedString).ToList();
            var selected = types.SelectIndicesWhere(t => t.Id == m_newModule?.Id).FirstOrDefault();
            picker.AddOptions(typeStrings);
            picker.OnValueChange(index =>
            {
                m_newModule = types[index];
                refresh();
            });

            picker.SetValueWithoutNotify(selected);

            if (m_newModule == null && types.Count > 0)
                m_newModule = types[0];
            else if (types.Count == 0)
                m_newModule = null;
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
            m_updaters.Clear();
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
                    new ModuleView(Builder, module, m_controller, this, selected, () => RedrawComponents(modules))
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

        private bool TryPlaceAt(ModuleProto moduleProto, int targetRow, int targetColumn)
        {
            var module = new Module(moduleProto, Entity.Context, Entity);
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
    }
}
