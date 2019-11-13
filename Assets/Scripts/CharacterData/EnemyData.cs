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
    public List<DropItem> DropItems = new List<DropItem>();

    public EnemyData(string name, EnemyType type, int hp,int mp,double attack, double defence, int pos,int level,int dropExp)
         : base(name, hp,mp, attack, defence, pos, level)
    {
        Type = type;
        DropExp = dropExp;
    }

    public void SetDropItems(List<DropItem> dropItems)
    {
        DropItems = dropItems;
    }
}
