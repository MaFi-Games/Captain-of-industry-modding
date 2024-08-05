using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class InstructionEditButton : StackContainer
    {
        private readonly UiBuilder m_builder;
        private readonly Computer m_computer;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly Instruction m_instruction;
        private readonly StackContainer m_btnPreviewHolder;
        private InstructionView m_instructionView;
        private Btn m_btnEdit;

        public InstructionEditButton(UiBuilder builder, Computer computer, Instruction instruction,
            Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "instruction_edit_" + instruction.UniqueId)
        {
            m_builder = builder;
            m_computer = computer;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;
            m_instruction = instruction;
            m_refresh = refresh;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + instruction.UniqueId)
                .SetStackingDirection(Direction.TopToBottom)
                .SetSizeMode(SizeMode.Dynamic)
                .SetSize(50, 20)
                .AppendTo(this);

            m_btnEdit = m_builder
                .NewBtnGeneral("item_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                .SetText(NewIds.Texts.Tools.Edit)
                .SetSize(50, 20)
                .OnClick(OpenWindow)
                .AppendTo(m_btnPreviewHolder);

            this.SetSizeMode(SizeMode.Dynamic);
            this.SetSize(50, 20);
        }

        private void OpenWindow()
        {
            if (m_instructionView == null)
            {
                m_instructionView = new InstructionView(this);
                m_window.SetupInnerWindowWithButton(m_instructionView, m_btnPreviewHolder, m_btnEdit, () => {
                    try
                    {
                        m_btnPreviewHolder.ClearAndDestroyAll();
                    }
                    catch (Exception e)
                    {
                        //Log.Error($"Dialog closing save failed: {e.Message}");
                    }
                    try
                    {
                        m_btnEdit = m_builder
                            .NewBtnGeneral("item_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.GeneralBtn)
                            .SetText(NewIds.Texts.Tools.Edit)
                            .SetSize(50, 20)
                            .OnClick(OpenWindow)
                            .AppendTo(m_btnPreviewHolder);
                    }
                    catch (Exception e)
                    {
                        //Log.Error($"Dialog closing save failed: {e.Message}");
                    }
                }, m_refresh);
                m_window.OnHide += () =>
                {
                    try
                    {
                        m_instructionView.Hide();
                    }
                    catch (Exception e)
                    {
                        //Log.Error($"Dialog closing parent failed: {e.Message}");
                    }
                };
                m_instructionView.BuildAndShow(m_builder);
            }
            else
            {
                m_instructionView.Show();
            }
        }

        private class InstructionView : WindowView
        {
            private InstructionEditButton m_instructionButton;
            private StackContainer m_grid;
            private List<IRefreshable> refreshed;

            public InstructionView(InstructionEditButton instructionButto)
                :base("instruction_edit_view")
            {
                this.m_instructionButton = instructionButto;
                SetOnCloseButtonClickAction(this.Hide);
            }

            protected override void BuildWindowContent()
            {
                string computerName = m_instructionButton.m_computer.CustomTitle.ValueOrNull ?? m_instructionButton.m_computer.DefaultTitle.Value;
                int instructionIndex = m_instructionButton.m_computer.Instructions
                    .SelectIndicesWhere(i => i.UniqueId == m_instructionButton.m_instruction.UniqueId)
                    .First();
                this.m_headerText.SetText($"{computerName}: {instructionIndex:D3}");

                SetContentSize(500, 300);

                var scroll = Builder
                    .NewScrollableContainer("items-scroll")
                    .AddVerticalScrollbar()
                    .SetSize(500, 460)
                    .PutTo(GetContentPanel());

                GetContentPanel()
                    .SetSize(500, 300)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg);

                m_grid = Builder
                    .NewStackContainer("items-grid")
                    .SetItemSpacing(5)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg)
                    .SetStackingDirection(Direction.TopToBottom)
                    .SetInnerPadding(Offset.All(5));

                scroll.AddItem(m_grid);

                refreshed = new List<IRefreshable>();

                AddInputs(m_instructionButton.m_instruction, m_grid);
                AddDisplays(m_instructionButton.m_instruction, m_grid);
            }

            private void AddInputs(Instruction instruction, StackContainer instructionHolder)
            {
                instructionHolder.AppendDivider(5, ColorRgba.DarkDarkGray);

                UiData uiData = new UiData(Builder, Style, instruction.Prototype, m_instructionButton.m_computer, m_instructionButton.m_inspectorContext);

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
                        .SetTabDynamicHeight(3)
                        .SetTabCellSize(new Vector2(100, 20))
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
                            AddEntityInput(tabs, input, uiData, instruction.Prototype, type);
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

                UiData uiData = new UiData(Builder, Style, instruction.Prototype, m_instructionButton.m_computer, m_instructionButton.m_inspectorContext);
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
                var variableTab = new VariableTab(Builder, Style, m_instructionButton.m_computer, input, uiData.Refresh);
                uiData.AddToRefreshable(variableTab);

                tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Variable], variableTab);
                if (input.Type == InstructionProto.InputType.Variable)
                    tabs.SwitchToTab(variableTab);
            }

            private void AddEntityInput(MyTabContainer tabs, MemoryPointer input, UiData uiData, InstructionProto instruction, InstructionProto.InputType type)
            {
                var entityTab = new EntityTab(Builder, m_instructionButton.m_computer, instruction, type, input, uiData.Refresh, m_instructionButton.m_window, m_instructionButton.m_inspectorContext);
                uiData.AddToRefreshable(entityTab);

                tabs.AddTab(NewIds.Texts.PointerTypes[type], entityTab);
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
                var variableTab = new InstructionTab(Builder, Style, input, m_instructionButton.m_computer.Instructions, uiData.Refresh);
                uiData.AddToRefreshable(variableTab);

                tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Instruction], variableTab);
                if (input.Type == InstructionProto.InputType.Instruction)
                    tabs.SwitchToTab(variableTab);
            }

            private void AddProductInput(MyTabContainer tabs, MemoryPointer input, UiData uiData, InstructionProto instruction)
            {
                var variableTab = new ProductTab(Builder, m_instructionButton.m_computer, instruction, input, uiData.Refresh, m_instructionButton.m_window, m_instructionButton.m_inspectorContext);
                uiData.AddToRefreshable(variableTab);

                tabs.AddTab(NewIds.Texts.PointerTypes[InstructionProto.InputType.Product], variableTab);
                if (input.Type == InstructionProto.InputType.Product)
                    tabs.SwitchToTab(variableTab);
            }
        }
    }
}