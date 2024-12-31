using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{

    public class ControllerProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate
    {
        public override Type EntityType { get; } = typeof(Controller);
        public int UsableTime { get; }
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public int Rows { get; }
        public int Columns { get; }
        public Func<ModuleProto, bool> AllowedModule { get; }

        public ControllerProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics,
            int rows = 4,
            int columns = 16,
            Upoints? boostCost = null,
            Electricity? workingPower = default,
            Electricity? iddlePower = default,
            Func<ModuleProto, bool> allowedModules = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(10), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(5);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2);
            this.Rows = rows;
            this.Columns = columns;
            this.AllowedModule = allowedModules ?? ((module) => true);
        }
    }
}
