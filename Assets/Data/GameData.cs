﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public Dictionary<string, HeroDataRow> HeroTable;
    public Dictionary<HeroJobType, HeroJob> HeroJobTable;
    public Dictionary<string, EnemyDataRow> EnemyTable;
    public Dictionary<string, ItemRow> ItemTable;
    public Dictionary<string, SkillRow> SkillTable;

    public GameData(Dictionary<string, HeroDataRow> heroes, Dictionary<HeroJobType, HeroJob> heroJobs, Dictionary<string, EnemyDataRow> enemies,
        Dictionary<string, ItemRow> items, Dictionary<string, SkillRow> skills)
    {
        HeroTable = heroes;
        HeroJobTable = heroJobs;
        EnemyTable = enemies;
        ItemTable = items;
        SkillTable = skills;
    }
}

public class HeroDataRow
{
    public string ID;
    public string Name;
    public HeroJobType Job;
    public List<string> Skills;
}

public class HeroJob
{
    public HeroJobType Type;
    public string Name;
    public double OriginHp;
    public double OriginMp;
    public string HeadImageKey;
    public string DeathImageKey;
    public double Attack;
    public double Defence;
}

public class EnemyDataRow
{
    public string ID;
    public string Name;
    public EnemyType Type;
    public double OriginHp;
    public double OriginMp;
    public double Attack;
    public double Defence;
    public int DropExp;
}

public class ItemRow
{
    public string ID;
    public ItemType Type;
    public string Name;
    public double EffectValue;
    public string IconKey;
}

public class SkillRow
{
    public string ID;
    public EffectType EffectType;
    public string Name;
    public int MpCost;
    public SkillVariety Variety;
    public bool IsRemote;
    public string ImageKey;
    public float Multiple = 1f;
    public int EffectValue;
    public EffectiveWay EffectiveWay;
    public EffectiveResult EffectiveResult;
    public float MoveSpeed;
    public float Duration = 0f;
}