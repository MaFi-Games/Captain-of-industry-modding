using Mafi.Localization;
using System;
using System.Runtime.Serialization;

namespace ProgramableNetwork
{
    [Serializable]
    internal class ProgramException : Exception
    {
        public new readonly LocStrFormatted Message;

        public ProgramException(LocStrFormatted locStrFormatted)
            : base(locStrFormatted.Value)
        {
            this.Message = locStrFormatted;
        }
    }
}