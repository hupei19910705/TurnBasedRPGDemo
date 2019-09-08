using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.GameUtility
{
    public class TeamMemberObjectPool : GeneralObjectPool
    {
        [SerializeField] private GameObject _warriorPrefab = null;
        [SerializeField] private GameObject _wizardPrefab = null;

        private Dictionary<MemberJob, Queue<GameObject>> _memberObjects = new Dictionary<MemberJob, Queue<GameObject>>();

        private GameObject _GetPrefabByMemberJob(MemberJob job)
        {
            switch (job)
            {
                case MemberJob.Warrior:
                    return _warriorPrefab;
                case MemberJob.Wizard:
                    return _wizardPrefab;
            }
            return _warriorPrefab;
        }

        public override void InitPool(int count = DEFAULT_INIT_COUNT)
        {
            _memberObjects.Clear();
            _memberObjects.Add(MemberJob.Warrior, new Queue<GameObject>());
            _memberObjects.Add(MemberJob.Wizard, new Queue<GameObject>());

            for (int i = 0; i < count; i++)
            {
                var warriorObj = Instantiate(_warriorPrefab, _root);
                warriorObj.SetActive(false);
                _memberObjects[MemberJob.Warrior].Enqueue(warriorObj);
                var wizardObj = Instantiate(_wizardPrefab, _root);
                wizardObj.SetActive(false);
                _memberObjects[MemberJob.Wizard].Enqueue(wizardObj);
            }
        }

        public GameObject GetInstance(MemberJob job)
        {
            GameObject obj = null;

            if (_memberObjects[job].Count == 0)
                obj = Instantiate(_GetPrefabByMemberJob(job), _root);
            else
                obj = _memberObjects[job].Dequeue();

            obj.SetActive(true);
            return obj;
        }

        public void ReturnInstance(MemberJob job, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_root);
            _memberObjects[job].Enqueue(obj);
        }
    }
}