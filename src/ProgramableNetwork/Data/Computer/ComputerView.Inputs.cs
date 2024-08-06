using Mafi;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mafi.Unity.UiFramework;

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
                    .SetStackingDirection(StackContainer.Direction.LeftToRight)
                    .SetBackground(ColorRgba.DarkDarkGray)
                    .SetInnerPadding(Offset.All(5))
                    .SetItemSpacing(5)
                    .SetHeight(30)
                    .SetSizeMode(StackContainer.SizeMode.Dynamic)
                    .AppendTo(instructionListHolder);

                AddNameAndHelp(i, instruction, singleHolder);
                AddControls(instruction, i, singleHolder);
            }
        }

        private void AddNameAndHelp(int index, Instruction instruction, StackContainer instructionHolder)
        {
            new Btn(Builder, "help_" + DateTime.Now.Ticks)
                .SetText("?")
                .SetButtonStyle(Style.Global.GeneralBtn)
                .AddToolTip(instruction.Prototype.Strings.DescShort)
                .SetSize(20, 20)
                .AppendTo(instructionHolder, ContainerPosition.LeftOrTop);
            
            Builder
                .NewTxt("line_" + DateTime.Now.Ticks)
                .SetText($"{index:D3} : ")
                .SetSize(50, 20)
                .AppendTo(instructionHolder, ContainerPosition.LeftOrTop);

            Builder
                .NewTitle(instruction.Prototype.Strings.Name)
                .SetAlignment(TextAnchor.MiddleLeft)
                .SetHeight(20)
                .AppendTo(instructionHolder, ContainerPosition.LeftOrTop);
        }

        private void AddControls(Instruction instruction, int i, StackContainer controls)
        {
            // TODO delete, copy, paste, move up, move down
            new InstructionEditButton(Builder, Entity, instruction, () => { }, this, m_controller)
                .AppendTo(controls, ContainerPosition.RightOrBottom);

            new Btn(Builder, "remove_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_controller.SelectedEntity.Instructions.Remove(instruction);
                    m_repaintInstructions = true;
                })
                .SetText(NewIds.Texts.Tools.Remove)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(50, 20)
                .AppendTo(controls, ContainerPosition.RightOrBottom);

            new Btn(Builder, "copy_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_copiedInstruction = instruction;
                    m_repaintInstructions = true;
                })
                .SetText(NewIds.Texts.Tools.Copy)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(50, 20)
                .AppendTo(controls, ContainerPosition.RightOrBottom);

            new Btn(Builder, "paste_" + DateTime.Now.Ticks)
                .OnClick(() =>
                {
                    m_controller.SelectedEntity.Instructions[i] = m_copiedInstruction.Clone();
                    m_repaintInstructions = true;
                })
                .SetEnabled(m_copiedInstruction != null)
                .SetText(NewIds.Texts.Tools.Paste)
                .SetButtonStyle(Style.Global.GeneralBtn)
                .SetSize(50, 20)
                .AppendTo(controls, ContainerPosition.RightOrBottom);

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
                .SetSize(50, 20)
                .AppendTo(controls, ContainerPosition.RightOrBottom);

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
                .SetSize(50, 20)
                .AppendTo(controls, ContainerPosition.RightOrBottom);
        }
    }
}
