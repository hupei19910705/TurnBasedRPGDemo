using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class GameRecord : IDisposable
{
    private static int[] _indexes = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public int RecordID;
    public string RecordName;
    public string UpdateTime;

    public List<HeroRecordData> HeroRecord;
    public Dictionary<int, string> TeamRecord;
    public List<ItemRecordData> ItemRecord;

    public GameRecord(string name = "")
    {
        for(int i = 0;i<_indexes.Length;i++)
        {
            if(_indexes[i] == -1)
            {
                RecordID = i;
                RecordName = string.IsNullOrEmpty(name) ? "新建存档" + RecordID : name;
                _indexes[i] = i;
                UpdateTime = DateTime.Now.ToLocalTime().ToString();
                _InitRecord();
                break;
            }
        }
    }

    private void _InitRecord()
    {
        var uid1 = AddHero("hero_0001", 0, 1);
        var uid2 = AddHero("hero_0002", 0, 1);
        var uid3 = AddHero("hero_0003", 0, 1);

        TeamRecord = new Dictionary<int, string>
        {
            {0,uid1},
            {3,uid2},
            {2,uid3},
        };

        AddItem("10000", 0, 3);
        AddItem("10001", 2, 120);
        AddItem("10000", 5, 3);
        AddItem("20000", 9, 5);
    }

    public void Dispose()
    {
        _indexes[RecordID] = -1;
    }

    public string AddHero(string id,int exp,int level,string uid = "",int pos = -1)
    {
        if (HeroRecord == null)
            HeroRecord = new List<HeroRecordData>();

        if (string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();

        HeroRecordData hero = new HeroRecordData
        {
            UID = uid,
            ID = id,
            Pos = pos,
            Exp = exp,
            Level = level
        };

        HeroRecord.Add(hero);
        return uid;
    }

    public void RemoveHero(string uid)
    {
        for(int i = HeroRecord.Count;i>=0;i--)
        {
            if (string.Equals(uid, HeroRecord[i].UID))
                HeroRecord.RemoveAt(i);
        }
    }

    public void AddItem(string id,int pos,int count)
    {
        if (ItemRecord == null)
            ItemRecord = new List<ItemRecordData>();

        ItemRecordData item = new ItemRecordData { ID = id, Pos = pos, Count = count };
        ItemRecord.Add(item);
    }

    public void RemoveItem(string id,int pos,int count)
    {
        for(int i = ItemRecord.Count;i>=0;i--)
        {
            var item = ItemRecord[i];
            if (string.Equals(id, item.ID) && pos == item.Pos)
            {
                item.Count -= count;
                if (item.Count <= 0)
                    ItemRecord.RemoveAt(i);
                break;
            }
        }
    }
}

public class GameRecords : Dictionary<int, GameRecord> { }

public class HeroRecordData
{
    public string UID;
    public string ID;
    public int Pos;
    public int Exp;
    public int Level;
}

public class ItemRecordData
{
    public string ID;
    public int Pos;
    public int Count;
}