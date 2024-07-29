using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{

    public class ComputerProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate
    {

        public override Type EntityType { get; } = typeof(Computer);
        public int UsableTime { get; }
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public int InstructionLevel { get; }
        public int Variables { get; }

        public ComputerProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics,
            int operationCount,
            int instructionLevel = 1,
            int variables = 5,
            Upoints? boostCost = null,
            Electricity? workingPower = default, Electricity? iddlePower = default,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(10), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags)
        {
            this.UsableTime = operationCount;
            this.WorkingPower = workingPower ?? Electricity.FromKw(5 * operationCount);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2 * operationCount / 10);
            this.InstructionLevel = instructionLevel;
            this.Variables = variables;
        }
    }
}
