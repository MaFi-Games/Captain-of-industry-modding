using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace ProgramableNetwork
{
	public partial class NewIds
	{
		public partial class Research
		{
			[ResearchCosts(difficulty: 1)]
			public static readonly ResNodeID ProgramableNetwork_Stage1 = Ids.Research.CreateId("ProgramableNetwork_Stage1");
		}
	}

	internal class Research : AValidatedData, IResearchNodesData
	{

		protected override void RegisterDataInternal(ProtoRegistrator registrator)
		{
			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Programable Network", NewIds.Research.ProgramableNetwork_Stage1)
				.Description("Unlocks controlled input by condition")
				.SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
				.AddLayoutEntityToUnlock(NewIds.Computers.Computer)
				.AddProtoUnlockNoIcon(NewIds.Tools.SelectStaticEntity)
				.AddProtoUnlockNoIcon(NewIds.Tools.SelectDynamicEntity)
				.BuildAndAdd();
			
			nodeProto.GridPosition = new Vector2i(36, 15);
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.ResearchLab2));
		}

	}
}