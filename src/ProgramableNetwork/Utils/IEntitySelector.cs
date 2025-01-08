using Mafi.Core.Entities;

namespace ProgramableNetwork
{
    public interface IEntitySelector<TEntity>
        where TEntity : IEntity
    {
        bool EntityFilter(TEntity e);
    }
}