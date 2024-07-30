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
        private readonly List<IRefreshable> refreshed = new List<IRefreshable>();

        private void UpdateUI()
        {
            refreshed.Clear();

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
                    .SetSizeMode(StackContainer.SizeMode.Dynamic)
                    .AppendTo(instructionListHolder);

                AddNameAndHelp(i, instruction, singleHolder);
                AddInputs(instruction, singleHolder);
                AddDisplays(instruction, singleHolder);
                AddControls(instruction, i, singleHolder);
            }
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

            UiData uiData = new UiData(Builder, Style, instruction.Prototype, Entity, m_controller.Context);

            if (instruction.Inputs.Length == 0)
            {
                instruction.Prototype.CustomUI(uiData);
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

                var tabs = new MyTabContainer(Builder, Style, "picker_" + DateTime.Now.Ticks, uiData.Refresh)
                    .AppendTo(variableContainer);

                MemoryPointer input = instruction.Inputs[i];
                if (input == null)
                    throw new InvalidCastException($"Input is null");

                UiData inputUiData = uiData.ForInput(i, input);

                foreach (InstructionProto.InputType type in instruction.Prototype.Inputs[i].Types)
                {
                    if (type == InstructionProto.InputType.Variable)
                        AddVariableInput(tabs, input, uiData);
                    if (type == InstructionProto.InputType.Instruction)
                        AddInstructionInput(tabs, input, uiData);
                    if (type.HasFlag(InstructionProto.InputType.Entity))
                        AddEntityInput(tabs, input, uiData, instruction.Prototype);
                    if (type.HasFlag(InstructionProto.InputType.Boolean))
                        AddBooleanInput(tabs, input, uiData);
                    if (type.HasFlag(InstructionProto.InputType.Integer))
                        AddIntegerInput(tabs, input, uiData);
                    if (type.HasFlag(InstructionProto.InputType.Product))
                        AddProductInput(tabs, input, uiData, instruction.Prototype);
                }

                instruction.Prototype.CustomUI(inputUiData);
            }
        }

        private void AddDisplays(Instruction instruction, StackContainer instructionHolder)
        {
            if (instruction.Displays.Length == 0)
            {
                return;
            }

            instructionHolder.AppendDivider(5, ColorRgba.DarkDarkGray);

            UiData uiData = new UiData(Builder, Style, instruction.Prototype, Entity, m_controller.Context);
            refreshed.Add(uiData);

            for (int i = 0; i < instruction.Displays.Length; i++)
            {
                var variableContainer = Builder
                    .NewStackContainer("instruction_" + DateTime.Now.Ticks)
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetSizeMode(StackContainer.SizeMode.Dynamic)
                    .AppendTo(instructionHolder);

                Builder
                    .NewTxt("varname_" + DateTime.Now.Ticks)
                    .SetText(instruction.Prototype.Displays[i].Name)
                    .SetWidth(100)
                    .SetAlignment(TextAnchor.UpperLeft)
                    .AppendTo(variableContainer);

                MemoryPointer input = instruction.Displays[i];
                if (input == null)
                    throw new InvalidCastException($"Input is null");

                new DisplayTab(uiData.ForInput(i, input))
                    .AppendTo(variableContainer);
            }
        }

        private void AddVariableInput(MyTabContainer tabs, MemoryPointer input, UiData uiData)
        {
            var variableTab = new VariableTab(Builder, Style, m_controller.SelectedEntity, input, uiData.Refresh);
            uiData.AddToRefreshable(variableTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Variable], variableTab);
            if (input.Type == InstructionProto.InputType.Variable)
                tabs.SwitchToTab(variableTab);
        }

        private void AddEntityInput(MyTabContainer tabs, MemoryPointer input, UiData uiData, InstructionProto instruction)
        {
            var entityTab = new EntityTab(Builder, m_controller.SelectedEntity, instruction, input, uiData.Refresh, this, m_controller.Context);
            uiData.AddToRefreshable(entityTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Entity], entityTab);
            if (input.Type == InstructionProto.InputType.Entity)
                tabs.SwitchToTab(entityTab);
        }

        private void AddBooleanInput(MyTabContainer tabs, MemoryPointer input, UiData uiData)
        {
            var variableTab = new BooleanTab(Builder, Style, input, uiData.Refresh);
            uiData.AddToRefreshable(variableTab);
            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Boolean], variableTab);

            if (input.Type == InstructionProto.InputType.Boolean)
                tabs.SwitchToTab(variableTab);
        }

        private void AddIntegerInput(MyTabContainer tabs, MemoryPointer input, UiData uiData)
        {
            var variableTab = new IntegerTab(Builder, Style, input, uiData.Refresh);
            uiData.AddToRefreshable(variableTab);
            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Integer], variableTab);

            if (input.Type == InstructionProto.InputType.Integer)
                tabs.SwitchToTab(variableTab);
        }

        private void AddInstructionInput(MyTabContainer tabs, MemoryPointer input, UiData uiData)
        {
            var variableTab = new InstructionTab(Builder, Style, input, m_controller.SelectedEntity.Instructions, uiData.Refresh);
            uiData.AddToRefreshable(variableTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Instruction], variableTab);
            if (input.Type == InstructionProto.InputType.Instruction)
                tabs.SwitchToTab(variableTab);
        }

        private void AddProductInput(MyTabContainer tabs, MemoryPointer input, UiData uiData, InstructionProto instruction)
        {
            var variableTab = new ProductTab(Builder, m_controller.SelectedEntity, instruction, input, uiData.Refresh, this, m_controller.Context);
            uiData.AddToRefreshable(variableTab);

            tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Product], variableTab);
            if (input.Type == InstructionProto.InputType.Product)
                tabs.SwitchToTab(variableTab);
        }
    }
}
