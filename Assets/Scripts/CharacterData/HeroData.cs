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
    public double Exp;
    public string HeadImageKey;
    public string DeathImageKey;
    public bool IsTurnEnd;

    private const string HEAD_IMAGE_KEY_PREFIX = "Texture/Characters/Team Member/";
    private const string DEATH_IMAGE_KEY_PREFIX = "Texture/Icons/Death/";

    public HeroData(string name, HeroJobType job, int hp, int mp, string headImageKey, string deathImageKey, double attack, double defence, int pos, int level)
        : base(name, hp,mp, attack, defence, pos, level)
    {
        Job = job;
        HeadImageKey = HEAD_IMAGE_KEY_PREFIX + headImageKey;
        DeathImageKey = DEATH_IMAGE_KEY_PREFIX + deathImageKey;
        Exp = 0;
        IsTurnEnd = false;
    }

    public void SetEndTurnFlag(bool endTurn)
    {
        IsTurnEnd = endTurn;
    }
}