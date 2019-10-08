using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroJob
{
    Warrior,
    Wizard
}

public class HeroData
{
    private static int _serial = 0;
    private string _id;
    public string ID
    {
        get { return _id; }
        set
        {
            if (_serial < int.MaxValue)
                _serial++;
            else
                _serial = 0;
            _id = string.Format("member_{0}", _serial.ToString().PadLeft(6));
        }
    }
    public string Name { get; private set; }
    public HeroJob Job { get; private set; }
    public double OriginHp { get; private set; }
    public double MaxHp { get; private set; }
    public double CurrentHp { get; private set; }
    public double MaxMp { get; private set; }
    public double CurrentMp { get; private set; }
    public int Level { get; private set; }
    public double Exp { get; private set; }
    public string HeadImageKey { get; private set; }
    public string DeathImageKey { get; private set; }
    public double Attack { get; private set; }
    public double Defence { get; private set; }
    public int Pos;
    public Dictionary<string,Skill> Skills { get; private set; }
    public bool IsAlive { get { return CurrentHp > 0; } }
    public bool IsTurnEnd { get; private set; }

    private const string HEAD_IMAGE_KEY_PREFIX = "Texture/Characters/Team Member/";
    private const string DEATH_IMAGE_KEY_PREFIX = "Texture/Icons/Death/";

    public HeroData(string name,HeroJob job,int hp,int mp,string headImageKey,string deathImageKey,double attack,double defence,int pos,int level)
    {
        Name = name;
        Job = job;
        OriginHp = MaxHp = CurrentHp = hp;
        MaxMp = CurrentMp = mp;
        HeadImageKey = HEAD_IMAGE_KEY_PREFIX + headImageKey;
        DeathImageKey = DEATH_IMAGE_KEY_PREFIX + deathImageKey;
        Attack = attack;
        Defence = defence;
        Pos = pos;
        Level = level;
        Exp = 0;
        IsTurnEnd = false;
    }

    public void SetSkills(Dictionary<string,Skill> skills)
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

    public void SetEndTurnFlag(bool endTurn)
    {
        IsTurnEnd = endTurn;
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
}