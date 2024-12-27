using Mafi.Base;
using Mafi.Core.Entities;
using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    internal class Modules : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.ModuleBuilderStart("Constant", "Constant", "#", new ModuleProto.Gfx(Assets.Base.Products.Icons.Vegetables_svg))
                .AddOutput("value", "Value")
                .AddNumberField("number", "Number")
                .Action(m => { m.Output["number"] = m.Field["number", 0]; })
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Sum", "C = A + B", "A+B", new ModuleProto.Gfx(Assets.Base.Products.Icons.Vegetables_svg))
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] + m.Input["b", 0]; })
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Invert", "B = -A", "-A", new ModuleProto.Gfx(Assets.Base.Products.Icons.Vegetables_svg))
                .AddInput("a", "A")
                .AddOutput("b", "B")
                .Action(m => { m.Output["b"] = 0 - m.Input["a", 0]; })
                .BuildAndAdd();
        }
    }
}
