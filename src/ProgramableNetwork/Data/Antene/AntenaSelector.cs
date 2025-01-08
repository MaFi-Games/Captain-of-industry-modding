using Mafi;
using Mafi.Core.Entities;
using System;

namespace ProgramableNetwork
{
    public class AntenaSelector : IEntitySelector<Antena>
    {
        public readonly Fix32 EntitySearchDistance;
        public readonly Action Refresh;
        private Func<IEntity, bool> filter;
        private Action<Antena> selected;

        public AntenaSelector(Fix32 distance, Action refresh, Func<IEntity, bool> filter, Action<Antena> selected)
        {
            this.EntitySearchDistance = distance;
            this.Refresh = refresh;
            this.filter = filter;
            this.selected = selected;
        }

        public Antena Entity { set => selected.Invoke(value); }

        public bool EntityFilter(Antena e)
        {
            return filter.Invoke(e);
        }
    }
}
