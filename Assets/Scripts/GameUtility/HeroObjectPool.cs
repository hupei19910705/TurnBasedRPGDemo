using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.GameUtility
{
    public class HeroObjectPool : GeneralObjectPool
    {
        [SerializeField] private GameObject _warriorPrefab = null;
        [SerializeField] private GameObject _wizardPrefab = null;

        private Dictionary<HeroJob, Queue<GameObject>> _memberObjects = new Dictionary<HeroJob, Queue<GameObject>>();

        private GameObject _GetPrefabByMemberJob(HeroJob job)
        {
            switch (job)
            {
                case HeroJob.Warrior:
                    return _warriorPrefab;
                case HeroJob.Wizard:
                    return _wizardPrefab;
            }
            return _warriorPrefab;
        }

        public override void InitPool(int count = DEFAULT_INIT_COUNT)
        {
            _memberObjects.Clear();
            _memberObjects.Add(HeroJob.Warrior, new Queue<GameObject>());
            _memberObjects.Add(HeroJob.Wizard, new Queue<GameObject>());

            for (int i = 0; i < count; i++)
            {
                var warriorObj = Instantiate(_warriorPrefab, _root);
                warriorObj.SetActive(false);
                _memberObjects[HeroJob.Warrior].Enqueue(warriorObj);
                var wizardObj = Instantiate(_wizardPrefab, _root);
                wizardObj.SetActive(false);
                _memberObjects[HeroJob.Wizard].Enqueue(wizardObj);
            }
        }

        public GameObject GetInstance(HeroJob job)
        {
            GameObject obj = null;

            if (_memberObjects[job].Count == 0)
                obj = Instantiate(_GetPrefabByMemberJob(job), _root);
            else
                obj = _memberObjects[job].Dequeue();

            obj.SetActive(true);
            return obj;
        }

        public void ReturnInstance(HeroJob job, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_root);
            _memberObjects[job].Enqueue(obj);
        }
    }
}