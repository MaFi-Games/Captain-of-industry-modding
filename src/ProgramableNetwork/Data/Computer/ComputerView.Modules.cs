using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Collections;
using Mafi.Core;

namespace ProgramableNetwork
{
    public partial class ComputerView : StaticEntityInspectorBase<Computer>
    {

        Dict<EntityId, ComputerModuleView> m_views = new Dict<EntityId, ComputerModuleView>();
        private StackContainer m_moduleLayout;

        private void AddModuleImplementation(StackContainer itemsContainer, UpdaterBuilder updaterBuilder)
        {
            m_moduleLayout = Builder
                .NewStackContainer("")
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .AppendTo(itemsContainer);


            updaterBuilder.Observe(() => Entity?.Modules, new ModuleIdComparator())
                .Do(modules =>
                {
                    if (modules != null && Entity.Prototype.ModuleLayout != null)
                        m_moduleLayout.SetSize(Entity.Prototype.ModuleLayout.X * 20, Entity.Prototype.ModuleLayout.Y * 20);
                    // TODO validate colisions
                    Set<EntityId> found = new Set<EntityId>();
                    foreach (var module in modules)
                    {
                        if (m_views.TryGetValue(module.Id, out var view))
                            found.Add(module.Id);
                        else
                        {
                            view = new ComputerModuleView(module, m_controller, this);
                            m_views.Add(module.Id, view);
                            found.Add(module.Id);
                        }
                    }
                    foreach (var view in m_views)
                    {
                        if (found.Add(view.Key))
                        {
                            view.Value.Remove();
                        }
                    }
                });
        }

        private class ModuleIdComparator : ICollectionComparator<ComputerModule, IEnumerable<ComputerModule>>
        {
            public bool AreSame(IEnumerable<ComputerModule> collection, Lyst<ComputerModule> lastKnown)
            {
                int i = 0;
                foreach (var item in collection)
                {
                    if (lastKnown.Count <= i || item.Id != lastKnown[i].Id)
                    {
                        return false;
                    }
                    i++;
                }
                return i == lastKnown.Count;
            }
        }

        private class ComputerModuleView
        {
            private readonly ComputerModule m_module;
            private readonly ComputerInspector m_controller;
            private readonly ComputerView m_computerView;
            private readonly IUiUpdater m_updater;

            public ComputerModuleView(ComputerModule module, ComputerInspector controller, ComputerView computerView)
            {
                this.m_module = module;
                this.m_controller = controller;
                this.m_computerView = computerView;
                var updater = UpdaterBuilder.Start();

                // TODO

                computerView.AddUpdater(this.m_updater = updater.Build());
            }

            public void Remove()
            {
                ((IUiUpdater)m_computerView.GetType().GetField("m_updater", System.Reflection.BindingFlags.NonPublic).GetValue(m_computerView)).RemoveChildUpdater(m_updater);
            }
        }
    }
}
