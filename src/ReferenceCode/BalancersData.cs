using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Zippers;
using Mafi.Core.Mods;
using Mafi.Core.Ports.Io;
using Mafi.Core.Prototypes;
using static Mafi.Base.Assets.Base.Zippers;

namespace ExampleMod.ReferenceCode {
	/// <summary>
	/// In order to add your new balancers to the game, add following code to your mod registration.
	/// <code>
	/// registrator.RegisterData{BalancersData}(); // replace { and } with angle brackets (thanks, XML comments).
	/// </code>
	/// </summary>
	public class BalancersData : IModData {
		public void RegisterData(ProtoRegistrator registrator) {
			ProtosDb db = registrator.PrototypesDb;
			const bool DISABLE_COSTS = true;

			registerBalancer(registrator,
				name: "My flat balancer",
				portShape: db.GetOrThrow<IoPortShapeProto>(Ids.IoPortShapes.FlatConveyor),
				costs: Costs.Transports.FlatZipper,
				prefabPath: CountableZipper_prefab,
				disableCosts: DISABLE_COSTS);

			registerBalancer(registrator,
				name: "My U-shape balancer",
				portShape: db.GetOrThrow<IoPortShapeProto>(Ids.IoPortShapes.LooseMaterialConveyor),
				costs: Costs.Transports.LooseZipper,
				prefabPath: LooseZipper_prefab,
				disableCosts: DISABLE_COSTS);

			registerBalancer(registrator,
				name: "My pipe balancer",
				portShape: db.GetOrThrow<IoPortShapeProto>(Ids.IoPortShapes.Pipe),
				costs: Costs.Transports.FluidZipper,
				prefabPath: LiquidZipper_prefab,
				disableCosts: DISABLE_COSTS);

		}

		private static void registerBalancer(
			ProtoRegistrator registrator,
			string name,
			IoPortShapeProto portShape,
			EntityCostsTpl costs,
			string prefabPath,
			bool disableCosts
		) {
			StaticEntityProto.ID id = new StaticEntityProto.ID("MyBalancer_" + portShape.Id.Value);
			registrator.PrototypesDb.Add(new ZipperProto(
				id: id,
				strings: Proto.CreateStr(id, name,
					"Allows distributing and prioritizing products using any of its two input and output ports.",
					translationComment: "small machine that allows splitting and merging of transports"),
				costs: costs.MapToEntityCosts(registrator.PrototypesDb, disableCosts),
				requiredPower: 5.Kw(),
				layout: registrator.LayoutParser.ParseLayoutOrThrow(
					// Allow port connection only to transports, otherwise balancing logic can is broken.
					new EntityLayoutParams(useNewLayoutSyntax: true, portsCanOnlyConnectToTransports: true),
					"E?+[1][1]+?B".Replace('?', portShape.LayoutChar),
					"F?+[1][1]+?A".Replace('?', portShape.LayoutChar)),
				hasPriorities: true,
				graphics: new LayoutEntityProto.Gfx(
					prefabPath: prefabPath,
					color: ColorRgba.White,
					hideBlockedPortsIcon: true,
					categories: registrator.GetCategoriesProtos(Ids.ToolbarCategories.Transports))));
		}
	}
}
