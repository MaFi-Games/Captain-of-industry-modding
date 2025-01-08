using Mafi;
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

    public class DataUpdater<T, C> : IDataUpdater
    {
        private T value;
        private Func<C, T> getter;
        private Action<C, T> setter;
        private Func<T, T, bool> comparator;
        private C context;

        public DataUpdater(Func<C, T> getter, Action<C, T> setter, Func<T,T,bool> comparator, C context)
        {
            this.getter = getter;
            this.setter = setter;
            this.comparator = comparator;
            this.context = context;
        }

        public bool WasChanged()
        {
            return true;
            //try
            //{
            //    T newValue = getter(context);
            //    if (comparator(value, newValue))
            //    {
            //        return false;
            //    }
            //    value = newValue;
            //    return true;
            //}
            //catch (NullReferenceException e)
            //{
            //    Log.Info("Change failed: " + e);
            //    return false;
            //}
        }

        public void Update()
        {
            //try
            //{
                //setter(context, value);
                setter(context, getter(context));
            //}
            //catch (NullReferenceException e)
            //{
            //    Log.Info("Update failed: " + e);
            //}
        }
    }
}