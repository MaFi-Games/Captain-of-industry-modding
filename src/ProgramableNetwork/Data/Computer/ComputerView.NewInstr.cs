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
        private Dictionary<string, InstructionProto> availableInstructions;
        private List<string> typeNames;
        private List<string> names;
        private Computer lastSelection;
        private Dropdwn pick;
        private Btn btnAdd;

        private void AddInstructionAdder(StackContainer itemContainer, UpdaterBuilder updaterBuilder)
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

            updaterBuilder.Observe(() => (lastSelection, lastSelection = m_controller.SelectedEntity ?? lastSelection))
                .Do(((Computer oldComputer, Computer newComputer) change) =>
                {
                    if (change.newComputer != null && change.newComputer.Prototype != change.oldComputer?.Prototype)
                        UpdateNewInstructionSelection(change.newComputer);
                });
        }

        private void UpdateNewInstructionSelection(Computer newComputer)
        {
            availableInstructions = m_controller.Context.ProtosDb
                .All<InstructionProto>()
                .Where(p => p.InstructionLevel <= newComputer.Prototype.InstructionLevel)
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

        private void AddNewInstruction()
        {
            m_controller.SelectedEntity.Instructions.Add(new Instruction(
                availableInstructions[typeNames[pick.Value]],
                Entity
            ));
            m_repaintInstructions = true;
        }
    }
}
