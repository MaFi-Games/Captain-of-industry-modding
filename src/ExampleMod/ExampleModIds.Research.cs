using Mafi.Base;
using Mafi.Core.Research;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace ExampleMod;

public partial class ExampleModIds {

	public partial class Research {

		[ResearchCosts(difficulty: 1)]
		public static readonly ResNodeID UnlockExampleModStuff = Ids.Research.CreateId("UnlockExampleModStuff");

	}

}