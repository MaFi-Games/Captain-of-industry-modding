using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;

namespace ExampleMod {
	internal class ExampleResearchData : IResearchNodesData {

		public void RegisterData(ProtoRegistrator registrator) {

			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Unlock MyMod stuff!", MyIds.Research.UnlockMyModStuff)
				.Description("This unlocks all the awesome stuff in MyMod!")
				.AddProductToUnlock(MyIds.Products.MySlickLooseProduct)
				.AddProductToUnlock(MyIds.Products.MyAmazingFluidProduct)
				.AddProductToUnlock(MyIds.Products.MyExcitingUnitProduct)
				.AddProductToUnlock(MyIds.Products.MyMajesticMoltenProduct)
				.AddRecipeToUnlock(MyIds.Recipes.MySmoothSmelting)
				.BuildAndAdd();

			nodeProto.GridPosition = new Vector2i(4, 31);
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.BasicFarming));

		}

	}
}
