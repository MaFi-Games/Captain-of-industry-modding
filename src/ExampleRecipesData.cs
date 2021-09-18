using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace ExampleMod {
	internal class ExampleRecipesData : IModData {

		public void RegisterData(ProtoRegistrator registrator) {

			// Example of a new furnace recipe.
			registrator.RecipeProtoBuilder
				.Start(name: "My smooth smelting",
					recipeId: MyIds.Recipes.MySmoothSmelting,
					machineId: Ids.Machines.SmeltingFurnaceT1)
				.AddInput(8, MyIds.Products.MySlickLooseProduct)
				.AddInput(2, Ids.Products.Coal)
				.SetDuration(20.Seconds())
				.AddOutput(8, MyIds.Products.MyMajesticMoltenProduct)
				.AddOutput(24, Ids.Products.Exhaust, outputAtStart: true)
				.BuildAndAdd();

		}

	}
}
