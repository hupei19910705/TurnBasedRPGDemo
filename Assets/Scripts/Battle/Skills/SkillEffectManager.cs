using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SkillEffectViewType
{
    FireBall,
    FireBallExplosion,
    GeneralHit,
    IceExplosion,
    MagicAura
}

public class SkillEffectManager
{
    private Dictionary<string, Queue<SkillEffectView>> _skillEffectPool;
    private List<SkillEffectView> _overTimeSkillViews;

    private Dictionary<string, SkillEffectView> _skillEffectPrefabs;

    private string SKILL_EFFECT_PATH_PREFIX = "Prefabs/Characters/SkillEffect/";

    public SkillEffectManager(Transform defaultRoot, Dictionary<string, SkillEffectRow> table)
    {
        _skillEffectPrefabs = new Dictionary<string, SkillEffectView>();

        if (table != null && table.Count > 0)
        {
            foreach (var row in table.Values)
            {
                var view = _LoadPrefab(row.PrefabKey);
                view.Init(row.ID, row.Type, defaultRoot, _ReturnEffectView, row.IsOverTime, row.Ballistic);
                _skillEffectPrefabs.Add(row.ID, view);
            }
        }

        _skillEffectPool = new Dictionary<string, Queue<SkillEffectView>>();
        _overTimeSkillViews = new List<SkillEffectView>();
    }

    private SkillEffectView _LoadPrefab(string key)
    {
        var path = SKILL_EFFECT_PATH_PREFIX + key;
        var prefab = Resources.Load<GameObject>(path);
        return prefab.GetComponent<SkillEffectView>();
    }

    public IEnumerator PlayImmediatelyEffectViews(Skill skill, Transform fromTrans, Transform targetTrans)
    {
        List<string> skillEffectIds = skill == null ? null : skill.SkillEffects;
        var skillEffects = _GetSkillEffects(skillEffectIds);

        if (skillEffects == null || skillEffects.Count == 0)
            yield break;

        for (int i = 0; i < skillEffects.Count; i++)
        {
            var skillEffect = skillEffects[i];

            if (skillEffect.Ballistic)
            {
                skillEffect.LocateTo(fromTrans);
                yield return skillEffect.PlaySkillAni(targetTrans, skill.MoveSpeed);
            }
            else
                yield return skillEffect.PlaySkillAni(targetTrans);
        }
    }

    public IEnumerator AddBuffEffectViews(List<BuffRow> buffRows,Transform target)
    {
        if (buffRows == null || buffRows.Count == 0)
            yield break;

        foreach (var row in buffRows)
        {
            var effects = _GetSkillEffects(row.Effects);
            if (effects == null || effects.Count == 0)
                continue;

            foreach (var effect in effects)
            {
                effect.SetRoundCount(row.RoundCount);
                _overTimeSkillViews.Add(effect);
                yield return effect.PlaySkillAni(target);
            }
        }
    }

    private List<SkillEffectView> _GetSkillEffects(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
            return _GetDefaultSkillEffects();

        List<SkillEffectView> result = new List<SkillEffectView>();
        for (int i = 0; i < ids.Count; i++)
            result.Add(_GetEffectView(ids[i]));

        return result;
    }

    private List<SkillEffectView> _GetDefaultSkillEffects()
    {
        return new List<SkillEffectView> { _GetEffectView("10003") };
    }

    private SkillEffectView _GetEffectView(string id)
    {
        if (_skillEffectPool.ContainsKey(id))
        {
            var queue = _skillEffectPool[id];
            if (queue == null)
                queue = new Queue<SkillEffectView>();

            if (queue.Count > 0)
                return queue.Dequeue();
            else
            {
                var oriView = _skillEffectPrefabs[id];
                var obj = Object.Instantiate(oriView.gameObject);
                var newView = obj.GetComponent<SkillEffectView>();
                oriView.CopyTo(newView);
                return newView;
            }
        }
        else
        {
            var queue = new Queue<SkillEffectView>();
            _skillEffectPool.Add(id, queue);
            var oriView = _skillEffectPrefabs[id];
            var obj = Object.Instantiate(oriView.gameObject);
            var newView = obj.GetComponent<SkillEffectView>();
            oriView.CopyTo(newView);
            return newView;
        }
    }

    private void _ReturnEffectView(SkillEffectView view)
    {
        Queue<SkillEffectView> queue = null;
        if (_skillEffectPool.ContainsKey(view.ID))
            queue = _skillEffectPool[view.ID];
        else
            _skillEffectPool.Add(view.ID, queue);

        if (queue == null)
            queue = new Queue<SkillEffectView>();
        queue.Enqueue(view);
    }

    public void EndRound(int round = 1)
    {
        if (_overTimeSkillViews == null || _overTimeSkillViews.Count == 0)
            return;

        for (int i = _overTimeSkillViews.Count - 1; i >= 0; i--)
        {
            var view = _overTimeSkillViews[i];
            if (view.EndRound(round))
                _overTimeSkillViews.RemoveAt(i);
        }
    }
}
