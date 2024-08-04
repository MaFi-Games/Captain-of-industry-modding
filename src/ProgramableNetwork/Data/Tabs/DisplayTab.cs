using Mafi;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    internal class DisplayTab : StackContainer, IRefreshable
    {
        private readonly UiData uiData;

        private InstructionProto.InputType oldType;
        private long oldValueNumber;
        private string oldValueString;

        public DisplayTab(UiData uiData)
            : base(uiData.Builder, "display_" + DateTime.Now.Ticks)
        {
            this.uiData = uiData;
            this.uiData.AddToRefreshable(this);

            SetStackingDirection(Direction.LeftToRight);
            SetSizeMode(SizeMode.Dynamic);

            oldType = InstructionProto.InputType.None;

            Refresh();
        }

        public void Refresh()
        {
            if (oldType == uiData.Input.Type &&
                oldValueNumber == uiData.Input.Data &&
                oldValueString == uiData.Input.SData) return;

            oldType = uiData.Input.Type;
            oldValueNumber = uiData.Input.Data;
            oldValueString = uiData.Input.SData;

            switch (uiData.Input.Type)
            {
                case InstructionProto.InputType.Instruction:
                    initText($"{uiData.Computer.Instructions.FindIndex(i => i.UniqueId == uiData.Input.Instruction):D3}");
                    break;
                case InstructionProto.InputType.Boolean:
                    initText(NewIds.Texts.Boolean[uiData.Input]);
                    break;
                case InstructionProto.InputType.Integer:
                    initText(uiData.Input.Integer.ToString());
                    break;
                case InstructionProto.InputType.Entity:
                    initButton(uiData.Input.Entity.GetIcon(),
                        () =>
                        {
                            if (uiData.Input.Entity.HasPosition(out Tile2f position))
                                uiData.Context.CameraController.PanTo(position);
                        },
                        () =>
                        {
                            uiData.Context.Highlighter.Highlight(
                                (IRenderedEntity)uiData.Input.Entity, ColorRgba.DarkYellow);
                        },
                        () =>
                        {
                            uiData.Context.Highlighter.RemoveHighlight(
                                (IRenderedEntity)uiData.Input.Entity);
                        }
                        );
                    break;
                case InstructionProto.InputType.Product:
                    initImage(uiData.Input.Product.IconPath);
                    break;
                default:
                    break;
            }
        }

        private void initText(string text)
        {
            this.ClearAndDestroyAll();
            this.SetSize(400, 20);

            this.uiData.Builder
                .NewBtnGeneral("btn_" + DateTime.Now.Ticks)
                .SetText(text)
                .SetButtonStyle(uiData.Style.Global.GeneralBtnActive)
                .SetSize(400, 20)
                .AppendTo(this);
        }

        private void initText(LocStrFormatted text)
        {
            this.ClearAndDestroyAll();
            this.SetSize(400, 20);

            this.uiData.Builder
                .NewBtnGeneral("btn_" + DateTime.Now.Ticks)
                .SetText(text)
                .SetButtonStyle(uiData.Style.Global.GeneralBtnActive)
                .SetSize(400, 20)
                .AppendTo(this);
        }

        private void initImage(string image)
        {
            this.ClearAndDestroyAll();
            this.SetSize(40, 40);

            this.uiData.Builder
                .NewBtnGeneral("btn_" + DateTime.Now.Ticks)
                .SetButtonStyle(uiData.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(image)
                .AppendTo(this);
        }

        private void initButton(string image, Action onClick, Action onMouseEnter, Action onMouseLeave)
        {
            this.ClearAndDestroyAll();

            this.uiData.Builder
                .NewBtnGeneral("btn_" + DateTime.Now.Ticks)
                .SetButtonStyle(uiData.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(image)
                .OnClick(onClick)
                .SetOnMouseEnterLeaveActions(onMouseEnter, onMouseLeave)
                .AppendTo(this);
        }
    }
}