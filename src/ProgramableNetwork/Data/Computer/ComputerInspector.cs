using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class ComputerInspector : EntityInspector<Computer, ComputerView>
    {
        private readonly ComputerView m_windowView;

        public ComputerInspector(InspectorContext context) : base(context)
        {
            m_windowView = new ComputerView(this);
        }

        protected override ComputerView GetView()
        {
            return m_windowView;
        }
    }
}
