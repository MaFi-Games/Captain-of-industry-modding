using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;

namespace ExampleMod;

internal class ExampleResearchData : IResearchNodesData {

	public void RegisterData(ProtoRegistrator registrator) {

		ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
			.Start("Unlock MyMod stuff!", ExampleModIds.Research.UnlockExampleModStuff, costMonths: 6)
			.Description("This unlocks all the awesome stuff in MyMod!")
			.AddProductToUnlock(ExampleModIds.Products.ExampleLooseProduct)
			.AddProductToUnlock(ExampleModIds.Products.ExampleFluidProduct)
			.AddProductToUnlock(ExampleModIds.Products.ExampleUnitProduct)
			.AddProductToUnlock(ExampleModIds.Products.ExampleMoltenProduct)
			.AddRecipeToUnlock(ExampleModIds.Recipes.ExampleSmelting)
			.BuildAndAdd();

		nodeProto.GridPosition = new Vector2i(4, 31);
		nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.BasicFarming));

	}

}