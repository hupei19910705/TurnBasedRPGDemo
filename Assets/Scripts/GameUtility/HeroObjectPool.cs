using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.GameUtility
{
    public class HeroObjectPool : GeneralObjectPool
    {
        [SerializeField] private GameObject _warriorPrefab = null;
        [SerializeField] private GameObject _wizardPrefab = null;

        private Dictionary<HeroJobType, Queue<GameObject>> _memberObjects = new Dictionary<HeroJobType, Queue<GameObject>>();

        private GameObject _GetPrefabByMemberJob(HeroJobType job)
        {
            switch (job)
            {
                case HeroJobType.Warrior:
                    return _warriorPrefab;
                case HeroJobType.Wizard:
                    return _wizardPrefab;
            }
            return _warriorPrefab;
        }

        public override void InitPool(int count = DEFAULT_INIT_COUNT)
        {
            _memberObjects.Clear();
            _memberObjects.Add(HeroJobType.Warrior, new Queue<GameObject>());
            _memberObjects.Add(HeroJobType.Wizard, new Queue<GameObject>());

            for (int i = 0; i < count; i++)
            {
                var warriorObj = Instantiate(_warriorPrefab, _root);
                warriorObj.SetActive(false);
                _memberObjects[HeroJobType.Warrior].Enqueue(warriorObj);
                var wizardObj = Instantiate(_wizardPrefab, _root);
                wizardObj.SetActive(false);
                _memberObjects[HeroJobType.Wizard].Enqueue(wizardObj);
            }
        }

        public GameObject GetInstance(HeroJobType job)
        {
            GameObject obj = null;

            if (_memberObjects[job].Count == 0)
                obj = Instantiate(_GetPrefabByMemberJob(job), _root);
            else
                obj = _memberObjects[job].Dequeue();

            obj.SetActive(true);
            return obj;
        }

        public void ReturnInstance(HeroJobType job, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_root);
            _memberObjects[job].Enqueue(obj);
        }
    }
}