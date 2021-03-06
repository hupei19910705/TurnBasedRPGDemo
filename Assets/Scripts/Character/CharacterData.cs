﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string UID { get; protected set; }
    public string ID { get; protected set; }
    public string Name { get; protected set; }
    public int Level { get; protected set; }
    public bool IsAlive { get { return CurrentHp > 0; } }
    public List<string> SkillList { get; protected set; }

    //rebuild
    private int _initHp;
    public int OriginHp { get; protected set; }
    public int MaxHp { get; protected set; }
    public int CurrentHp { get; protected set; }
    public float HPGrowthRate { get; protected set; }

    private int _initMp;
    public int OriginMp { get; protected set; }
    public int MaxMp { get; protected set; }
    public int CurrentMp { get; protected set; }
    public float MPGrowthRate { get; protected set; }

    private int _initPAtk;
    public int OriginPAtk { get; protected set; }
    public int PAttack { get; protected set; }
    public float PAtkGrowthRate { get; protected set; }

    private int _initMAtk;
    public int OriginMAtk { get; protected set; }
    public int MAttack { get; protected set; }
    public float MAtkGrowthRate { get; protected set; }

    private int _initPDef;
    public int OriginPDef { get; protected set; }
    public int PDefence { get; protected set; }
    public float PDefGrowthRate { get; protected set; }

    private int _initMDef;
    public int OriginMDef { get; protected set; }
    public int MDefence { get; protected set; }
    public float MDefGrowthRate { get; protected set; }

    private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();

    public CharacterData(string uid,string id,string name, int level, List<string> skills, int hp, float hpGrowth, int mp, float mpGrowth, 
        int pAtk, float pAtkGrowth, int mAtk, float mAtkGrowth,int pDef, float pDefGrowth, int mDef, float mDefGrowth)
    {
        UID = uid;
        ID = id;
        Name = name;
        Level = level;
        SkillList = skills;

        HPGrowthRate = hpGrowth;
        MPGrowthRate = mpGrowth;
        PAtkGrowthRate = pAtkGrowth;
        MAtkGrowthRate = mAtkGrowth;
        PDefGrowthRate = pDefGrowth;
        MDefGrowthRate = mDefGrowth;

        _initHp = hp;
        _initMp = mp;
        _initPAtk = pAtk;
        _initMAtk = mAtk;
        _initPDef = pDef;
        _initMDef = mDef;

        _CaculateValueByLevel();
    }

    protected virtual void _CaculateValueByLevel()
    {
        OriginHp = MaxHp = CurrentHp = Mathf.FloorToInt(_initHp * Mathf.Pow((1 + HPGrowthRate), Level - 1));
        OriginMp = MaxMp = CurrentMp = Mathf.FloorToInt(_initMp * Mathf.Pow((1 + MPGrowthRate), Level - 1));
        OriginPAtk = PAttack = Mathf.FloorToInt(_initPAtk * Mathf.Pow((1 + PAtkGrowthRate), Level - 1));
        OriginMAtk = MAttack = Mathf.FloorToInt(_initMAtk * Mathf.Pow((1 + MAtkGrowthRate), Level - 1));
        OriginPDef = PDefence = Mathf.FloorToInt(_initPDef * Mathf.Pow((1 + PDefGrowthRate), Level - 1));
        OriginMDef = MDefence = Mathf.FloorToInt(_initMDef * Mathf.Pow((1 + MDefGrowthRate), Level - 1));
    }

    public void BeHit(int attack, bool isReal = false)
    {
        var changeVlaue = attack - PDefence;
        if (isReal)
            changeVlaue = attack;

        changeVlaue = Math.Max(1, changeVlaue);

        ChangeHp(-changeVlaue);
    }

    public bool ChangeHp(int changeValue)
    {
        if (!IsAlive)
            return false;

        CurrentHp += changeValue;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        return true;
    }

    public bool ChangeMp(int changeValue)
    {
        if (!IsAlive)
            return false;

        CurrentMp += changeValue;
        CurrentMp = Mathf.Clamp(CurrentMp, 0, MaxMp);
        return true;
    }

    //rebuild
    public void AddBuffOrDebuffs(List<Buff> datas)
    {
        if (datas == null || datas.Count == 0)
            return;

        foreach (var data in datas)
        {
            var key = data.SingleID;
            if (_buffs.ContainsKey(key))
                _buffs.Remove(key);
            _buffs.Add(key, data);
        }
    }

    public void RemoveBuffOrDebuff(Buff data)
    {
        var key = data.SingleID;
        if (_buffs.ContainsKey(key))
            _buffs.Remove(key);
    }

    public void RemoveAllBuffOrDebuffs()
    {
        _buffs.Clear();
    }

    public List<ResultModel> BuffAndDebuffsEffect()
    {
        List<ResultModel> results = null;
        if (_buffs == null || _buffs.Count == 0)
            return results;

        List<EffectModel> models = new List<EffectModel>();

        foreach (var buff in _buffs.Values)
            models.AddRange(buff.BuffEffectThenReturnModels());

        results = ValueEffectByModels(models);
        _UpdateBuffStatus();
        return results;
    }

    public List<ResultModel> ValueEffectByModels(List<EffectModel> models)
    {
        List<ResultModel> results = null;
        if (models == null || models.Count == 0)
            return results;

        results = new List<ResultModel>();
        foreach (var model in models)
        {
            var result = ValueEffectByModel(model);
            if (result.ChangeValue == 0)
                continue;
            results.Add(result);
        }

        return results;
    }

    public ResultModel ValueEffectByModel(EffectModel model)
    {
        ResultModel result = default;
        result.EffectWay = model.EffectWay;
        float value = model.ChangeValue;
        switch (model.ValueType)
        {
            case CharacterValueType.HP:
                if (model.IsTargetPercent)
                    value *= OriginHp;
                result.IsReduce = value < 0;
                result.ChangeValue = _ChangeHp(Mathf.FloorToInt(value), model.EffectWay);
                result.ValueType = ResultValueType.Hp;
                break;
            case CharacterValueType.MaxHp:
                if (model.IsTargetPercent)
                    value *= OriginHp;
                result.IsReduce = value < 0;
                result.ChangeValue = _ChangeMaxHp(Mathf.FloorToInt(value), model.EffectWay);
                result.ValueType = ResultValueType.Hp;
                break;
            case CharacterValueType.MP:
                if (model.IsTargetPercent)
                    value *= OriginMp;
                result.IsReduce = value < 0;
                result.ChangeValue = _ChangeMp(Mathf.FloorToInt(value));
                result.ValueType = ResultValueType.Mp;
                break;
            case CharacterValueType.PAttack:
                if (model.IsTargetPercent)
                    value *= OriginPAtk;
                result.IsReduce = value < 0;
                _ChangePAtk(Mathf.FloorToInt(value));
                break;
            case CharacterValueType.MAttack:
                if (model.IsTargetPercent)
                    value *= OriginMAtk;
                result.IsReduce = value < 0;
                _ChangeMAtk(Mathf.FloorToInt(value));
                break;
            case CharacterValueType.PDefence:
                if (model.IsTargetPercent)
                    value *= OriginPDef;
                result.IsReduce = value < 0;
                _ChangePDef(Mathf.FloorToInt(value));
                break;
            case CharacterValueType.MDefence:
                if (model.IsTargetPercent)
                    value *= OriginMDef;
                result.IsReduce = value < 0;
                _ChangeMDef(Mathf.FloorToInt(value));
                break;
        }

        return result;
    }

    private void _UpdateBuffStatus()
    {
        List<string> endBuffs = new List<string>();
        foreach(var pair in _buffs)
        {
            if (!pair.Value.IsActive)
                endBuffs.Add(pair.Key);
        }

        if (endBuffs.Count == 0)
            return;

        foreach (var id in endBuffs)
            _buffs.Remove(id);
    }

    private int _DerateByEffectWay(int value, ValueEffectWay effectWay)
    {
        if (value > 0)
            return value;

        switch (effectWay)
        {
            case ValueEffectWay.Phisical:
                value += PDefence;
                if (value >= 0)
                    value = -1;
                break;
            case ValueEffectWay.Magic:
                value += MDefence;
                if (value >= 0)
                    value = -1;
                break;
        }

        return value;
    }

    private int _ChangeHp(int changeValue, ValueEffectWay effectWay = ValueEffectWay.None)
    {
        if (!IsAlive)
            return 0;

        changeValue = _DerateByEffectWay(changeValue, effectWay);
        int lastHp = CurrentHp;
        CurrentHp += changeValue;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        return CurrentHp - lastHp;
    }

    private int _ChangeMaxHp(int changeValue, ValueEffectWay effectWay = ValueEffectWay.None)
    {
        if (!IsAlive)
            return 0;

        changeValue = _DerateByEffectWay(changeValue, effectWay);

        MaxHp += changeValue;
        if (MaxHp < 1)
            MaxHp = 1;

        int lastHp = CurrentHp;
        CurrentHp += changeValue;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        return CurrentHp - lastHp;
    }

    private int _ChangeMp(int changeValue)
    {
        if (!IsAlive)
            return 0;

        int lastMp = CurrentMp;
        CurrentMp += changeValue;
        CurrentMp = Mathf.Clamp(CurrentMp, 0, MaxMp);
        return CurrentMp - lastMp;
    }

    private void _ChangePAtk(int changeValue)
    {
        PAttack += changeValue;
        if (PAttack <= 0)
            PAttack = 1;
    }

    private void _ChangeMAtk(int changeValue)
    {
        MAttack += changeValue;
        if (MAttack <= 0)
            MAttack = 1;
    }

    private void _ChangePDef(int changeValue)
    {
        PDefence += changeValue;
        if (PDefence <= 0)
            PDefence = 1;
    }

    private void _ChangeMDef(int changeValue)
    {
        MDefence += changeValue;
        if (MDefence <= 0)
            MDefence = 1;
    }
}

//rebuild
public struct EffectModel
{
    public CharacterValueType ValueType;
    public ValueEffectWay EffectWay;
    public float ChangeValue;
    public bool IsTargetPercent;

    public EffectModel(CharacterValueType valueType, ValueEffectWay effectWay,bool isTargetPercent,float changeValue)
    {
        ValueType = valueType;
        IsTargetPercent = isTargetPercent;
        EffectWay = effectWay;
        ChangeValue = changeValue;
    }
}

public enum ResultValueType
{
    None,
    Hp,
    Mp
}

public struct ResultModel
{
    public ResultValueType ValueType;
    public ValueEffectWay EffectWay;
    public bool IsReduce;
    public int ChangeValue;
}