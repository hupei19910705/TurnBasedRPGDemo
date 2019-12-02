﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public Dictionary<string, HeroDataRow> HeroTable;
    public Dictionary<HeroJobType, HeroJob> HeroJobTable;
    public Dictionary<HeroJobType, Dictionary<int,List<string>>> HeroUnlockSkillTable;
    public Dictionary<string, EnemyDataRow> EnemyTable;
    public Dictionary<EnemyType, Dictionary<int, List<string>>> EnemyUnlockSkillTable;
    public Dictionary<string, ItemRow> ItemTable;
    public Dictionary<string, SkillRow> SkillTable;
    public Dictionary<int, int> LevelExpTable;
    public Dictionary<string, BuffRow> BuffTable;
    public ConstantData ConstantData;

    public GameData(Dictionary<string, HeroDataRow> heroes, Dictionary<HeroJobType, HeroJob> heroJobs, Dictionary<string, EnemyDataRow> enemies,
        Dictionary<string, ItemRow> items, Dictionary<string, SkillRow> skills, Dictionary<int, int> levelExp, ConstantData constantData,
        Dictionary<string, UnLockHeroSkillData> heroUnlockSkills, Dictionary<string, UnLockEnemySkillData> enemyUnlockSkills, 
        Dictionary<string, BuffRow> buffs)
    {
        HeroTable = heroes;
        HeroJobTable = heroJobs;
        EnemyTable = enemies;
        ItemTable = items;
        SkillTable = skills;
        LevelExpTable = levelExp;
        ConstantData = constantData;
        HeroUnlockSkillTable = _ConvertHeroUnlockSkillTable(heroUnlockSkills);
        EnemyUnlockSkillTable = _ConvertEnemyUnlockSkillTable(enemyUnlockSkills);
        BuffTable = buffs;
    }

    private Dictionary<HeroJobType, Dictionary<int, List<string>>> _ConvertHeroUnlockSkillTable(Dictionary<string, UnLockHeroSkillData> heroUnlockSkills)
    {
        Dictionary<HeroJobType, Dictionary<int, List<string>>> result = new Dictionary<HeroJobType, Dictionary<int, List<string>>>();
        foreach (var data in heroUnlockSkills.Values)
        {
            if(result.ContainsKey(data.HeroJobType))
            {
                var levelSkills = result[data.HeroJobType];
                if (levelSkills.ContainsKey(data.Level))
                    levelSkills[data.Level] = data.Skills;
                else
                    levelSkills.Add(data.Level, data.Skills);
            }
            else
            {
                var levelSkills = new Dictionary<int, List<string>>();
                levelSkills.Add(data.Level, data.Skills);
                result.Add(data.HeroJobType, levelSkills);
            }
        }

        return result;
    }

    private Dictionary<EnemyType, Dictionary<int, List<string>>> _ConvertEnemyUnlockSkillTable(Dictionary<string, UnLockEnemySkillData> enemyUnlockSkills)
    {
        Dictionary<EnemyType, Dictionary<int, List<string>>> result = new Dictionary<EnemyType, Dictionary<int, List<string>>>();
        foreach (var data in enemyUnlockSkills.Values)
        {
            if (result.ContainsKey(data.EnemyType))
            {
                var levelSkills = result[data.EnemyType];
                if (levelSkills.ContainsKey(data.Level))
                    levelSkills[data.Level] = data.Skills;
                else
                    levelSkills.Add(data.Level, data.Skills);
            }
            else
            {
                var levelSkills = new Dictionary<int, List<string>>();
                levelSkills.Add(data.Level, data.Skills);
                result.Add(data.EnemyType, levelSkills);
            }
        }

        return result;
    }
}

public class HeroDataRow
{
    public string ID;
    public string Name;
    public HeroJobType Job;
}

public class HeroJob
{
    public HeroJobType Type;
    public string Name;
    public int InitHp;
    public int InitMp;
    public string HeadImageKey;
    public string DeathImageKey;
    public int InitAtk;
    public int InitDef;
    public float HPGrowthRate;
    public float MPGrowthRate;
    public float AtkGrowthRate;
    public float DefGrowthRate;
}

public class UnLockHeroSkillData
{
    public string ID;
    public HeroJobType HeroJobType;
    public int Level;
    public List<string> Skills;
}

public class EnemyDataRow
{
    public string ID;
    public string Name;
    public EnemyType Type;
    public int InitHp;
    public int InitMp;
    public int InitAtk;
    public int InitDef;
    public int InitDropExp;
    public float HPGrowthRate;
    public float MPGrowthRate;
    public float AtkGrowthRate;
    public float DefGrowthRate;
    public float DropExpGrowthRate;
}

public class UnLockEnemySkillData
{
    public string ID;
    public EnemyType EnemyType;
    public int Level;
    public List<string> Skills;
}

public class ItemRow
{
    public string ID;
    public ItemType Type;
    public string Name;
    public int EffectValue;
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
    public float Multiple;
    public int EffectValue;
    public EffectiveWay EffectiveWay;
    public EffectiveResult EffectiveResult;
    public float MoveSpeed;
    public int Duration;
    public bool IsConstant;
    public List<string> BuffIds;
}

public class ConstantData
{
    public int MAX_RECORD_COUNT;
    public int MAX_LEVEL;
}

public class BuffRow
{
    public string ID;
    public BuffType Type;
    public SpecialBuffType SType;
    public EffectType EffectType;
}