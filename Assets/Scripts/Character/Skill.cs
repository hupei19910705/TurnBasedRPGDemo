using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OldEffectType
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

public enum SkillTarget
{
    SelfSide,
    OppositeSide
}

public enum EffectTiming
{
    Immediately,
    EndRound
}

public enum SkillType
{
    Damage,
    Heal,
    Buff,
    Debuff
}

public interface IUseData
{
    bool CanUseToSelf { get; }
    bool CanUseToOpposite { get; }
}

public class Skill : IUseData
{
    //rebuild
    public string ID;
    public string Name;
    public int MpCost;
    public bool IsRemote;
    public string ImageKey;
    public float MoveSpeed;
    public string Desc;
    public List<string> SkillEffects = new List<string>();

    public bool CanUseToSelf { get { return _useToSelfSideDatas.Count > 0 || _useToSelfSideBuffs.Count > 0; } }
    public bool CanUseToOpposite { get { return _useToOppositeSideDatas.Count > 0 || _useToOppositeSideBuffs.Count > 0; } }

    private List<EffectData> _useToSelfSideDatas = new List<EffectData>();
    private List<string> _useToSelfSideBuffs;

    private List<EffectData> _useToOppositeSideDatas = new List<EffectData>();
    private List<string> _useToOppositeSideBuffs;

    private const string IMAGE_PATH_PREFIX = "Texture/Icons/";

    public Skill(SkillRow skillRow)
    {
        ID = skillRow.ID;
        Name = skillRow.Name;
        SkillEffects = skillRow.Effects;
        MpCost = skillRow.MpCost;
        IsRemote = skillRow.IsRemote;
        if (!string.IsNullOrEmpty(skillRow.ImageKey))
            ImageKey = IMAGE_PATH_PREFIX + skillRow.ImageKey;
        MoveSpeed = skillRow.MoveSpeed;
        Desc = skillRow.Desc;
        _SetEffectDatas(skillRow);

        _useToSelfSideBuffs = skillRow.UseToSelfSideBuffIds;
        if (_useToSelfSideBuffs == null)
            _useToSelfSideBuffs = new List<string>();
        _useToOppositeSideBuffs = skillRow.UseToOppositeSideBuffIds;
        if (_useToOppositeSideBuffs == null)
            _useToOppositeSideBuffs = new List<string>();
    }

    private void _SetEffectDatas(SkillRow skillRow)
    {
        _useToSelfSideDatas.Clear();
        _useToOppositeSideDatas.Clear();

        var selfId0 = skillRow.UseToSelfSideDataId0;
        var selfId1 = skillRow.UseToSelfSideDataId1;
        var oppositeId0 = skillRow.UseToOppositeSideDataId0;
        var oppositeId1 = skillRow.UseToOppositeSideDataId1;
        List<string> tempList = new List<string>
        {
            selfId0,
            selfId1,
            oppositeId0,
            oppositeId1
        };
        var ids = tempList.FindAll(id => !string.IsNullOrEmpty(id));
        var rows = CharacterUtility.Instance.GetEffectDataRows(ids);
        if (rows == null || rows.Count == 0)
            return;

        if (rows.ContainsKey(selfId0))
            _useToSelfSideDatas.Add(new EffectData(rows[selfId0], skillRow.UseToSelfSideDataValue0));
        if (rows.ContainsKey(selfId1))
            _useToSelfSideDatas.Add(new EffectData(rows[selfId1], skillRow.UseToSelfSideDataValue1));
        
        if (rows.ContainsKey(oppositeId0))
            _useToOppositeSideDatas.Add(new EffectData(rows[oppositeId0], skillRow.UseToOppositeSideDataValue0));
        if (rows.ContainsKey(oppositeId1))
            _useToOppositeSideDatas.Add(new EffectData(rows[oppositeId1], skillRow.UseToOppositeSideDataValue1));
    }

    public List<EffectModel> GetImmediatelyEffectModels(CharacterData from, SkillTarget target)
    {
        List<EffectData> datas = null;
        switch (target)
        {
            case SkillTarget.SelfSide:
                datas = _useToSelfSideDatas;
                break;
            case SkillTarget.OppositeSide:
                datas = _useToOppositeSideDatas;
                break;
        }

        if (datas == null || datas.Count == 0)
            return null;

        List<EffectModel> result = new List<EffectModel>();
        foreach (var data in datas)
            result.Add(data.CreateEffectModel(from));

        return result;
    }

    public List<Buff> GetBuffs(CharacterData from, SkillTarget target)
    {
        List<BuffRow> buffRows = GetBuffRows(target);

        if (buffRows == null || buffRows.Count == 0)
            return null;

        List<Buff> result = new List<Buff>();
        foreach (var row in buffRows)
        {
            var buff = new Buff(row, from);
            result.Add(buff);
        }

        return result;
    }

    public List<BuffRow> GetBuffRows(SkillTarget target)
    {
        List<BuffRow> buffRows = null;

        switch (target)
        {
            case SkillTarget.SelfSide:
                buffRows = CharacterUtility.Instance.GetBuffRows(_useToSelfSideBuffs);
                break;
            case SkillTarget.OppositeSide:
                buffRows = CharacterUtility.Instance.GetBuffRows(_useToOppositeSideBuffs);
                break;
        }

        return buffRows;
    }

    public static EffectModel GeneralAtkModel(int attack)
    {
        return new EffectModel(CharacterValueType.HP, ValueEffectWay.Phisical, false, -attack);
    }
}

//rebuild
public enum CharacterValueType
{
    None,
    HP,
    MaxHp,
    MP,
    PAttack,
    MAttack,
    PDefence,
    MDefence
}

public enum ValueEffectWay
{
    None,
    Phisical,
    Magic,
    Real
}

public enum SkillEffectType
{
    None,
    Multiple,
    Constant,
    TargetPercent
}

public struct EffectData
{
    public CharacterValueType ValueType { get; private set; }
    public ValueEffectWay EffectWay { get; private set; }
    public SkillEffectType EffectType { get; private set; }
    public float EffectValue { get; private set; }

    public EffectData(EffectDataRow row,float effectValue)
    {
        ValueType = row.ValueType;
        EffectWay = row.EffectWay;
        EffectType = row.EffectType;
        EffectValue = effectValue;
    }

    public EffectModel CreateEffectModel(CharacterData impact)
    {
        float changeValue = EffectValue;

        if (EffectType == SkillEffectType.Multiple)
        {
            switch (EffectWay)
            {
                case ValueEffectWay.Phisical:
                    changeValue = EffectValue * impact.PAttack;
                    break;
                case ValueEffectWay.Magic:
                    changeValue = EffectValue * impact.MAttack;
                    break;
            }
        }

        return new EffectModel(ValueType, EffectWay, EffectType == SkillEffectType.TargetPercent, changeValue);
    }
}