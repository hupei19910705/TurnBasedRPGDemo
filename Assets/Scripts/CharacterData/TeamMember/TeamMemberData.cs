using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MemberJob
{
    Warrior,
    Wizard
}

public class TeamMemberData
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
    public MemberJob Job { get; private set; }
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
    public Dictionary<SkillType,List<Skill>> Skills { get; private set; }
    public bool IsAlive { get { return CurrentHp > 0; } }
    public bool IsTurnEnd { get; private set; }

    private const string HEAD_IMAGE_KEY_PREFIX = "Texture/Characters/Team Member/";
    private const string DEATH_IMAGE_KEY_PREFIX = "Texture/Icons/Death/";

    public TeamMemberData(string name,MemberJob job,int hp,int mp,string headImageKey,string deathImageKey,double attack,double defence,int pos,int level)
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

    public void SetSkills(Dictionary<SkillType, List<Skill>> skills)
    {
        Skills = skills;
    }

    public void AddSkill(Skill skill)
    {
        if(Skills.ContainsKey(skill.Type))
        {
            foreach(var s in Skills[skill.Type])
            {
                if (!string.Equals(s.ID, skill.ID))
                    Skills[skill.Type].Add(skill);
            }
        }
        else
            Skills.Add(skill.Type, new List<Skill> { skill });
    }

    public void SetEndTurnFlag(bool endTurn)
    {
        IsTurnEnd = endTurn;
    }

    public void BeHit(double attack)
    {
        CurrentHp = (int)Math.Max(0, Math.Floor(CurrentHp - Math.Max(1, (attack - Defence))));
    }
}