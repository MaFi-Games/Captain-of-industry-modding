using Mafi;
using Mafi.Core.Mods;
using System;

namespace ProgramableNetwork
{
    public abstract class AValidatedData : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            try
            {
                RegisterDataInternal(registrator);
            }
            catch (System.Exception e)
            {
                Log.Exception(e);
                throw;
            }
        }

        protected abstract void RegisterDataInternal(ProtoRegistrator registrator);
    }
}