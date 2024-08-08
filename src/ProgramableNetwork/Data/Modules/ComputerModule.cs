using Mafi.Core;

namespace ProgramableNetwork
{
    public class ComputerModule
    {
        public ModuleStatus Status { get; private set; }
        public string Error { get; private set; } = "";
        public ComputerModuleProto Prototype { get; }

        public void Execute()
        {
            try
            {
                Error = "";
                Prototype.Action(this);
                Status = ModuleStatus.Running;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
                Status = ModuleStatus.Error;
            }
        }

        public bool IsInputModule => Prototype.IsInputModule;
        public bool IsOutputModule => Prototype.IsOutputModule;

        public ModuleConnector[] InputModules { get; } // todo get by module id, cached
        public ModuleConnector[] OuputModules { get; } // todo get by module id, cached
        public EntityId Id { get; }
    }
}