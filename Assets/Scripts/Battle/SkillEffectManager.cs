using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillEffetType
{
    FireBall,
    FireBallExplotion,
    Hit,
    IceExplotion,
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
        _fireBall.Init(transform);
        _fireBallExplotion.Init(transform);
        _hit.Init(transform);
        _iceExplotion.Init(transform);
        _magicAura.Init(transform);
    }

    public SkillEffectView GetSkillEffectByType(SkillEffetType type)
    {
        switch(type)
        {
            case SkillEffetType.FireBall:
                return _fireBall;
            case SkillEffetType.FireBallExplotion:
                return _fireBallExplotion;
            case SkillEffetType.Hit:
                return _hit;
            case SkillEffetType.IceExplotion:
                return _iceExplotion;
            case SkillEffetType.MagicAura:
                return _magicAura;
        }
        return _fireBallExplotion;
    }
}
