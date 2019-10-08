using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SkillVariety
{
    FireBall,
    Hit,
    Ice,
    MagicAura
}

public class SkillEffectManager : MonoBehaviour
{
    [SerializeField] private SkillEffectView _fireBall = null;
    [SerializeField] private SkillEffectView _fireBallExplotion = null;
    [SerializeField] private SkillEffectView _hit = null;
    [SerializeField] private SkillEffectView _iceExplotion = null;
    [SerializeField] private SkillEffectView _magicAura = null;

    public void Init()
    {
        _fireBall.Init(transform, true);
        _fireBallExplotion.Init(transform);
        _hit.Init(transform);
        _iceExplotion.Init(transform);
        _magicAura.Init(transform);
    }

    public IEnumerator PlaySkillEffect(Skill skill,Transform fromTrans,Transform targetTrans,UnityAction callBack)
    {
        bool callBackInvoked = false;
        var variety = skill == null ? SkillVariety.Hit : skill.Variety;
        var skillEffects = _GetSkillEffects(variety);

        if (skillEffects == null || skillEffects.Count == 0)
            yield break;

        for (int i = 0; i < skillEffects.Count; i++)
        {
            if (skillEffects[i].Ballistic)
            {
                skillEffects[i].LocateTo(fromTrans);
                yield return skillEffects[i].PlaySkillAni(targetTrans, skill.MoveSpeed);
                if (!callBackInvoked && callBack != null)
                {
                    callBack();
                    callBackInvoked = true;
                }
            }
            else
            {
                yield return skillEffects[i].PlaySkillAni(targetTrans);
                if (!callBackInvoked && callBack != null)
                {
                    callBack();
                    callBackInvoked = true;
                }
            }
        }
    }

    private List<SkillEffectView> _GetSkillEffects(SkillVariety variety)
    {
        List<SkillEffectView> list = new List<SkillEffectView>();
        switch(variety)
        {
            case SkillVariety.FireBall:
                list.Add(_fireBall);
                list.Add(_fireBallExplotion);
                break;
            case SkillVariety.Hit:
                list.Add(_hit);
                break;
            case SkillVariety.Ice:
                list.Add(_iceExplotion);
                break;
            case SkillVariety.MagicAura:
                list.Add(_magicAura);
                break;
        }
        return list;
    }
}
