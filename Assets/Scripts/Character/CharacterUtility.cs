using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUtility
{
    private static CharacterUtility _instance;
    public static CharacterUtility Instance
    {
        get
        {
            if (_instance == null)
                _instance = new CharacterUtility();
            return _instance;
        }
    }

    private GameData _gameData;

    public void Init(GameData gameData)
    {
        _gameData = gameData;
    }

    public List<string> GetUnLockHeroSkills(HeroJobType job, int level)
    {
        List<string> skills = new List<string>();

        var unlockSkills = _gameData.HeroUnlockSkillTable;
        if (unlockSkills.ContainsKey(job))
        {
            var levelUnlockSkills = unlockSkills[job];
            for (int i = 1; i <= level; i++)
            {
                if (levelUnlockSkills.ContainsKey(i))
                    skills.AddRange(levelUnlockSkills[i]);
            }
        }
        
        return skills;
    }

    public List<string> GetUnLockEnemySkills(EnemyType type, int level)
    {
        List<string> skills = new List<string>();

        var unlockSkills = _gameData.EnemyUnlockSkillTable;
        if (unlockSkills.ContainsKey(type))
        {
            var levelUnlockSkills = unlockSkills[type];
            for (int i = 1; i <= level; i++)
            {
                if (levelUnlockSkills.ContainsKey(i))
                    skills.AddRange(levelUnlockSkills[i]);
            }
        }

        return skills;
    }

    public Dictionary<string,EffectDataRow> GetEffectDataRows(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
            return null;

        Dictionary<string, EffectDataRow> result = new Dictionary<string, EffectDataRow>();
        var dataTable = _gameData.EffectDataTable;
        foreach(var id in ids)
        {
            if (dataTable.ContainsKey(id))
                result.Add(id, dataTable[id]);
        }

        return result;
    }

    public EffectDataRow GetEffectDataRow(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        var dataTable = _gameData.EffectDataTable;
        if (dataTable.ContainsKey(id))
            return dataTable[id];

        return null;
    }

    public List<BuffRow> GetBuffRows(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
            return null;

        List<BuffRow> result = new List<BuffRow>();
        var dataTable = _gameData.BuffTable;
        foreach (var id in ids)
        {
            if (dataTable.ContainsKey(id))
                result.Add(dataTable[id]);
        }

        return result;
    }
}
