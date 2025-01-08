using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Gfx;
using Mafi.Core.Mods;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Controllers
        {
            public static readonly StaticEntityProto.ID Controller = new StaticEntityProto.ID("ProgramableNetwork_Computer");
            public static readonly StaticEntityProto.ID Antena = new StaticEntityProto.ID("ProgramableNetwork_Antena");
            public static readonly StaticEntityProto.ID Database = new StaticEntityProto.ID("ProgramableNetwork_Database");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Icon.png";
                public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Icon.png";
            }

            public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Antena.prefab";
        }
    }

    internal class Entities : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var category = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Machines).ToImmutableArray();

            registrator.PrototypesDb.Add(new ControllerProto(
                id: NewIds.Controllers.Controller,
                strings: Proto.CreateStr(NewIds.Controllers.Controller, "Controller", "Handles basic operations and automatization"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                allowedModules: (module) => module.AllowedDevices.Contains(NewIds.Controllers.Controller),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Controller,
                    customIconPath: NewAssets.Computers.Icons.Controller,
                    categories: category
                )
            ));

            registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.Antena,
                strings: Proto.CreateStr(NewIds.Controllers.Antena, "Antena", "Handles signal transfer for longer distance"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[3]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Antena,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: category
                )
            ));
        }
    }
}
