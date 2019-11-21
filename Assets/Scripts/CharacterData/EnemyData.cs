﻿using System;
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
    public List<DropItem> DropItems = new List<DropItem>();

    public EnemyData(string uid,EnemyDataRow dataRow, List<DropItem> dropItems,int level)
        : base(uid, dataRow.ID, dataRow.Name, dataRow.Skills, dataRow.OriginHp, dataRow.OriginMp, dataRow.Attack, dataRow.Defence, level)
    {
        Type = dataRow.Type;
        DropExp = dataRow.DropExp;
        DropItems = dropItems;
    }
}