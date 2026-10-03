using System.Collections.Generic;
using UnityEngine;

namespace ScramblyFoxDefense.Core
{
    /// <summary>Prewarmed pool of prefab instances: no Instantiate during waves.</summary>
    public sealed class ObjectPool
    {
        readonly GameObject _prefab;
        readonly Transform _parent;
        readonly Stack<GameObject> _free = new Stack<GameObject>();

        public ObjectPool(GameObject prefab, Transform parent, int prewarm)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++) _free.Push(Create());
        }

        public GameObject Get()
        {
            var instance = _free.Count > 0 ? _free.Pop() : Create();
            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            instance.SetActive(false);
            _free.Push(instance);
        }

        GameObject Create()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.SetActive(false);
            return instance;
        }
    }
}
