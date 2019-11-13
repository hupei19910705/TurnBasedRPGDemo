using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Physical,
    Magic,
    Real,
    Mp
}

public enum EffectiveWay
{
    Direct,
    Sustained
}

public enum EffectiveResult
{
    Reduce,
    Restore
}

public class Skill
{
    public EffectType EffectType;
    public EffectiveWay EffectiveWay;
    public EffectiveResult EffectiveResult;
    public bool IsConstant;
    public SkillVariety Variety;
    public int EffectValue;
    public string ID;
    public string Name;
    public string ImageKey;
    public int SkillLv = 1;
    public float Multiple = 1f;
    public string Desc;
    public int MpCost;
    public bool IsRemote;
    public float MoveSpeed;
    public float Duration = 0f;

    private const string IMAGE_PATH_PREFIX = "Texture/Icons/";

    public Skill(EffectType type, string id, string name, int cost, SkillVariety variety, bool isRemote, string imageKey,
        float multiple, EffectiveWay way, EffectiveResult result, float duration, float speed)
    {
        EffectType = type;
        Variety = variety;
        ID = id;
        Name = name;
        MpCost = cost;
        Multiple = multiple;
        IsConstant = false;
        EffectValue = 0;
        Desc = _GetDescription();
        IsRemote = isRemote;
        EffectiveWay = way;
        EffectiveResult = result;
        Duration = duration;
        MoveSpeed = speed;
        if (!string.IsNullOrEmpty(imageKey))
            ImageKey = IMAGE_PATH_PREFIX + imageKey;
    }

    public Skill(EffectType type, string id, string name, int cost, SkillVariety variety, bool isRemote, string imageKey,
        int effectValue, EffectiveWay way, EffectiveResult result, float duration, float speed)
    {
        EffectType = type;
        Variety = variety;
        ID = id;
        Name = name;
        MpCost = cost;
        Multiple = 0f;
        IsConstant = true;
        EffectValue = effectValue;
        Desc = _GetDescription();
        IsRemote = isRemote;
        EffectiveWay = way;
        EffectiveResult = result;
        Duration = duration;
        MoveSpeed = speed;
        if (!string.IsNullOrEmpty(imageKey))
            ImageKey = IMAGE_PATH_PREFIX + imageKey;
    }

    private string _GetDescription()
    {
        var type = string.Empty;

        var effectType = string.Empty;
        switch (EffectType)
        {
            case EffectType.Physical:
                effectType = "物理伤害";
                type = "攻击x";
                break;
            case EffectType.Magic:
                effectType = "魔法伤害";
                type = "魔力x";
                break;
            case EffectType.Real:
                effectType = "真实伤害";
                type = "攻击x";
                break;
        }

        if (IsConstant)
            type = "固定 ";

        var effectValue = string.Empty;
        if (IsConstant && EffectValue > 0)
            effectValue = EffectValue.ToString();
        else
            effectValue = Math.Round(Multiple, 2).ToString();

        return string.Format("{0}\n造成 {1}{2} 点{3}", Name, type, effectValue, effectType);
    }
}
