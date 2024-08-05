using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Computers
        {
            public static readonly StaticEntityProto.ID Computer = new StaticEntityProto.ID("ProgramableNetwork_Computer");
            public static readonly StaticEntityProto.ID Database = new StaticEntityProto.ID("ProgramableNetwork_Database");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Computer = "Assets/ProgramableNetwork/Computer/Icon.png";
                public static readonly string Database = "Assets/ProgramableNetwork/Computer/Icon.png";
            }

            public static readonly string Computer = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Database = "Assets/ProgramableNetwork/Computer/Computer.prefab";
        }
    }

    internal class Computers : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var category = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Machines).ToImmutableArray();

            registrator.PrototypesDb.Add(new ComputerProto(
                id: NewIds.Computers.Computer,
                strings: Proto.CreateStr(NewIds.Computers.Computer, "Computer", "Handles basic operations and automatization"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                operationCount: 10,
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Computer,
                    customIconPath: NewAssets.Computers.Icons.Computer,
                    categories: category
                )
            ));

            registrator.PrototypesDb.Add(new ComputerProto(
                id: NewIds.Computers.Database,
                strings: Proto.CreateStr(NewIds.Computers.Database, "Database", "Holds only variables"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                operationCount: 20,
                variables: 20,
                iddlePower: 0.Kw(),
                workingPower: 1.Kw(),
                instructionLevel: 0,
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Database,
                    customIconPath: NewAssets.Computers.Icons.Database,
                    categories: category
                )
            ));
        }
    }
}
