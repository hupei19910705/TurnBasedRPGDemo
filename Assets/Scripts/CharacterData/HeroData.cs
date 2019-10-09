using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroJob
{
    Warrior,
    Wizard
}

public class HeroData : CharacterData
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
            _id = string.Format("hero_{0}", _serial.ToString().PadLeft(6));
        }
    }
    public HeroJob Job { get; private set; }
    public double Exp { get; private set; }
    public string HeadImageKey { get; private set; }
    public string DeathImageKey { get; private set; }
    public bool IsTurnEnd { get; private set; }

    private const string HEAD_IMAGE_KEY_PREFIX = "Texture/Characters/Team Member/";
    private const string DEATH_IMAGE_KEY_PREFIX = "Texture/Icons/Death/";

    public HeroData(string name, HeroJob job, int hp, int mp, string headImageKey, string deathImageKey, double attack, double defence, int pos, int level)
        : base(name, hp, attack, defence, pos, level)
    {
        Job = job;
        MaxMp = CurrentMp = mp;
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