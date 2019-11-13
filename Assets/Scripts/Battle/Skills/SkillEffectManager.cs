using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SkillVariety
{
    FireBall,
    GeneralHit,
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

    public IEnumerator PlaySkillEffect(Skill skill,Transform fromTrans,Transform targetTrans)
    {
        var variety = skill == null ? SkillVariety.GeneralHit : skill.Variety;
        var skillEffects = _GetSkillEffects(variety);

        if (skillEffects == null || skillEffects.Count == 0)
            yield break;

        for (int i = 0; i < skillEffects.Count; i++)
        {
            var skillEffect = skillEffects[i];

            if(skillEffect == null)
                yield return skillEffect.PlaySkillAni(targetTrans);
            else if(skillEffect.Ballistic)
            {
                skillEffect.LocateTo(fromTrans);
                yield return skillEffect.PlaySkillAni(targetTrans, skill.MoveSpeed);
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
            case SkillVariety.GeneralHit:
                list.Add(_hit);
                break;
            case SkillVariety.Ice:
                list.Add(_iceExplotion);
                break;
            case SkillVariety.MagicAura:
                list.Add(_magicAura);
                break;
            default:
                list.Add(null);
                break;
        }
        return list;
    }
}
