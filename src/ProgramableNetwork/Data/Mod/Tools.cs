using Mafi.Core.Buildings.Storages;
using Mafi.Core.Prototypes;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using System;
using System.Linq;
using Mafi.Unity.UserInterface.Style;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Tools
        {
            public static readonly Proto.ID SelectStaticEntity = new Proto.ID("ProgramableNetwork_Tools_SelectStaticEntity");
            public static readonly Proto.ID SelectDynamicEntity = new Proto.ID("ProgramableNetwork_Tools_SelectDynamicEntity");
        }
    }

    internal class Tools : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(new TechnologyProto(
                NewIds.Tools.SelectStaticEntity,
                Proto.CreateStr(NewIds.Tools.SelectStaticEntity, "Select static entity"),
                new TechnologyProto.Gfx(new UiStyle().Icons.Empty)
            ));

            registrator.PrototypesDb.Add(new TechnologyProto(
                NewIds.Tools.SelectDynamicEntity,
                Proto.CreateStr(NewIds.Tools.SelectDynamicEntity, "Select dynamic entity"),
                new TechnologyProto.Gfx(new UiStyle().Icons.Empty)
            ));
        }
    }
}
