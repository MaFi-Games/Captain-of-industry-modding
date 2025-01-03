using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    /// <summary>
    /// Deprecation class contains module Ids, which was not found during deserialization.
    /// They was maybe removed or replaced with different ones, or added by different mode.
    /// </summary>
    public class Deprecation
    {
        public static Dictionary<ModuleProto.ID, ModuleProto.ID> Deprecations { get; private set; }

        public static void RegisterDeprecation(ModuleProto.ID deprecated, ModuleProto.ID replacement)
        {
            Deprecations[deprecated] = replacement;
        }

        public static ModuleProto.ID GetAlternative(ModuleProto.ID original)
        {
            if (Deprecations.TryGetValue(original, out var alternative))
                return alternative;
            throw new Exception("Can not load Module ID: " + original.Value);
        }
    }
}
