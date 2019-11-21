using System;
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
    public ConstantData ConstantData;

    public GameData(Dictionary<string, HeroDataRow> heroes, Dictionary<HeroJobType, HeroJob> heroJobs, Dictionary<string, EnemyDataRow> enemies,
        Dictionary<string, ItemRow> items, Dictionary<string, SkillRow> skills, Dictionary<int, int> levelExp, ConstantData constantData,
        Dictionary<string, UnLockHeroSkillData> heroUnlockSkills, Dictionary<string, UnLockEnemySkillData> enemyUnlockSkills)
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
    public double OriginHp;
    public double OriginMp;
    public string HeadImageKey;
    public string DeathImageKey;
    public double Attack;
    public double Defence;
    public double HPGrowthRate;
    public double MPGrowthRate;
    public double AtkGrowthRate;
    public double DefGrowthRate;
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
    public double OriginHp;
    public double OriginMp;
    public double Attack;
    public double Defence;
    public int DropExp;
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
    public float Multiple;
    public int EffectValue;
    public EffectiveWay EffectiveWay;
    public EffectiveResult EffectiveResult;
    public float MoveSpeed;
    public float Duration;
    public bool IsConstant;
}

public class ConstantData
{
    public int MAX_RECORD_COUNT;
}