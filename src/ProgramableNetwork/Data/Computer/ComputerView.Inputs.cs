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

        private void UpdateUI(Dictionary<IUiElement, Action> displayChanged)
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

                // displays
                int displayIndex = 0;
                if (instruction.Displays.Length > 0)
                {
                    int j = displayIndex++;

                    var displayHolder = Builder
                        .NewStackContainer("displayHolder_" + i + "_" + DateTime.Now.Ticks)
                        .SetStackingDirection(StackContainer.Direction.LeftToRight)
                        .SetBackground(ColorRgba.DarkDarkGray)
                        .SetInnerPadding(Offset.All(5))
                        .SetHeight(30)
                        .SetItemSpacing(5)
                        .SetSizeMode(StackContainer.SizeMode.Dynamic)
                        .AppendTo(instructionListHolder);

                    foreach (var display in instruction.Displays)
                    {
                        var name = Builder
                            .NewTxt("displayName_" + i + "_" + j + "_" + DateTime.Now.Ticks)
                            .SetText(instruction.Prototype.Displays[j].Name + ":")
                            .SetWidth(200)
                            .SetAlignment(TextAnchor.MiddleLeft)
                            .AppendTo(displayHolder);

                        var icon = Builder
                            .NewIconContainer("displayIcon_" + i + "_" + j + "_" + DateTime.Now.Ticks)
                            .SetSize(40, 40)
                            .AppendTo(displayHolder);

                        var text = Builder
                            .NewTxt("displayText_" + i + "_" + j + "_" + DateTime.Now.Ticks)
                            .SetWidth(200)
                            .SetAlignment(TextAnchor.MiddleLeft)
                            .AppendTo(displayHolder);

                        displayHolder.SetItemVisibility(text, false);
                        displayHolder.SetItemVisibility(icon, false);

                        displayChanged.Add(icon, (Action)(() =>
                        {
                            try
                            {
                                if (display.Type == InstructionProto.InputType.Product)
                                {
                                    icon.SetIcon(display.Product.IconPath);
                                    displayHolder.SetHeight(40);
                                    displayHolder.SetItemVisibility(icon, true);
                                }
                                else if (display.Type == InstructionProto.InputType.Entity)
                                {
                                    icon.SetIcon(display.Entity.GetIcon());
                                    displayHolder.SetHeight(40);
                                    displayHolder.SetItemVisibility(icon, true);
                                }
                                else
                                {
                                    displayHolder.SetItemVisibility(icon, false);
                                }
                            }
                            catch (Exception)
                            {
                                displayChanged.Remove(icon);
                            }
                        }));

                        displayChanged.Add(text, (Action)(() =>
                        {
                            try
                            {
                                if (display.Type == InstructionProto.InputType.Integer)
                                {
                                    text.SetText(display.Integer.ToString());
                                    displayHolder.SetHeight(30);
                                    displayHolder.SetItemVisibility(text, true);
                                }
                                else if (display.Type == InstructionProto.InputType.Boolean)
                                {
                                    text.SetText(display.Boolean.ToString());
                                    displayHolder.SetHeight(30);
                                    displayHolder.SetItemVisibility(text, true);
                                }
                                else if (display.Type == InstructionProto.InputType.Instruction)
                                {
                                    text.SetText(m_controller.SelectedEntity.GetInstructionIndex(display.Instruction).ToString("D3"));
                                    displayHolder.SetHeight(30);
                                    displayHolder.SetItemVisibility(text, true);
                                }
                                else
                                {
                                    displayHolder.SetItemVisibility(text, false);
                                }
                            }
                            catch (Exception)
                            {
                                displayChanged.Remove(text);
                            }
                        }));
                    }
                }
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
