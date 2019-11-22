using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Snake,
    Pig,
    DarkPig,
    Bat
}

public class EnemyData : CharacterData
{
    public EnemyType Type;
    public int DropExp;
    public float DropExpGrowthRate;
    public List<DropItem> DropItems = new List<DropItem>();

    private int _initDropExp;

    public EnemyData(string uid, EnemyDataRow dataRow, List<DropItem> dropItems, int level, List<string> skills)
        : base(uid, dataRow.ID, dataRow.Name, skills, dataRow.InitHp, dataRow.InitMp, dataRow.InitAtk, dataRow.InitDef, level,
            dataRow.HPGrowthRate, dataRow.MPGrowthRate, dataRow.AtkGrowthRate, dataRow.DefGrowthRate)
    {
        Type = dataRow.Type;
        DropItems = dropItems;
        DropExpGrowthRate = dataRow.DropExpGrowthRate;

        _initDropExp = dataRow.InitDropExp;
        DropExp = Mathf.FloorToInt(_initDropExp * Mathf.Pow((1 + DropExpGrowthRate), Level - 1));
    }
}