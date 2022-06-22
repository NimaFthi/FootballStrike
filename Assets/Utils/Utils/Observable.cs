using UnityEngine;

namespace _Scripts.Utils
{
    [System.Serializable]
    public class Observable<T>
    {
        [SerializeField] private T _value = default;
        public T value
        {
            set
            {
                _value = value;
                onChange();
            }
            get => _value;
        }

        public event System.Action onChange = delegate { };
        public void Invoke()
        {
            onChange();
        }
    }
}