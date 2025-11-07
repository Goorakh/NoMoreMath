using System;
using System.Collections.Generic;

namespace NoMoreMath.Utils
{
    public sealed class TrackedValue<T>
    {
        public IEqualityComparer<T> EqualityComparer { get; set; }

        T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (EqualityComparer.Equals(_value, value))
                    return;

                T oldValue = _value;
                _value = value;

                OnValueChanged?.Invoke(oldValue, _value);
            }
        }

        public delegate void ValueChangedDelegate(T oldValue, T newValue);
        public event ValueChangedDelegate OnValueChanged;

        public TrackedValue(T value) : this(value, EqualityComparer<T>.Default)
        {
        }

        public TrackedValue(T value, IEqualityComparer<T> equalityComparer)
        {
            _value = value;
            EqualityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
        }

        public bool Push(T value)
        {
            if (EqualityComparer.Equals(_value, value))
                return false;

            Value = value;
            return true;
        }
    }
}
