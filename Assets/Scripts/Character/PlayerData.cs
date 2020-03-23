using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerData
{
    public Dictionary<string, HeroData> Heroes = new Dictionary<string, HeroData>();
    public Dictionary<string, HeroData> TeamHeroes = new Dictionary<string, HeroData>();

    public Item[] BackPack { get; private set; }
    private int _backPackSize;
    private GameRecord _gameRecord;

    public PlayerData(Dictionary<string, HeroData> heroes, Dictionary<string, string> teamHeroes , Dictionary<string, Item> items,int backPackSize)
    {
        _gameRecord = GameUtility.Instance.GetCurGameRecord();
        Heroes = heroes;
        foreach (var pair in teamHeroes)
            TeamHeroes.Add(pair.Key, Heroes[pair.Value]);
        _backPackSize = backPackSize;
        BackPack = new Item[_backPackSize];
        AddItems(items);
    }

    public void AddItems(List<Item> items)
    {
        foreach (var item in items)
            _AddExtraItem(item, item.Count);

        _SaveBackPackRecord();
    }

    public void AddItems(Dictionary<string, Item> items)
    {
        if (BackPack == null)
            BackPack = new Item[_backPackSize];

        foreach (var pair in items)
            _AddItem(int.Parse(pair.Key), pair.Value);

        _SaveBackPackRecord();
    }

    private void _AddItem(int pos,Item item)
    {
        var oldItem = BackPack[pos];
        if (oldItem == null)
        {
            BackPack[pos] = item;
            return;
        }
            
        if(string.Equals(oldItem.ID,item.ID))
        {
            int extra = oldItem.AddNum(item.Count);
            _AddExtraItem(item, extra);
            return;
        }
    }

    private int _GetEmptyPos()
    {
        for(int pos = 0;pos < _backPackSize;pos ++)
        {
            if (BackPack[pos] == null)
                return pos;
        }
        return -1;
    }

    private int _GetNotFullPos(string id)
    {
        for (int pos = 0; pos < _backPackSize; pos++)
        {
            var item = BackPack[pos];
            if (item == null || !string.Equals(id,item.ID) || item.IsFull)
                continue;

            return pos;
        }

        return -1;
    }

    private void _AddExtraItem(Item item,int extraCount)
    {
        while (extraCount > 0)
        {
            bool isNew = false;
            var pos = _GetNotFullPos(item.ID);
            if (pos < 0)
            {
                isNew = true;
                pos = _GetEmptyPos();
                if (pos < 0)
                    break;
            }

            if (isNew)
            {
                item = item.Copy(0);
                extraCount = item.AddNum(extraCount);
                BackPack[pos] = item;
            }
            else
                extraCount = BackPack[pos].AddNum(extraCount);
        }
    }

    public void SplitItem(int pos,int count)
    {
        var item = BackPack[pos];
        if (item == null || item.Count <= count)
            return;

        item.RemoveNum(count);
        _AddExtraItem(item, count);
        _SaveBackPackRecord();
    }

    public void FreshBackpack(int pos)
    {
        var item = BackPack[pos];
        if (item == null)
            return;

        if (item.Count <= 0)
            BackPack[pos] = null;

        _SaveBackPackRecord();
    }

    #region Hero
    public HeroLevelExpData AddExp(string uid,int exp, Dictionary<int, int> expTable)
    {
        var hero = Heroes[uid];
        var levelExpData = hero.AddExp(exp, expTable);
        _SaveHeroesRecord();
        return levelExpData;
    }
    #endregion

    private void _SaveToRecord()
    {
        _SaveBackPackRecord();
        _SaveHeroesRecord();
        _SaveTeamRecord();
    }

    private void _SaveBackPackRecord()
    {
        _gameRecord.ItemRecord.Clear();
        for(int i = 0;i< _backPackSize;i++)
        {
            var item = BackPack[i];
            if (item == null)
                continue;

            _gameRecord.AddItem(item.ID, i, item.Count);
        }
    }

    private void _SaveHeroesRecord()
    {
        _gameRecord.HeroRecord.Clear();
        foreach (var hero in Heroes.Values)
            _gameRecord.AddHero(hero.ID, hero.Exp, hero.Level, hero.UID);
    }

    private void _SaveTeamRecord()
    {
        _gameRecord.TeamRecord.Clear();
        foreach (var pair in TeamHeroes)
            _gameRecord.SetTeamHero(int.Parse(pair.Key), pair.Value.UID);
    }
}
