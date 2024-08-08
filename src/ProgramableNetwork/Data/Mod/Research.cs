using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<ResNodeID, List<ComputerModuleProto.ID>> m_modules = new Dictionary<ResNodeID, List<ComputerModuleProto.ID>>();

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
		{
			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Programable Network", NewIds.Research.ProgramableNetwork_Stage1)
				.Description("Unlocks controlled input by condition")
				.SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
				.AddLayoutEntityToUnlock(NewIds.Computers.Computer)
				.AddTransportToUnlock(NewIds.Transport.Cable_T1)
				.AddProtoUnlockNoIcon(NewIds.Transport.CablePort)
				.BuildAndAdd();
			
			nodeProto.GridPosition = new Vector2i(36, 15);
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.ResearchLab2));
		}

        internal static void AddModule(ComputerModuleProto.ID id, ResNodeID stage)
        {
			if (!m_modules.TryGetValue(stage, out var moduleProtos))
            {
				m_modules[stage] = moduleProtos = new List<ComputerModuleProto.ID>();
            }

			moduleProtos.Add(id);
        }
    }
}