using Mafi.Collections;
using Mafi.Core.Syncers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork
{
    public class ChannelComparator : ICollectionComparator<IDataBandChannel, IEnumerable<IDataBandChannel>>
    {
        private Func<Func<IDataBandChannel, IDataBandChannel, bool>> comparatorGetter;

        public ChannelComparator(Func<Func<IDataBandChannel, IDataBandChannel, bool>> comparatorGetter)
        {
            this.comparatorGetter = comparatorGetter;
        }

        public bool AreSame(IEnumerable<IDataBandChannel> collection, Lyst<IDataBandChannel> lastKnown)
        {
            if (collection == null && lastKnown == null)
                return true;

            if (collection == null && lastKnown.Count > 0)
                return false;

            var listOriginal = collection?.ToList() ?? new List<IDataBandChannel>();
            if (lastKnown.Count != listOriginal.Count)
                return false;

            var comparator = comparatorGetter.Invoke();

            for (int i = 0; i < listOriginal.Count; i++)
            {
                if (listOriginal[i].Antena?.Id != lastKnown[i].Antena?.Id ||
                    comparator.Invoke(listOriginal[i], lastKnown[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}