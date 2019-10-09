using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string Name { get; protected set; }
    public double OriginHp { get; protected set; }
    public double MaxHp { get; protected set; }
    public double CurrentHp { get; protected set; }
    public double MaxMp { get; protected set; } = 0;
    public double CurrentMp { get; protected set; } = 0;
    public int Level { get; protected set; }
    public double Attack { get; protected set; }
    public double Defence { get; protected set; }
    public int Pos { get; protected set; }
    public bool IsAlive { get { return CurrentHp > 0; } }
    public Dictionary<string, Skill> Skills { get; protected set; }

    public CharacterData(string name, int hp, double attack, double defence, int pos, int level)
    {
        Name = name;
        OriginHp = MaxHp = CurrentHp = hp;
        Level = level;
        Attack = attack;
        Defence = defence;
        Pos = pos;
    }

    public void SetPos(int pos)
    {
        Pos = pos;
    }

    public void BeHit(double attack, EffectType effectType = EffectType.Multiple)
    {
        var changeVlaue = attack - Defence;
        if (effectType == EffectType.Constant)
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
