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
}
