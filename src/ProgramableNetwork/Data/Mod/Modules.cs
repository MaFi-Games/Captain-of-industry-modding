using Mafi.Base;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
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
            registrator.ModuleBuilderStart("Constant", "Constant", "#", Assets.Base.Products.Icons.Vegetables_svg)
                .AddOutput("value", "Value")
                .AddInt32Field("number", "Number")
                .Action(m => { m.Output["number"] = m.Field["number", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Sum", "C = A + B", "A+B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] + m.Input["b", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Invert", "B = -A", "-A", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddOutput("b", "B")
                .Action(m => { m.Output["b"] = 0 - m.Input["a", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_Controller", "Connection: Controller (4 pin)", "C", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddInput("c", "C")
                .AddInput("d", "D")
                .AddOutput("a", "A")
                .AddOutput("b", "B")
                .AddOutput("c", "C")
                .AddOutput("d", "D")
                .AddStringField("Name", "Name", "C")
                .AddEntityField<Controller>("ConnectedController", "Connection device")
                .AddStringField("ConnectedModuleName", "Connection name", "")
                .Action(m => {
                    int entityId = m.NumberData.TryGetValue("ConnectedController_Id", out int entity) ? entity : 0;
                    string moduleId = m.StringData.TryGetValue("ConnectedModuleName", out string module) ? module : "";
                    if (moduleId.Length > 0 && m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Controller controller))
                    {
                        var targetModule = controller.Modules.FirstOrDefault(mod => m.StringData["Name"] == moduleId);
                        if (targetModule != null)
                        {
                            m.Input["a"] = targetModule.Output["a"];
                            m.Input["b"] = targetModule.Output["b"];
                            m.Input["c"] = targetModule.Output["c"];
                            m.Input["d"] = targetModule.Output["d"];
                            m.Output["a"] = targetModule.Input["a"];
                            m.Output["b"] = targetModule.Input["b"];
                            m.Output["c"] = targetModule.Input["c"];
                            m.Output["d"] = targetModule.Input["d"];

                            m.StatusIn["a"] = ModuleStatus.Running;
                            m.StatusIn["b"] = ModuleStatus.Running;
                            m.StatusIn["c"] = ModuleStatus.Running;
                            m.StatusIn["d"] = ModuleStatus.Running;
                            m.StatusOut["a"] = ModuleStatus.Running;
                            m.StatusOut["b"] = ModuleStatus.Running;
                            m.StatusOut["c"] = ModuleStatus.Running;
                            m.StatusOut["d"] = ModuleStatus.Running;
                            return;
                        }
                    }
                    m.StatusIn["a"] = ModuleStatus.Error;
                    m.StatusIn["b"] = ModuleStatus.Error;
                    m.StatusIn["c"] = ModuleStatus.Error;
                    m.StatusIn["d"] = ModuleStatus.Error;
                    m.StatusOut["a"] = ModuleStatus.Error;
                    m.StatusOut["b"] = ModuleStatus.Error;
                    m.StatusOut["c"] = ModuleStatus.Error;
                    m.StatusOut["d"] = ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_SwitchOff", "Connection: Switch Off", "S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "Pause")
                .AddEntityField<StaticEntity>("ConnectedEntity", "Connection device")
                .Action(m => {
                    m.NumberData.TryGetValue("ConnectedEntity_Id", out int entityId);
                    int input = m.Input["a", 0];
                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out StaticEntity entity)
                        && entity.CanBePaused)
                    {
                        entity.SetPaused(input > 0);
                        m.StatusIn["a"] = ModuleStatus.Running;
                        return;
                    }
                    m.StatusIn["a"] = ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }
    }
}
