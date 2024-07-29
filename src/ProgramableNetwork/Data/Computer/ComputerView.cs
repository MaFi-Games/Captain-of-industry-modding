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

namespace ProgramableNetwork
{
    public class ComputerView : StaticEntityInspectorBase<Computer>
    {
        private readonly ComputerInspector m_controller;
        private ISyncer<Computer> selectionchanged;
        private Txt instructionCount;
        private Txt instructionError;
        private StackContainer instructionListHolder;
        private StackContainer scrollHolder;
        private ScrollableContainer scroll;
        private Instruction m_copiedInstruction;
        private bool m_repaintInstructions;
        private Dropdwn pick;
        private Btn btnAdd;
        private Dictionary<string, InstructionProto> availableInstructions;
        private List<string> typeNames;
        private List<string> names;
        private ISyncer<string> errorInScript;

        public ComputerView(ComputerInspector controller)
            : base(controller)
        {
            m_controller = controller;
        }

        protected override Computer Entity => m_controller.SelectedEntity;

        protected override void AddCustomItems(StackContainer itemContainer)
        {
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            errorInScript = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity?.ErrorMessage ?? "");

            AddGeneralPriorityPanel(m_controller.Context, () => m_controller.SelectedEntity);
            AddUnityCostPanel(updaterBuilder, () => m_controller.SelectedEntity);

            base.AddCustomItems(itemContainer);

            instructionCount = Builder
                .NewTxt("count")
                .SetText(NewIds.Texts.InstructionCount.Format(
                    (m_controller.SelectedEntity?.Instructions?.Count ?? 0).ToString()))
                .AppendTo(itemContainer);

            instructionError = Builder
                .NewTxt("count")
                .SetText("")
                .SetColor(ColorRgba.Red)
                .SetTextStyle(Style.Notifications.CriticalTextStyle)
                .AppendTo(itemContainer);

            instructionListHolder = Builder
                .NewStackContainer("instructionHolder")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetBackground(Style.EntitiesMenu.MenuBg)
                .SetInnerPadding(Offset.All(5))
                .SetWidth(720)
                .SetItemSpacing(5);

            scrollHolder = Builder
                .NewStackContainer("scroll_holder")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetBackground(ColorRgba.DarkDarkGray)
                .SetHeight(450)
                .SetWidth(760)
                .AppendTo(itemContainer);

            scroll = Builder
                .NewScrollableContainer("scroll")
                .DisableHorizontalScroll()
                .AddVerticalScrollbar()
                .SetHeight(450)
                .SetWidth(760)
                .AppendTo(scrollHolder);

            scroll.AddItem(instructionListHolder);

            itemContainer.SetItemVisibility(scrollHolder, false);

            this.SetWidth(760);

            itemContainer.SetSizeMode(StackContainer.SizeMode.Dynamic);

            selectionchanged = updaterBuilder.CreateSyncer(() => m_controller.SelectedEntity);

            AddUpdater(updaterBuilder.Build());

            AddInstructionAdder(itemContainer);

            m_repaintInstructions = true;
        }

        private void AddInstructionAdder(StackContainer itemContainer)
        {
            itemContainer.AppendDivider(5, Style.EntitiesMenu.MenuBg);

            var newInstruction = Builder
                .NewStackContainer("new")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetBackground(ColorRgba.DarkDarkGray)
                .SetHeight(20)
                .SetInnerPadding(Offset.LeftRight(5))
                .AppendTo(itemContainer);

            btnAdd = new Btn(Builder, "add")
                .OnClick(AddNewInstruction)
                .SetEnabled(false)
                .SetText(NewIds.Texts.Tools.Add)
                .SetSize(new Vector2(50, 20))
                .SetButtonStyle(Style.Global.GeneralBtn)
                .AppendTo(newInstruction);

            pick = Builder
                .NewDropdown("new_type")
                .SetSize(new Vector2(300, 20))
                .AppendTo(newInstruction);
        }

