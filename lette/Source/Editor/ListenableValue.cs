using System.Collections.Generic;

namespace Lette.Editor
{
    public class ListenableValue<T>
    {
        T? value;
        public T? Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    this.value = value;
                    Notify();
                }
            }
        }

        public void Notify()
        {
            OnChange?.Invoke(this.value);
        }

        public delegate void onChangeDelegate(T? newValue);
        public event onChangeDelegate? OnChange;

        public static implicit operator T?(ListenableValue<T> self)
        {
            return self.Value;
        }
    }
}