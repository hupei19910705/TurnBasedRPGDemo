using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SkillVariety
{
    FireBall,
    GeneralHit,
    IceExplosion,
    MagicAura
}

public enum SkillEffectViewType
{
    FireBall,
    FireBallExplosion,
    GeneralHit,
    IceExplosion,
    MagicAura
}

public class SkillEffectManager : MonoBehaviour
{
    [SerializeField] private SkillEffectView _fireBall = null;
    [SerializeField] private SkillEffectView _fireBallExplosion = null;
    [SerializeField] private SkillEffectView _hit = null;
    [SerializeField] private SkillEffectView _iceExplosion = null;
    [SerializeField] private SkillEffectView _magicAura = null;

    private Dictionary<SkillEffectViewType, Queue<SkillEffectView>> _skillEffectPool;
    private List<SkillEffectView> _overTimeSkillViews;

    public void Init()
    {
        _fireBall.Init(SkillEffectViewType.FireBall, transform, _ReturnEffectView, false, true);
        _fireBallExplosion.Init(SkillEffectViewType.FireBallExplosion, transform, _ReturnEffectView);
        _hit.Init(SkillEffectViewType.GeneralHit, transform, _ReturnEffectView);
        _iceExplosion.Init(SkillEffectViewType.IceExplosion, transform, _ReturnEffectView);
        _magicAura.Init(SkillEffectViewType.MagicAura, transform, _ReturnEffectView, true);
        _skillEffectPool = new Dictionary<SkillEffectViewType, Queue<SkillEffectView>>();
        _overTimeSkillViews = new List<SkillEffectView>();
    }

    public IEnumerator PlaySkillEffect(Skill skill,Transform fromTrans,Transform targetTrans,UnityAction callBack = null)
    {
        var variety = skill == null ? SkillVariety.GeneralHit : skill.Variety;
        var skillEffects = _GetSkillEffects(variety);

        if (skillEffects == null || skillEffects.Count == 0)
            yield break;

        bool callBackExecuted = false;

        for (int i = 0; i < skillEffects.Count; i++)
        {
            var skillEffect = skillEffects[i];

            if (skillEffect.Ballistic)
            {
                skillEffect.LocateTo(fromTrans);
                yield return skillEffect.PlaySkillAni(targetTrans, skill.MoveSpeed);
            }
            else
            {
                if (!callBackExecuted && callBack != null)
                {
                    callBack();
                    callBackExecuted = true;
                }

                int duration = 0;
                if (skill != null && skill.EffectiveWay == EffectiveWay.EffectOverTime)
                {
                    _overTimeSkillViews.Add(skillEffect);
                    duration = skill.Duration;
                }

                yield return skillEffect.PlaySkillAni(targetTrans, duration);
            }
        }
    }

    private List<SkillEffectView> _GetSkillEffects(SkillVariety variety)
    {
        List<SkillEffectView> list = new List<SkillEffectView>();
        switch(variety)
        {
            case SkillVariety.FireBall:
                list.Add(_GetEffectView(SkillEffectViewType.FireBall));
                list.Add(_GetEffectView(SkillEffectViewType.FireBallExplosion));
                break;
            case SkillVariety.GeneralHit:
                list.Add(_GetEffectView(SkillEffectViewType.GeneralHit));
                break;
            case SkillVariety.IceExplosion:
                list.Add(_GetEffectView(SkillEffectViewType.IceExplosion));
                break;
            case SkillVariety.MagicAura:
                list.Add(_GetEffectView(SkillEffectViewType.MagicAura));
                break;
            default:
                list.Add(null);
                break;
        }
        return list;
    }

    private SkillEffectView _GetEffectView(SkillEffectViewType type)
    {
        if(_skillEffectPool.ContainsKey(type))
        {
            var queue = _skillEffectPool[type];
            if (queue == null)
                queue = new Queue<SkillEffectView>();

            if (queue.Count > 0)
                return queue.Dequeue();
            else
            {
                var oriView = _GetOriSkillEffectView(type);
                var obj = Instantiate(oriView.gameObject);
                var newView = obj.GetComponent<SkillEffectView>();
                oriView.CopyTo(newView);
                return newView;
            }
        }
        else
        {
            var queue = new Queue<SkillEffectView>();
            _skillEffectPool.Add(type, queue);
            var oriView = _GetOriSkillEffectView(type);
            var obj = Instantiate(oriView.gameObject);
            var newView = obj.GetComponent<SkillEffectView>();
            oriView.CopyTo(newView);
            return newView;
        }
    }

    private void _ReturnEffectView(SkillEffectView view)
    {
        Queue<SkillEffectView> queue = null;
        if (_skillEffectPool.ContainsKey(view.Type))
            queue = _skillEffectPool[view.Type];
        else
            _skillEffectPool.Add(view.Type, queue);

        if (queue == null)
            queue = new Queue<SkillEffectView>();
        queue.Enqueue(view);
    }

    private SkillEffectView _GetOriSkillEffectView(SkillEffectViewType type)
    {
        switch (type)
        {
            case SkillEffectViewType.FireBall:
                return _fireBall;
            case SkillEffectViewType.FireBallExplosion:
                return _fireBallExplosion;
            case SkillEffectViewType.GeneralHit:
                return _hit;
            case SkillEffectViewType.IceExplosion:
                return _iceExplosion;
            case SkillEffectViewType.MagicAura:
                return _magicAura;
        }
        return null;
    }

    public void OverTime(int round = 1)
    {
        if (_overTimeSkillViews == null || _overTimeSkillViews.Count == 0)
            return;
        for(int i = _overTimeSkillViews.Count-1;i>=0;i--)
        {
            var view = _overTimeSkillViews[i];
            if (view.OverTime(round))
                _overTimeSkillViews.RemoveAt(i);
        }
    }
}