        private void AddNewInstruction()
        {
            m_controller.SelectedEntity.Instructions.Add(new Instruction(
                availableInstructions[typeNames[pick.Value]],
                m_controller.SelectedEntity.Context
            ));
            m_repaintInstructions = true;
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            base.SyncUpdate(gameTime);
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (errorInScript.HasChanged)
            {
                var text = errorInScript.GetValueAndReset();
                instructionError.SetText(text);
                ItemsContainer.SetItemVisibility(instructionError, !text.IsNullOrEmpty());
            }

            if (m_controller.SelectedEntity != null && Builder != null && (m_repaintInstructions || selectionchanged.HasChanged))
            {
                selectionchanged.GetValueAndReset();

                m_repaintInstructions = false;

                instructionListHolder.ClearAndDestroyAll();

                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            instructionCount.SetText(NewIds.Texts.InstructionCount.Format(
                                (m_controller.SelectedEntity?.Instructions?.Count ?? 0).ToString()));

            this.ItemsContainer.SetItemVisibility(scrollHolder, (m_controller.SelectedEntity?.Instructions?.Count ?? 0) != 0);

            int index = 0;
            foreach (var instruction in new List<Instruction>(m_controller.SelectedEntity?.Instructions))
            {
                int i = index++;

                if (i > 0)
                    instructionListHolder.AppendDivider(5, Style.EntitiesMenu.MenuBg);

                var singleHolder = Builder
                    .NewStackContainer("instructionHolder_" + i + "_" + DateTime.Now.Ticks)
                    .SetStackingDirection(StackContainer.Direction.TopToBottom)
                    .SetBackground(ColorRgba.DarkDarkGray)
                    .SetInnerPadding(Offset.All(5))
                    .SetItemSpacing(5)
                    .AppendTo(instructionListHolder);

                AddNameAndHelp(i, instruction, singleHolder);
                AddInputs(instruction, singleHolder);
                instruction.Prototype.CustomUI?.Invoke(Builder, instructionListHolder);
                AddControls(instruction, i, singleHolder);
            }

            // TODO add
            availableInstructions = m_controller.Context.ProtosDb
                .All<InstructionProto>()
                .Where(p => p.InstructionLevel <= m_controller.SelectedEntity.Prototype.InstructionLevel)
                .ToDictionary(p => p.Id.Value);

            typeNames = availableInstructions.Keys.ToList();
            typeNames.Sort((v0, v1) => string.Compare(
                availableInstructions[v0].Strings.Name.TranslatedString,
                availableInstructions[v1].Strings.Name.TranslatedString));

            names = typeNames
                .Select(v => availableInstructions[v].Strings.Name.TranslatedString)
                .ToList();

            pick.ClearOptions();
            pick.AddOptions(names);
            btnAdd.SetEnabled(names.Count > 0);
        }

        private void AddNameAndHelp(int index, Instruction instruction, StackContainer instructionHolder)
        {
            var helpAndName = Builder
                .NewStackContainer("help_" + DateTime.Now.Ticks)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetHeight(20)
                .AppendTo(instructionHolder);

            Builder
                .NewTxt("line_" + DateTime.Now.Ticks)
                .SetText($"{index:D3} : ")
                .SetSize(100, 20)
                .AppendTo(helpAndName);

            helpAndName.AppendDivider(5, ColorRgba.DarkDarkGray);

            Builder
                .NewTitle(instruction.Prototype.Strings.Name)
                .AppendTo(helpAndName);

            helpAndName.AppendDivider(5, ColorRgba.DarkDarkGray);

            new Btn(Builder, "help_" + DateTime.Now.Ticks)
                .SetText("?")
                .SetButtonStyle(Style.Global.GeneralBtn)
                .AddToolTip(instruction.Prototype.Strings.DescShort)
                .SetSize(new Vector2(20, 20))
                .AppendTo(helpAndName);
        }

        private void AddControls(Instruction instruction, int i, StackContainer container)
        {
            var controls = Builder
                .NewStackContainer("controls_" + DateTime.Now.Ticks)
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetHeight(20)
                .AppendTo(container);

            // TODO delete, copy, paste, move up, move down
            new Btn(Builder, "remove_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_controller.SelectedEntity.Instructions.Remove(instruction);
                    m_repaintInstructions = true;
                })
                .SetText(NewIds.Texts.Tools.Remove)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(new Vector2(50, 20))
                .AppendTo(controls);

            new Btn(Builder, "copy_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_copiedInstruction = instruction;
                    m_repaintInstructions = true;
                })
                .SetText(NewIds.Texts.Tools.Copy)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(new Vector2(50, 20))
                .AppendTo(controls);

