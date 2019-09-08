using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility.GameUtility
{
    public class GeneralObjectPool : MonoBehaviour
    {
        [SerializeField] protected Transform _root = null;
        [SerializeField] private GameObject _prefab = null;

        protected const int DEFAULT_INIT_COUNT = 2;

        private Queue<GameObject> _objects = new Queue<GameObject>();

        public virtual void InitPool(int count = DEFAULT_INIT_COUNT)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(_prefab, _root);
                obj.transform.SetParent(_root);
                obj.SetActive(false);
                _objects.Enqueue(obj);
            }
        }

        public virtual GameObject GetInstance()
        {
            GameObject obj = null;

            if (_objects.Count == 0)
                obj = Instantiate(_prefab, _root);
            else
                obj = _objects.Dequeue();

            obj.SetActive(true);
            return obj;
        }

        public virtual void ReturnInstance(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_root);
            _objects.Enqueue(obj);
        }
    }
}