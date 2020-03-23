using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class GameRecord : IDisposable
{
    private static int[] _indexes;

    public string RecordID;
    public string RecordName;
    public string UpdateTime;

    public Dictionary<string, HeroRecordData> HeroRecord;
    public Dictionary<string, string> TeamRecord;
    public Dictionary<string,ItemRecordData> ItemRecord;

    public static int[] GetIndexes()
    {
        return _indexes;
    }

    public static void InitIndexes(int[] indexes)
    {
        _indexes = indexes;
    }

    public GameRecord()
    {
        for (int i = 0; i < _indexes.Length; i++)
        {
            if (_indexes[i] == -1)
            {
                RecordID = i.ToString();
                RecordName = "新建存档" + RecordID;
                _indexes[i] = i;
                UpdateTime = DateTime.Now.ToLocalTime().ToString();
                _InitRecord();
                break;
            }
        }
    }

    public GameRecord(string name = "")
    {
        for(int i = 0;i<_indexes.Length;i++)
        {
            if(_indexes[i] == -1)
            {
                RecordID = i.ToString();
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

        ItemRecord = new Dictionary<string, ItemRecordData>();
        AddItem("10000", 3, 3);
        AddItem("20000", 40, 3);
    }

    public void SetTeamHero(int pos,string uid)
    {
        if (pos == -1 || !HeroRecord.ContainsKey(uid))
            return;

        if (TeamRecord == null)
            TeamRecord = new Dictionary<string, string>();

        var posStr = pos.ToString();
        if (TeamRecord.ContainsKey(posStr))
            TeamRecord[posStr] = uid;
        else
            TeamRecord.Add(posStr, uid);
    }

    public void Dispose()
    {
        _indexes[int.Parse(RecordID)] = -1;
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
            ItemRecord = new Dictionary<string, ItemRecordData>();

        ItemRecordData item = new ItemRecordData { ID = id, Pos = pos, Count = count };
        ItemRecord.Add(pos.ToString(), item);
    }

    public void RemoveItem(string id,int pos,int count)
    {
        var posStr = pos.ToString();
        if(ItemRecord.ContainsKey(posStr))
        {
            var item = ItemRecord[posStr];
            if(string.Equals(item.ID,id))
            {
                item.Count -= count;
                if (item.Count <= 0)
                    ItemRecord.Remove(posStr);
            }
        }
    }
}

public class GameRecords
{
    public Dictionary<string, GameRecord> Records;
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
            Records = new Dictionary<string, GameRecord>();

        GameRecord.InitIndexes(Indexes);
    }

    public void RemoveRecord(int id)
    {
        var key = id.ToString();
        Records[key].Dispose();
        Indexes[id] = -1;
        Records.Remove(key);
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