using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroJobType
{
    Warrior,
    Wizard
}

public class HeroData : CharacterData
{
    public HeroJobType Job;
    public double Exp = 0;
    public string HeadImageKey;
    public string DeathImageKey;
    public bool IsTurnEnd = false;

    private const string HEAD_IMAGE_KEY_PREFIX = "Texture/Characters/Team Member/";
    private const string DEATH_IMAGE_KEY_PREFIX = "Texture/Icons/Death/";

    public HeroData(string uid,HeroDataRow dataRow,HeroJob heroJob,double exp,int level,List<string> skills)
        : base(uid,dataRow.ID,dataRow.Name, skills, heroJob.OriginHp, heroJob.OriginMp, heroJob.Attack, heroJob.Defence, level)
    {
        Job = dataRow.Job;
        HeadImageKey = HEAD_IMAGE_KEY_PREFIX + heroJob.HeadImageKey;
        DeathImageKey = DEATH_IMAGE_KEY_PREFIX + heroJob.DeathImageKey;
        Exp = exp;

        Attack *= 10;
    }

    public void SetEndTurnFlag(bool endTurn)
    {
        IsTurnEnd = endTurn;
    }

    public HeroLevelExpData AddExp(int exp,Dictionary<int,int> expTable)
    {
        if (!IsAlive)
            exp = 0;

        int oldLevel = Level;
        double oldExp = Exp;

        var maxExp = expTable[Level];
        float oldExpRate = (float)oldExp / maxExp * 100f;

        Exp += exp;
        while (Exp >= maxExp)
        {
            Level++;
            Exp -= maxExp;
            maxExp = expTable[Level];
        }
        var newExpRate = (float)Exp / maxExp * 100f;

        return new HeroLevelExpData(oldLevel, oldExpRate, Level, newExpRate);
    }
}

public class HeroLevelExpData
{
    public int OldLevel { get; private set; }
    public float OldExpRate { get; private set; }
    public int NewLevel { get; private set; }
    public float NewExpRate { get; private set; }

    public HeroLevelExpData(int oldLevel,float oldExpRate,int newLevel,float newExpRate)
    {
        OldLevel = oldLevel;
        OldExpRate = oldExpRate;
        NewLevel = newLevel;
        NewExpRate = newExpRate;
    }
}