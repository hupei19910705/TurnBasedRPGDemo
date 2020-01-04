using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class GameRecord : IDisposable
{
    private static int[] _indexes;

    public int RecordID;
    public string RecordName;
    public string UpdateTime;

    public Dictionary<string, HeroRecordData> HeroRecord;
    public Dictionary<int, string> TeamRecord;
    public Dictionary<int,ItemRecordData> ItemRecord;

    public static int[] GetIndexes()
    {
        return _indexes;
    }

    public static void InitIndexes(int[] indexes)
    {
        _indexes = indexes;
    }

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

        SetTeamHero(0, uid1);
        SetTeamHero(3, uid2);
        SetTeamHero(2, uid3);

        ItemRecord = new Dictionary<int, ItemRecordData>();
        AddItem("10000", 3, 3);
        AddItem("20000", 40, 3);
    }

    public void SetTeamHero(int pos,string uid)
    {
        if (pos == -1 || !HeroRecord.ContainsKey(uid))
            return;

        if (TeamRecord == null)
            TeamRecord = new Dictionary<int, string>();

        if (TeamRecord.ContainsKey(pos))
            TeamRecord[pos] = uid;
        else
            TeamRecord.Add(pos, uid);
    }

    public void Dispose()
    {
        _indexes[RecordID] = -1;
    }

    public string AddHero(string id,double exp,int level,string uid = "")
    {
        if (HeroRecord == null)
            HeroRecord = new Dictionary<string, HeroRecordData>();

        if (string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();

        HeroRecordData hero = new HeroRecordData
        {
            UID = uid,
            ID = id,
            Exp = exp,
            Level = level
        };

        HeroRecord.Add(uid, hero);
        return uid;
    }

    public void RemoveHero(string uid)
    {
        if (HeroRecord.ContainsKey(uid))
            HeroRecord.Remove(uid);
    }

    public void AddItem(string id,int pos,int count)
    {
        if (ItemRecord == null)
            ItemRecord = new Dictionary<int, ItemRecordData>();

        ItemRecordData item = new ItemRecordData { ID = id, Pos = pos, Count = count };
        ItemRecord.Add(pos, item);
    }

    public void RemoveItem(string id,int pos,int count)
    {
        if(ItemRecord.ContainsKey(pos))
        {
            var item = ItemRecord[pos];
            if(string.Equals(item.ID,id))
            {
                item.Count -= count;
                if (item.Count <= 0)
                    ItemRecord.Remove(pos);
            }
        }
    }
}

public class GameRecords
{
    public Dictionary<int, GameRecord> Records;
    public int[] Indexes;

    public GameRecords()
    {
        Init();
    }

    public void Init()
    {
        if (Indexes == null)
            Indexes = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        if (Records == null)
            Records = new Dictionary<int, GameRecord>();

        GameRecord.InitIndexes(Indexes);
    }

    public void RemoveRecord(int id)
    {
        Records[id].Dispose();
        Indexes[id] = -1;
        Records.Remove(id);
    }
}

public class HeroRecordData
{
    public string UID;
    public string ID;
    public double Exp;
    public int Level;
}

public class ItemRecordData
{
    public string ID;
    public int Pos;
    public int Count;
}