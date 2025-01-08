using Mafi.Collections;
using Mafi.Core.Entities;

namespace ProgramableNetwork
{
    public interface IDataBandTyped<TDataBandChannel> : IDataBand
        where TDataBandChannel : IDataBandChannel
    {
    }
}