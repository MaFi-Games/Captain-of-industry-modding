using Mafi;
using Mafi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public class EntitySelector : IEntitySelector<IEntity>
    {
        public Fix32 EntitySearchDistance { get; }
        public Action Refresh { get; }
        public IEntity Entity { set => m_set(value); }

        private readonly Func<Module, IEntity, bool> m_filter;
        private readonly Action<IEntity> m_set;
        private readonly Module m_module;

        public EntitySelector(Module module, Fix32 distance, Action refresh, Func<Module, IEntity, bool> filter, Action<IEntity> set)
        {
            Refresh = refresh;
            m_module = module;
            EntitySearchDistance = distance;
            m_filter = filter;
            m_set = set;
        }

        public bool EntityFilter(IEntity e)
        {
            return m_filter(m_module, e);
        }
    }
}
