using Mafi.Unity.UiFramework.Components;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public class ModuleLayout
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">when is not set, the width is calculated by number of inputs or outputs</param>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        public ModuleLayout(ModuleProto proto)
        {
            Inputs = proto.Inputs?.Count ?? 0;
            Outputs = proto.Outputs?.Count ?? 0;
            Displays = proto.Displays == null ? 0 : proto.Displays.Select(d => d.Width).Sum();
            Fields = proto.Fields?.Count ?? 0;
            Display = proto.DisplayFunction;
            DynamicWidth = proto.WidthFunction;
        }

        public int Inputs { get; }
        public int Outputs { get; }
        public int Displays { get; }
        public int Fields { get; }
        public Action<Module, StackContainer> Display { get; }
        public Func<Module, int> DynamicWidth { get; }

        /// <summary>
        /// Returns with in slots
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public int GetWidth(Module module)
        {
            if (DynamicWidth != null)
                return DynamicWidth.Invoke(module);
            else
                return Math.Max(Math.Max(Inputs, Outputs), Math.Max(Displays, Fields));
        }
    }
}