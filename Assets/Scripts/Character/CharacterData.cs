using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string UID;
    public string ID;
    public string Name;
    public int OriginHp;
    public int MaxHp;
    public int CurrentHp;
    public int OriginMp;
    public int MaxMp = 0;
    public int CurrentMp = 0;
    public int Level;
    public int Attack;
    public int Defence;
    public float HPGrowthRate;
    public float MPGrowthRate;
    public float AtkGrowthRate;
    public float DefGrowthRate;
    public bool IsAlive { get { return CurrentHp > 0; } }
    public Dictionary<string, Skill> Skills;
    public List<string> SkillList;

    private int _initHp;
    private int _initMp;
    private int _initAtk;
    private int _initDef;

    public CharacterData(string uid,string id,string name, List<string> skills, int hp, int mp, int attack,
        int defence, int level, float hpGrowth, float mpGrowth, float atkGrowth, float defGrowth)
    {
        UID = uid;
        ID = id;
        Name = name;
        Level = level;
        SkillList = skills;

        HPGrowthRate = hpGrowth;
        MPGrowthRate = mpGrowth;
        AtkGrowthRate = atkGrowth;
        DefGrowthRate = defGrowth;

        _initHp = hp;
        _initMp = mp;
        _initAtk = attack;
        _initDef = defence;
        _CaculateValueByLevel();

        MaxHp = CurrentHp = OriginHp;
        MaxMp = CurrentMp = OriginMp;
    }

    protected virtual void _CaculateValueByLevel()
    {
        OriginHp = Mathf.FloorToInt(_initHp * Mathf.Pow((1 + HPGrowthRate), Level - 1));
        OriginMp = Mathf.FloorToInt(_initMp * Mathf.Pow((1 + MPGrowthRate), Level - 1));
        Attack = Mathf.FloorToInt(_initAtk * Mathf.Pow((1 + AtkGrowthRate), Level - 1));
        Defence = Mathf.FloorToInt(_initDef * Mathf.Pow((1 + DefGrowthRate), Level - 1));
    }

    public void BeHit(int attack, bool isReal = false)
    {
        var changeVlaue = attack - Defence;
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

    public void SetSkills(Dictionary<string, Skill> skills)
    {
        Skills = skills;
    }

    public void AddSkill(Skill skill)
    {
        if (Skills.ContainsKey(skill.ID) && Skills[skill.ID].SkillLv > skill.SkillLv)
            Skills[skill.ID] = skill;
        else
            Skills.Add(skill.ID, skill);
    }
}