using Mafi.Base;
using MachineID = Mafi.Core.Factory.Machines.MachineProto.ID;

namespace ExampleMod;

public partial class ExampleModIds {

	public partial class Machines {

		public static readonly MachineID ExampleFurnace = Ids.Machines.CreateId("ExampleFurnace");

	}

}