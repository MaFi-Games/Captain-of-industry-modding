using System;

namespace ProgramableNetwork
{
    public class Reference
    {
        public int Value
        {
            get => getter();
            set => setter(value);
        }

        private readonly Action<int> setter;
        private readonly Func<int> getter;

        public Reference(Action<int> setter, Func<int> getter)
        {
            this.setter = setter;
            this.getter = getter;
        }
    }
}