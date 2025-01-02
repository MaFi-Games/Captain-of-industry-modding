using Mafi;
using Mafi.Base;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
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
        static readonly string[] names = new string[] { "a", "b", "c", "d" };

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.ModuleBuilderStart("Constant", "Constant", "#", Assets.Base.Products.Icons.Vegetables_svg)
                .AddOutput("value", "Value")
                .AddInt32Field("number", "Number")
                .Action(m => { m.Output["value"] = m.Field["number", 0]; })
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

            Comparation(registrator);
            Connections(registrator);
            Forks(registrator);
            Booleans(registrator);
            Display(registrator);
        }

        private void Forks(ProtoRegistrator registrator)
        {
            Action<Module>[] actions = new Action<Module>[] {
                m => {
                    m.Output["a"] = m.Input["a"];
                    m.Output["b"] = m.Input["a"];
                },
                m => {
                    m.Output["a"] = m.Input["a"];
                    m.Output["b"] = m.Input["a"];
                    m.Output["c"] = m.Input["a"];
                    m.Output["d"] = m.Input["a"];
                },
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Fork_{i}", $"Fork: 1 pin to {i}", $"F-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddInput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(actions[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddOutput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
        }

        private void Booleans(ProtoRegistrator registrator)
        {
            Action<Module>[] ands = new Action<Module>[] {
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 &&
                        m.Input["b", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                },
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 &&
                        m.Input["b", 0] > 0 &&
                        m.Input["c", 0] > 0 &&
                        m.Input["d", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                }
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Boolean_And_{i}", $"Boolean: AND ({i} pins)", $"AND-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddOutput("b", "not A")
                    .AddOutput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(ands[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddInput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
            Action<Module>[] ors = new Action<Module>[] {
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a"] > 0 ||
                        m.Input["b"] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                },
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 ||
                        m.Input["b", 0] > 0 ||
                        m.Input["c", 0] > 0 ||
                        m.Input["d", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                }
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Boolean_Or_{i}", $"Boolean: OR ({i} pins)", $"OR-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddOutput("b", "not A")
                    .AddOutput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(ors[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddInput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
            registrator
                .ModuleBuilderStart($"Boolean_Xor", $"Boolean: XOR", $"XOR", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("b", "not A")
                .AddOutput("a", "A")
                .AddControllerDevice()
                // dynamic
                .Action(m =>
                    {
                        m.Output["a"] = (
                            m.Input["a"] > 0 !=
                            m.Input["b"] > 0
                        ) ? 1 : 0;
                        m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                    })
                .BuildAndAdd();
        }

        private static void Connections(ProtoRegistrator registrator)
        {
            registrator.ModuleBuilderStart("Connection_Controller_Input", "Connection: Controller (4 pin, input)", "C-IN", Assets.Base.Products.Icons.Vegetables_svg)
                .AddOutput("a", "A")
                .AddOutput("b", "B")
                .AddOutput("c", "C")
                .AddOutput("d", "D")
                .AddEntityField<Controller>("controller", "Connection device", 20.ToFix32())
                .AddStringField("name", "Output Name", "C")
                .Action(m =>
                {
                    //Mafi.Log.Info("Update of input");
                    int entityId = m.Field["controller", 0];
                    string moduleType = "Connection_Controller_Output".ModuleId();
                    string noduleName = m.Field["name", ""];
                    if (noduleName.Length > 0 && m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Controller controller))
                    {
                        //Mafi.Log.Info("Target entity found");
                        Module targetModule = controller.Modules.AsEnumerable()
                            .FirstOrDefault(mod => mod.Prototype.Id.Value == moduleType
                                                && mod.Field["name", ""] == noduleName);
                        if (targetModule != null)
                        {
                            m.Output["a"] = targetModule.Input["a", 0];
                            m.Output["b"] = targetModule.Input["b", 0];
                            m.Output["c"] = targetModule.Input["c", 0];
                            m.Output["d"] = targetModule.Input["d", 0];

                            m.StatusOut["a"] = ModuleStatus.Running;
                            m.StatusOut["b"] = ModuleStatus.Running;
                            m.StatusOut["c"] = ModuleStatus.Running;
                            m.StatusOut["d"] = ModuleStatus.Running;
                            return;
                        }
                    }
                    m.Output["a"] = 0;
                    m.Output["b"] = 0;
                    m.Output["c"] = 0;
                    m.Output["d"] = 0;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_Controller_Output", "Connection: Controller (4 pin, output)", "C-OUT", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddInput("c", "C")
                .AddInput("d", "D")
                .AddStringField("name", "Name", "C")
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_SwitchOff", "Connection: Switch Off", "S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("pause", "Pause")
                .AddEntityField<StaticEntity>("entity", "Connection device", 20.ToFix32())
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int input = m.Input["pause", 0];
                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out StaticEntity entity)
                        && entity.CanBePaused)
                    {
                        entity.SetPaused(input > 0);
                        m.StatusIn["pause"] = ModuleStatus.Running;
                        return;
                    }
                    m.StatusIn["pause"] = ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_Storage", "Connection: Storage (input)", "STOCK", Assets.Base.Products.Icons.Vegetables_svg)
                .AddOutput("quantity", "Quantity")
                .AddOutput("capacity", "Capacity")
                .AddOutput("fullness", "Fullness in %")
                .AddOutput("product", "Product in #")
                .AddEntityField<StorageBase>("entity", "Connection device", 20.ToFix32())
                // .AddInt32Field("buffer", "Storage slot (0-based)", 0)
                // TODO add filter input field
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int buffer = m.Input["buffer", 0];

                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out StorageBase entity))
                    {
                        m.Output["quantity"] = entity.CurrentQuantity.Value;
                        m.Output["capacity"] = entity.Capacity.Value;
                        m.Output["fullness"] = (int)(100f * entity.CurrentQuantity.Value / entity.Capacity.Value);

                        if (entity.StoredProduct.HasValue)
                        {
                            m.Output["product"] = (int)(uint)entity.StoredProduct.Value.SlimId.Value;
                        }
                        else
                        {
                            m.Output["product"] = -1;
                        }
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["product"] = -1;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Connection_Transport", "Connection: Transport (input)", "TRANS", Assets.Base.Products.Icons.Vegetables_svg)
                .AddOutput("quantity", "Quantity")
                .AddOutput("capacity", "Capacity")
                .AddOutput("fullness", "Fullness in %")
                .AddInput("product", "First Product in #")
                .AddEntityField<Transport>("entity", "Connection device", 20.ToFix32())
                // TODO add filter input field
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int buffer = m.Input["buffer", 0];
                    int filterId = m.Input["product", 0];

                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Transport entity))
                    {
                        m.Output["quantity"] = entity.TransportedProducts
                                            .Where(p => filterId == 0 || p.SlimId.Value == filterId)
                                            .Select(p => p.Quantity.Value).Sum();
                        m.Output["capacity"] = entity.Trajectory.MaxProducts;
                        m.Output["fullness"] = (int)(100f * m.Output["quantity"] / m.Output["capacity"]);

                        if (entity.FirstProduct.HasValue)
                        {
                            m.Output["product"] = (int)(uint)entity.FirstProduct.Value.SlimId.Value;
                        }
                        else
                        {
                            m.Output["product"] = -1;
                        }
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["product"] = -1;
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Comparation(ProtoRegistrator registrator)
        {
            registrator.ModuleBuilderStart("Compare_Int_Greater", "Compare: A > B", "A>B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] > m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Compare_Int_Lower", "Compare: A < B", "A<B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] > m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Compare_Int_GreaterOrEqual", "Compare: A ≥ B", "A≥B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] >= m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator.ModuleBuilderStart("Compare_Int_LowerOrEqual", "Compare: A ≤ B", "A≤B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] <= m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Display(ProtoRegistrator registrator)
        {
            // TODO add display
            // from 2 - 16 digits
            // display float values
        }
    }
}
