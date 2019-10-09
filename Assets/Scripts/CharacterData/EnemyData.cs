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
            _id = string.Format("enemy_{0}", _serial.ToString().PadLeft(6));
        }
    }
    public EnemyType Type { get; private set; }
    public int DropExp { get; private set; }
    public List<DropItem> DropItems = new List<DropItem>();

    public EnemyData(string name, EnemyType type, int hp,double attack, double defence, int pos,int level,int dropExp)
         : base(name, hp, attack, defence, pos, level)
    {
        Type = type;
        DropExp = dropExp;
    }

    public void SetDropItems(List<DropItem> dropItems)
    {
        DropItems = dropItems;
    }
}