            new Btn(Builder, "paste_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_controller.SelectedEntity.Instructions[i] = m_copiedInstruction.Clone();
                    m_repaintInstructions = true;
                })
                .SetEnabled(m_copiedInstruction != null)
                .SetText(NewIds.Texts.Tools.Paste)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(new Vector2(50, 20))
                .AppendTo(controls);

            new Btn(Builder, "up_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    var old = m_controller.SelectedEntity.Instructions[i];
                    m_controller.SelectedEntity.Instructions[i] = m_controller.SelectedEntity.Instructions[i - 1];
                    m_controller.SelectedEntity.Instructions[i - 1] = old;
                    m_repaintInstructions = true;
                })
                .SetEnabled(i - 1 >= 0)
                .SetText(NewIds.Texts.Tools.Up)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(new Vector2(100, 20))
                .AppendTo(controls);

            new Btn(Builder, "down_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    var old = m_controller.SelectedEntity.Instructions[i];
                    m_controller.SelectedEntity.Instructions[i] = m_controller.SelectedEntity.Instructions[i + 1];
                    m_controller.SelectedEntity.Instructions[i + 1] = old;
                    m_repaintInstructions = true;
                })
                .SetEnabled(i + 1 < m_controller.SelectedEntity.Instructions.Count)
                .SetText(NewIds.Texts.Tools.Down)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(new Vector2(100, 20))
                .AppendTo(controls);

            container.AppendDivider(5, ColorRgba.DarkDarkGray);
        }

        private void AddInputs(Instruction instruction, StackContainer instructionHolder)
        {
            instructionHolder.AppendDivider(5, ColorRgba.DarkDarkGray);

            if (instruction.Inputs.Length == 0)
            {
                var container = Builder
                    .NewStackContainer("none_" + DateTime.Now.Ticks)
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetHeight(40)
                    .AppendTo(instructionHolder);

                Builder
                    .NewIconContainer("icon_" + DateTime.Now.Ticks)
                    .SetIcon(Style.Icons.Empty)
                    .SetSize(new Vector2(40, 40))
                    .AppendTo(container);

                container.AppendDivider(5, ColorRgba.DarkDarkGray);

                Builder
                    .NewTxt("text_" + DateTime.Now.Ticks)
                    .SetText(Tr.Empty)
                    .SetAlignment(TextAnchor.MiddleLeft)
                    .AppendTo(container);

                return;
            }

            for (int i = 0; i < instruction.Inputs.Length; i++)
            {
                var variableContainer = Builder
                    .NewStackContainer("instruction_" + DateTime.Now.Ticks)
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetSizeMode(StackContainer.SizeMode.Dynamic)
                    .AppendTo(instructionHolder);

                Builder
                    .NewTxt("varname_" + DateTime.Now.Ticks)
                    .SetText(instruction.Prototype.Inputs[i].Strings.Name)
                    .SetWidth(100)
                    .SetAlignment(TextAnchor.UpperLeft)
                    .AppendTo(variableContainer);

                var tabs = new MyTabContainer(Builder, Style, "picker_" + DateTime.Now.Ticks, () => m_repaintInstructions = true)
                    .AppendTo(variableContainer);

                MemoryPointer input = instruction.Inputs[i];
                if (input == null)
                    throw new InvalidCastException($"Input is null");

                List<IRefreshable> allVariants = new List<IRefreshable>();
                Action refresh = () =>
                {
                    foreach (var variant in allVariants)
                    {
                        variant.Refresh();
                    }
                };
                foreach (InstructionProto.InputType type in instruction.Prototype.Inputs[i].Types)
                {
                    if (type == InstructionProto.InputType.Variable)
                        AddVariableInput(tabs, input, allVariants, refresh);
                    if (type == InstructionProto.InputType.Instruction)
                        AddInstructionInput(tabs, input, allVariants, refresh);
                    if (type.HasFlag(InstructionProto.InputType.Entity))
                        AddEntityInput(tabs, type & InstructionProto.InputType.EntityGroup, input, allVariants, refresh);
                    if (type.HasFlag(InstructionProto.InputType.Boolean))
                        AddBooleanInput(tabs, input, allVariants, refresh);
                }
            }
        }

        private void AddVariableInput(MyTabContainer tabs, MemoryPointer input, List<IRefreshable> refreshables, Action refresh)
        {
            var variableTab = new VariableTab(Builder, Style, m_controller.SelectedEntity, input, refresh);
            refreshables.Add(variableTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Variable], variableTab);
            if (input.Type == InstructionProto.InputType.Variable)
                tabs.SwitchToTab(variableTab);
        }

        private void AddEntityInput(MyTabContainer tabs, InstructionProto.InputType type, MemoryPointer input, List<IRefreshable> refreshables, Action refresh)
        {
            var entityTab = new EntityTab(Builder, Style, m_controller.SelectedEntity, type, input, m_controller.Context, 10.ToFix32(), refresh);
            refreshables.Add(entityTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[type], entityTab);
            if (input.Type.HasFlag(InstructionProto.InputType.Entity))
                tabs.SwitchToTab(entityTab);
        }

        private void AddBooleanInput(MyTabContainer tabs, MemoryPointer input, List<IRefreshable> refreshables, Action refresh)
        {
            var variableTab = new BooleanTab(Builder, Style, input, refresh);
            refreshables.Add(variableTab);
            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Boolean], variableTab);

            if (input.Type == InstructionProto.InputType.Boolean)
                tabs.SwitchToTab(variableTab);
        }

        private void AddInstructionInput(MyTabContainer tabs, MemoryPointer input, List<IRefreshable> refreshables, Action refresh)
        {
            var variableTab = new InstructionTab(Builder, Style, input, m_controller.SelectedEntity.Instructions, refresh);
            refreshables.Add(variableTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Instruction], variableTab);
            if (input.Type == InstructionProto.InputType.Instruction)
                tabs.SwitchToTab(variableTab);
        }
    }
}
