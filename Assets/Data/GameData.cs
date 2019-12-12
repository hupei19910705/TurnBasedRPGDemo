using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public ConstantData ConstantData;
    public Dictionary<string, HeroDataRow> HeroTable;
    public Dictionary<HeroJobType, HeroJob> HeroJobTable;
    public Dictionary<HeroJobType, Dictionary<int,List<string>>> HeroUnlockSkillTable;
    public Dictionary<string, EnemyDataRow> EnemyTable;
    public Dictionary<EnemyType, Dictionary<int, List<string>>> EnemyUnlockSkillTable;
    public Dictionary<string, ItemRow> ItemTable;
    public Dictionary<string, SkillRow> SkillTable;
    public Dictionary<string, BuffRow> BuffTable;
    public Dictionary<string, EffectDataRow> EffectDataTable;
    public Dictionary<string, SkillEffectRow> EffectTable;
    public Dictionary<int, int> LevelExpTable;

    public GameData(Dictionary<string, HeroDataRow> heroes, 
        Dictionary<HeroJobType, HeroJob> heroJobs, 
        Dictionary<string, EnemyDataRow> enemies,
        Dictionary<string, ItemRow> items, 
        Dictionary<string, SkillRow> skills,
        Dictionary<string,BuffRow> buffs,
        Dictionary<string,EffectDataRow> effectDatas,
        Dictionary<string, SkillEffectRow> effects,
        Dictionary<int, int> levelExp,
        ConstantData constantData,
        Dictionary<string, UnLockHeroSkillData> heroUnlockSkills,
        Dictionary<string, UnLockEnemySkillData> enemyUnlockSkills)
    {
        HeroTable = heroes;
        _InitHeroJobTable(heroJobs);
        HeroJobTable = heroJobs;
        _InitEnemyTable(enemies);
        EnemyTable = enemies;
        ItemTable = items;

        _InitSkillTable(skills);
        SkillTable = skills;
        _InitBuffDatas(buffs);
        BuffTable = buffs;
        EffectDataTable = effectDatas;
        EffectTable = effects;

        LevelExpTable = levelExp;
        ConstantData = constantData;
        HeroUnlockSkillTable = _ConvertHeroUnlockSkillTable(heroUnlockSkills);
        EnemyUnlockSkillTable = _ConvertEnemyUnlockSkillTable(enemyUnlockSkills);
        BuffTable = buffs;
    }

    private void _InitHeroJobTable(Dictionary<HeroJobType, HeroJob> heroJobs)
    {
        if (heroJobs == null || heroJobs.Count == 0)
            return;

        foreach(var job in heroJobs.Values)
        {
            job.HPGrowthRate /= 100;
            job.MPGrowthRate /= 100;
            job.PAtkGrowthRate /= 100;
            job.MAtkGrowthRate /= 100;
            job.PDefGrowthRate /= 100;
            job.MDefGrowthRate /= 100;
        }
    }

    private void _InitEnemyTable(Dictionary<string, EnemyDataRow> enemies)
    {
        if (enemies == null || enemies.Count == 0)
            return;

        foreach (var enemy in enemies.Values)
        {
            enemy.HPGrowthRate /= 100;
            enemy.MPGrowthRate /= 100;
            enemy.PAtkGrowthRate /= 100;
            enemy.MAtkGrowthRate /= 100;
            enemy.PDefGrowthRate /= 100;
            enemy.MDefGrowthRate /= 100;
            enemy.DropExpGrowthRate /= 100;
        }
    }

    private void _InitSkillTable(Dictionary<string, SkillRow> skills)
    {
        if (skills == null || skills.Count == 0)
            return;

        foreach(var skill in skills.Values)
        {
            skill.MoveSpeed /= 100;
            skill.UseToSelfSideDataValue0 /= 100;
            skill.UseToSelfSideDataValue1 /= 100;
            skill.UseToOppositeSideDataValue0 /= 100;
            skill.UseToOppositeSideDataValue1 /= 100;
        }
    }

    private void _InitBuffDatas(Dictionary<string, BuffRow> datas)
    {
        if (datas == null || datas.Count == 0)
            return;

        foreach (var data in datas.Values)
        {
            data.DataValue0 /= 100;
            data.DataValue1 /= 100;
        }
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

public class ConstantData
{
    public int MAX_RECORD_COUNT;
    public int MAX_LEVEL;
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
    public float HPGrowthRate;
    public int InitMp;
    public float MPGrowthRate;
    public int InitPAtk;
    public float PAtkGrowthRate;
    public int InitMAtk;
    public float MAtkGrowthRate;
    public int InitPDef;
    public float PDefGrowthRate;
    public int InitMDef;
    public float MDefGrowthRate;
    public string HeadImageKey;
    public string DeathImageKey;
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
    public float HPGrowthRate;
    public int InitMp;
    public float MPGrowthRate;
    public int InitPAtk;
    public float PAtkGrowthRate;
    public int InitMAtk;
    public float MAtkGrowthRate;
    public int InitPDef;
    public float PDefGrowthRate;
    public int InitMDef;
    public float MDefGrowthRate;
    public int InitDropExp;
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
    public string Name;
    public List<string> Effects;
    public int MpCost;
    public bool IsRemote;
    public string ImageKey;
    public float MoveSpeed;
    public string Desc;

    public string UseToSelfSideDataId0;
    public float UseToSelfSideDataValue0;
    public string UseToSelfSideDataId1;
    public float UseToSelfSideDataValue1;
    public List<string> UseToSelfSideBuffIds;

    public string UseToOppositeSideDataId0;
    public float UseToOppositeSideDataValue0;
    public string UseToOppositeSideDataId1;
    public float UseToOppositeSideDataValue1;
    public List<string> UseToOppositeSideBuffIds;
}

public class BuffRow
{
    public string ID;
    public string Name;
    public BuffType Type;
    public string IconKey;
    public List<string> Effects;
    public int RoundCount;

    public string DataId0;
    public float DataValue0;
    public string DataId1;
    public float DataValue1;
}

public class EffectDataRow
{
    public string ID;
    public CharacterValueType ValueType;
    public ValueEffectWay EffectWay;
    public SkillEffectType EffectType;
}

public class SkillEffectRow
{
    public string ID;
    public SkillEffectViewType Type;
    public string PrefabKey;
    public bool IsOverTime;
    public bool Ballistic;
}