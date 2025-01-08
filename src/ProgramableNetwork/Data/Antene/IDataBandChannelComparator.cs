using Mafi.Collections;
using Mafi.Core.Syncers;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public interface IDataBandChannelComparator<T> : ICollectionComparator<T, IEnumerable<T>>
    {
        new bool AreSame(IEnumerable<T> collection, Lyst<T> lastKnown);
    }
}