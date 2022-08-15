using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Animations;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;

namespace ExampleMod;

internal class ExampleMachineData : IModData {

	public void RegisterData(ProtoRegistrator registrator) {
		registrator.MachineProtoBuilder
			.Start("Example furnace", ExampleModIds.Machines.ExampleFurnace)
			.Description("Testing furnace")
			.SetCost(Costs.Build.CP(80).Workers(10))
			// For examples of layouts see `Mafi.Base.BaseMod` and `EntityLayoutParser`.
			.SetLayout(new EntityLayoutParams(useNewLayoutSyntax: true),
				"   [2][2][2][3][3][3][3][3][2]>~Y",
				"   [2][2][3][5][5][7][7][4][3]   ",
				"A~>[2][2][3][5][5][7][7][4][3]>'V",
				"B~>[2][2][3][5][5][7][7][4][3]>'W",
				"   [2][2][2][3][3][7][7][4][3]   ",
				"   [2][2][2][2][2][2][2][2][3]>@E")
			.SetCategories(Ids.ToolbarCategories.MachinesMetallurgy)
			.SetPrefabPath("Assets/ExampleMod/BlastFurnace.prefab")
			.SetAnimationParams(
				animParams: AnimationParams.RepeatTimes(Duration.FromKeyframes(360),
				times: 2,
				changeSpeedToFit: true))
			.BuildAndAdd();

		// Example of a new furnace recipe.
		registrator.RecipeProtoBuilder
			.Start(name: "Example smelting",
				recipeId: ExampleModIds.Recipes.ExampleSmelting,
				machineId: ExampleModIds.Machines.ExampleFurnace)
			.AddInput(8, ExampleModIds.Products.ExampleLooseProduct)
			.AddInput(2, Ids.Products.Coal)
			.SetDuration(20.Seconds())
			.AddOutput(8, ExampleModIds.Products.ExampleMoltenProduct)
			.AddOutput(24, Ids.Products.Exhaust, outputAtStart: true)
			.BuildAndAdd();

	}
}