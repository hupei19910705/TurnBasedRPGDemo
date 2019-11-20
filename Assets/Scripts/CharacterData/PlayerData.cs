using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerData
{
    public Dictionary<string, HeroData> Heroes = new Dictionary<string, HeroData>();
    public Dictionary<int, HeroData> TeamHeroes = new Dictionary<int, HeroData>();

    public Dictionary<int,Item> BackPack { get; private set; }
    private const int BACKPACK_MAX_SIZE = 50;

    private GameRecord _gameRecord;

    public PlayerData(Dictionary<string, HeroData> heroes, Dictionary<int, string> teamHeroes , List<Item> items)
    {
        _gameRecord = GameUtility.Instance.GetCurGameRecord();
        Heroes = heroes;
        foreach (var pair in teamHeroes)
            TeamHeroes.Add(pair.Key, Heroes[pair.Value]);
        BackPack = new Dictionary<int, Item>();
        AddItems(items);
        _CheckBackpackSize();
    }

    private void _CheckBackpackSize()
    {
        if(BackPack.Count > BACKPACK_MAX_SIZE)
        {
            var dict = new Dictionary<int, Item>();
            int count = BACKPACK_MAX_SIZE;
            foreach (var pair in BackPack)
            {
                dict.Add(pair.Key, pair.Value);
                count--;
                if (count <= 0)
                    break;
            }
            BackPack.Clear();
            BackPack = dict;
        }
    }

    public void AddItems(List<Item> items)
    {
        if (BackPack == null)
            BackPack = new Dictionary<int, Item>();

        for (int i = 0; i < items.Count; i++)
        {
            var newItem = items[i];
            int extra = newItem.Count;

            if (BackPack.ContainsKey(items[i].Pos))
            {
                var oldItem = BackPack[items[i].Pos];
                if (string.Equals(oldItem.ID, newItem.ID))
                    extra = oldItem.AddNum(newItem.Count);
            }

            _AddExtraItem(newItem, extra);
        }

        _SaveBackPackRecord();
    }

    private int _GetEmptyPos()
    {
        for(int pos = 0;pos < BACKPACK_MAX_SIZE;pos ++)
        {
            if (BackPack.ContainsKey(pos))
                continue;
            return pos;
        }
        return -1;
    }

    private int _GetNotFullPos(string id)
    {
        var items = BackPack.Values.ToList().FindAll(t => string.Equals(id, t.ID)).OrderBy(o => o.Pos).ToList();
        for(int i = 0;i<items.Count;i++)
        {
            if (!items[i].IsFull)
                return items[i].Pos;
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

            item = item.Copy(0);
            item.Pos = pos;
            
            if (isNew)
            {
                extraCount = item.AddNum(extraCount);
                BackPack.Add(pos, item);
                _CheckBackpackSize();
            }
            else
                extraCount = BackPack[pos].AddNum(item.Count);
        }
    }

    public void SplitItem(int pos,int count)
    {
        if (!BackPack.ContainsKey(pos) || BackPack[pos].Count <= count || BackPack.Count >= BACKPACK_MAX_SIZE)
            return;
        var item = BackPack[pos];
        item.RemoveNum(count);
        _AddExtraItem(item, count);
        _SaveBackPackRecord();
    }

    public void FreshBackpack(int pos)
    {
        if(BackPack.ContainsKey(pos))
        {
            var item = BackPack[pos];
            if (item.Count <= 0)
                BackPack.Remove(pos);
        }
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
        foreach(var item in BackPack.Values)
            _gameRecord.AddItem(item.ID, item.Pos, item.Count);
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
            _gameRecord.SetTeamHero(pair.Key, pair.Value.UID);
    }
}
