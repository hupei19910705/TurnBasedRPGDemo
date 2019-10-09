using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    GeneralAttack,
    Physical,
    Magic
}

public enum EffectType
{
    Multiple,
    Constant
}

public class Skill
{
    public SkillType Type { get; private set; }
    public EffectType EffectType { get; private set; }
    public SkillVariety Variety { get; private set; }
    public int EffectValue { get; private set; }
    public string ID { get; private set; }
    public string Name { get; private set; }
    public string ImageKey { get; private set; }
    public int SkillLv { get; private set; } = 1;
    public float Multiple { get; private set; } = 1f;
    public string Desc { get; private set; }
    public int MpCost { get; private set; }
    public bool IsRemote { get; private set; }
    public float MoveSpeed { get; private set; }

    private const string IMAGE_PATH_PREFIX = "Texture/Icons/";

    public Skill(SkillType type,string id,string name,int cost, SkillVariety variety,bool isRemote, float multiple = 1f,string imageKey = "", float speed = 0f)
    {
        Type = type;
        Variety = variety;
        ID = id;
        Name = name;
        MpCost = cost;
        Multiple = multiple;
        EffectType = EffectType.Multiple;
        EffectValue = 0;
        Desc = _GetDescription();
        IsRemote = isRemote;
        MoveSpeed = speed;
        if (!string.IsNullOrEmpty(imageKey))
            ImageKey = IMAGE_PATH_PREFIX + imageKey;
    }

    public Skill(SkillType type, string id, string name, int cost, int effectValue, SkillVariety variety, bool isRemote, string imageKey = "", float speed = 0f)
    {
        Type = type;
        Variety = variety;
        ID = id;
        Name = name;
        MpCost = cost;
        Multiple = 0f;
        EffectType = EffectType.Constant;
        EffectValue = effectValue;
        Desc = _GetDescription();
        IsRemote = isRemote;
        MoveSpeed = speed;
        if (!string.IsNullOrEmpty(imageKey))
            ImageKey = IMAGE_PATH_PREFIX + imageKey;
    }

    private string _GetDescription()
    {
        if (EffectType == EffectType.Constant)
            return string.Format("{0}\n造成{1}点固定伤害", Name, EffectValue);

        var effectTarget = string.Empty;
        switch (Type)
        {
            case SkillType.GeneralAttack:
            case SkillType.Physical:
                effectTarget = "物理伤害";
                break;
            case SkillType.Magic:
                effectTarget = "魔法伤害";
                break;
        }

        var effectValue = EffectValue.ToString();
        if (EffectValue > 0)
            effectValue = "+" + EffectValue.ToString();
        return string.Format("{0}\n造成 攻击x{1} 点{2}", Name, Math.Round(Multiple,2).ToString(), effectTarget);
    }
}
