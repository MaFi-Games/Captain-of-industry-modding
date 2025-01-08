using Mafi.Collections;
using Mafi.Core.Entities;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public interface IDataBand
    {
        EntityContext Context { set; get; }
        DataBandProto Prototype { get; }
        IEnumerable<IDataBandChannel> Channels { get; }

        void Update();
        void CreateChannel();
        void RemoveChannel(IDataBandChannel channel);
        void initContext();
    }
}