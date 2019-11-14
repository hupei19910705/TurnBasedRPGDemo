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

    public HeroData(HeroDataRow dataRow,HeroJob heroJob,double exp,int pos,int level)
        : base(dataRow.Name, dataRow.Skills, heroJob.OriginHp, heroJob.OriginMp, heroJob.Attack, heroJob.Defence, pos, level)
    {
        Job = dataRow.Job;
        HeadImageKey = HEAD_IMAGE_KEY_PREFIX + heroJob.HeadImageKey;
        DeathImageKey = DEATH_IMAGE_KEY_PREFIX + heroJob.DeathImageKey;
        Exp = exp;
    }

    public void SetEndTurnFlag(bool endTurn)
    {
        IsTurnEnd = endTurn;
    }
}