using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string UID;
    public string ID;
    public string Name;
    public double OriginHp;
    public double MaxHp;
    public double CurrentHp;
    public double OriginMp;
    public double MaxMp = 0;
    public double CurrentMp = 0;
    public int Level;
    public double Attack;
    public double Defence;
    public bool IsAlive { get { return CurrentHp > 0; } }
    public Dictionary<string, Skill> Skills;
    public List<string> SkillList;

    public CharacterData(string uid,string id,string name, List<string> skills, double hp,double mp, double attack, double defence, int level)
    {
        UID = uid;
        ID = id;
        Name = name;
        OriginHp = MaxHp = CurrentHp = hp;
        OriginMp = MaxMp = CurrentMp = mp;
        Level = level;
        Attack = attack;
        Defence = defence;
        SkillList = skills;
    }

    public void BeHit(double attack, bool isReal = false)
    {
        var changeVlaue = attack - Defence;
        if (isReal)
            changeVlaue = attack;

        changeVlaue = Math.Max(1, changeVlaue);

        ChangeHp(-changeVlaue);
    }

    public bool ChangeHp(double changeValue)
    {
        if (!IsAlive)
            return false;

        CurrentHp += changeValue;
        CurrentHp = Math.Floor(Mathf.Lerp(0f, (float)MaxHp, (float)(CurrentHp / MaxHp)));
        return true;
    }

    public bool ChangeMp(double changeValue)
    {
        if (!IsAlive)
            return false;

        CurrentMp += changeValue;
        CurrentMp = Math.Floor(Mathf.Lerp(0f, (float)MaxMp, (float)(CurrentMp / MaxMp)));
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