using System.Collections.Generic;
using UnityEngine;

namespace NoMoreMath.Utils
{
    public sealed class ObjectComponentCache<THostComponent, TCachedComponent> where THostComponent : Component where TCachedComponent : MonoBehaviour, ICachableComponent
    {
        readonly Dictionary<int, TCachedComponent> _componentDictionary = [];

        int getDictionaryKey(THostComponent host)
        {
            return host.GetInstanceID();
        }

        public bool TryGetComponent(THostComponent host, out TCachedComponent component)
        {
            if (!host)
            {
                component = default;
                return false;
            }

            return _componentDictionary.TryGetValue(getDictionaryKey(host), out component);
        }

        public TCachedComponent GetOrAddComponent(THostComponent host)
        {
            if (!host)
                return default;

            int dictionaryKey = getDictionaryKey(host);
            if (_componentDictionary.TryGetValue(dictionaryKey, out TCachedComponent component))
                return component;

            component = host.GetComponent<TCachedComponent>();
            if (!component)
                component = host.gameObject.AddComponent<TCachedComponent>();

            component.DictionaryKey = dictionaryKey;

            _componentDictionary.Add(dictionaryKey, component);

            return component;
        }

        public void OnDestroyed(TCachedComponent component)
        {
            if (component.DictionaryKey.HasValue)
            {
                _componentDictionary.Remove(component.DictionaryKey.Value);
            }
        }
    }
}
