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

public class EnemyData
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
    public string Name { get; private set; }
    public EnemyType Type { get; private set; }
    public double OriginHp { get; private set; }
    public double MaxHp { get; private set; }
    public double CurrentHp { get; private set; }
    public int Level { get; private set; }
    public int DropExp { get; private set; }
    public double Attack { get; private set; }
    public double Defence { get; private set; }
    public int Pos;
    public bool IsAlive { get { return CurrentHp > 0; } }
    public List<DropItem> DropItems = new List<DropItem>();

    public EnemyData(string name, EnemyType type, int hp,double attack, double defence, int pos,int level,int dropExp)
    {
        Name = name;
        Type = type;
        OriginHp = MaxHp = CurrentHp = hp;
        Level = level;
        DropExp = dropExp;
        Attack = attack;
        Defence = defence;
        Pos = pos;
    }

    public void SetDropItems(List<DropItem> dropItems)
    {
        DropItems = dropItems;
    }

    public void BeHit(double attack)
    {
        CurrentHp = (int)Math.Max(0, Math.Floor(CurrentHp - Math.Max(1, (attack - Defence))));
    }
}
