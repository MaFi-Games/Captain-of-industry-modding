using Mafi.Base;
using Mafi.Base.Prototypes;
using ProductID = Mafi.Core.Products.ProductProto.ID;

namespace ExampleMod {
	public static partial class MyIds {

		public static partial class Products {

			[CountableProduct(icon: Assets.Base.Products.Icons.Wood_svg,
				prefab: Assets.Base.Products.Countable.RawWood_prefab)]
			public static readonly ProductID MyExcitingUnitProduct = Ids.Products.CreateId("MyExcitingUnitProduct");


			[FluidProduct(color: 0xFF00FF, icon: Assets.Base.Products.Icons.Water_svg)]
			public static readonly ProductID MyAmazingFluidProduct = Ids.Products.CreateId("MyAmazingFluidProduct");


			[LooseProduct(pile: PileFamily.DirtLike, dumpByDefault: true,
				material: Assets.Base.Products.Loose.DirtConveyorPile_mat,
				prefab: Assets.Base.Products.Loose.DirtConveyor_prefab,
				icon: Assets.Base.Products.Icons.Dirt_svg)]
			public static readonly ProductID MySlickLooseProduct = Ids.Products.CreateId("MySlickLooseProduct");


			[MoltenProduct(material: Assets.Base.Products.Molten.Copper_mat,
				prefab: Assets.Base.Products.Molten.MoltenCopper_prefab,
				icon: Assets.Base.Products.Icons.CopperMolten_svg)]
			public static readonly ProductID MyMajesticMoltenProduct = Ids.Products.CreateId("MyMajesticMoltenProduct");

		}

	}
}
