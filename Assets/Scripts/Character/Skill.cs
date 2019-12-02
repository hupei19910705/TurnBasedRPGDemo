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
    EffectOverTime
}

public enum EffectiveResult
{
    Reduce,
    Restore
}

public interface IUseData { }

public class Skill : IUseData
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
    public int Duration = 0;

    private const string IMAGE_PATH_PREFIX = "Texture/Icons/";

    public Skill(SkillRow skillRow)
    {
        EffectType = skillRow.EffectType;
        Variety = skillRow.Variety;
        ID = skillRow.ID;
        Name = skillRow.Name;
        MpCost = skillRow.MpCost;
        Multiple = skillRow.Multiple;
        IsConstant = skillRow.IsConstant;
        EffectValue = skillRow.EffectValue;
        IsRemote = skillRow.IsRemote;
        EffectiveWay = skillRow.EffectiveWay;
        EffectiveResult = skillRow.EffectiveResult;
        Duration = skillRow.Duration;
        MoveSpeed = skillRow.MoveSpeed;
        Desc = _GetDescription();
        if (!string.IsNullOrEmpty(skillRow.ImageKey))
            ImageKey = IMAGE_PATH_PREFIX + skillRow.ImageKey;
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

        var desc = string.Format("{0}\n造成 {1}{2} 点{3}", Name, type, effectValue, effectType);
        if (EffectiveWay == EffectiveWay.EffectOverTime)
            desc = string.Format("{0}\n每回合造成 {1}{2} 点{3}\n持续{4}回合", Name, type, effectValue, effectType, Duration);

        return desc;
    }
}