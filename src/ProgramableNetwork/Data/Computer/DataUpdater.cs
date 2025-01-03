using Mafi.Localization;
using Mafi.Unity.UiFramework.Components;
using System;

namespace ProgramableNetwork
{
    public interface IDataUpdater
    {
        bool WasChanged();
        void Update();
    }

    public class DataUpdater<T> : IDataUpdater
    {
        private T value;
        private Func<T> getter;
        private Action<T> setter;
        private Func<T, T, bool> comparator;

        public DataUpdater(Func<T> getter, Action<T> setter, Func<T,T,bool> comparator)
        {
            this.getter = getter;
            this.setter = setter;
            this.comparator = comparator;
        }

        public bool WasChanged()
        {
            T newValue = getter();
            if (comparator(value, newValue)) {
                return false;
            }
            value = newValue;
            return true;
        }

        public void Update()
        {
            setter(value);
        }
    }
}