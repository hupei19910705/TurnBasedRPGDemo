using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.GameUtility
{
    public class EnemyMemberObjectPool : GeneralObjectPool
    {
        [SerializeField] private GameObject _snakePrefab = null;
        [SerializeField] private GameObject _pigPrefab = null;
        [SerializeField] private GameObject _darkPigPrefab = null;
        [SerializeField] private GameObject _batPrefab = null;

        private Dictionary<EnemyType, Queue<GameObject>> _memberObjects = new Dictionary<EnemyType, Queue<GameObject>>();

        private GameObject _GetPrefabByEnemyType(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Snake:
                    return _snakePrefab;
                case EnemyType.Pig:
                    return _pigPrefab;
                case EnemyType.DarkPig:
                    return _darkPigPrefab;
                case EnemyType.Bat:
                    return _batPrefab;
            }
            return _snakePrefab;
        }

        public override void InitPool(int count = DEFAULT_INIT_COUNT)
        {
            _memberObjects.Clear();
            _memberObjects.Add(EnemyType.Snake, new Queue<GameObject>());
            _memberObjects.Add(EnemyType.Pig, new Queue<GameObject>());
            _memberObjects.Add(EnemyType.DarkPig, new Queue<GameObject>());
            _memberObjects.Add(EnemyType.Bat, new Queue<GameObject>());

            for (int i = 0; i < count; i++)
            {
                var snakeObj = Instantiate(_snakePrefab, _root);
                snakeObj.SetActive(false);
                _memberObjects[EnemyType.Snake].Enqueue(snakeObj);

                var pigObj = Instantiate(_pigPrefab, _root);
                pigObj.SetActive(false);
                _memberObjects[EnemyType.Pig].Enqueue(pigObj);

                var darkPigObj = Instantiate(_darkPigPrefab, _root);
                darkPigObj.SetActive(false);
                _memberObjects[EnemyType.DarkPig].Enqueue(darkPigObj);

                var batObj = Instantiate(_batPrefab, _root);
                batObj.SetActive(false);
                _memberObjects[EnemyType.Bat].Enqueue(batObj);
            }
        }

        public GameObject GetInstance(EnemyType type)
        {
            GameObject obj = null;

            if (_memberObjects[type].Count == 0)
                obj = Instantiate(_GetPrefabByEnemyType(type), _root);
            else
                obj = _memberObjects[type].Dequeue();

            obj.SetActive(true);
            return obj;
        }

        public void ReturnInstance(EnemyType type, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_root);
            _memberObjects[type].Enqueue(obj);
        }
    }
}
