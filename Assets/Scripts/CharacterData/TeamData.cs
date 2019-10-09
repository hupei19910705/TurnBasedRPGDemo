using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamData
{
    public Dictionary<int, HeroData> Heroes = new Dictionary<int, HeroData>();
    //public Dictionary<ItemType, List<Item>> BackPack { get; private set; }

    public Dictionary<int,Item> BackPack { get; private set; }
    private const int BACKPACK_MAX_SIZE = 50;

    public TeamData(Dictionary<int, HeroData> heroes, Dictionary<int, Item> backpack)
    {
        Heroes = heroes;
        BackPack = backpack;
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
        for(int i=0;i<items.Count;i++)
        {
            var newItem = items[i];

            if (BackPack.ContainsKey(items[i].Pos))
            {
                var oldItem = BackPack[items[i].Pos];
                if(string.Equals(oldItem.ID, newItem.ID))
                {
                    var extra = oldItem.AddNum(newItem.Count);
                    _AddItemAtEmptyPos(newItem, extra);
                }
                else
                    _AddItemAtEmptyPos(newItem, newItem.Count);
            }
            else
            {
                if (newItem.IsFull)
                {
                    var item = newItem.Copy(newItem.MaxCount);
                    BackPack.Add(item.Pos, item);

                    var extra = newItem.Count - newItem.MaxCount;
                    _AddItemAtEmptyPos(newItem, extra);
                }
                else
                {
                    BackPack.Add(newItem.Pos, newItem);
                    _CheckBackpackSize();
                }
            }
        }
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

    private void _AddItemAtEmptyPos(Item item,int count)
    {
        while (count > 0)
        {
            var pos = _GetEmptyPos();
            if (pos < 0)
                break;
            item = item.Copy(0);
            item.Pos = pos;
            count = item.AddNum(count);
            BackPack.Add(pos, item);
            _CheckBackpackSize();
        }
    }

    public void SplitItem(int pos,int count)
    {
        if (!BackPack.ContainsKey(pos) || BackPack[pos].Count <= count || BackPack.Count >= BACKPACK_MAX_SIZE)
            return;
        var item = BackPack[pos];
        item.RemoveNum(count);
        _AddItemAtEmptyPos(item, count);
    }

    public void FreshBackpack(int pos)
    {
        if(BackPack.ContainsKey(pos))
        {
            var item = BackPack[pos];
            if (item.Count <= 0)
                BackPack.Remove(pos);
        }
    }
}
