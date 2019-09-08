using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    Potion,
    Other
}

public class Item
{
    public ItemType Type { get; private set; }
    public string ID { get; private set; }
    public int Count { get; private set; }
    public string Name { get; private set; }
    public double EffectValue { get; private set; }
    public int Pos;
    public string IconKey { get; private set; }
    private string _inCompleteIconKey = string.Empty;

    public int MaxCount = 99;

    public Item(ItemType type,string id,int count,string name,double effectValue,int pos,string iconKey)
    {
        Type = type;
        ID = id;
        Count = count;
        Name = name;
        EffectValue = effectValue;
        Pos = pos;
        IconKey = _GetIconKeyPrefixByItemType(Type) + iconKey;
        _inCompleteIconKey = iconKey;
    }

    public Item(ItemType type, string id,string name, double effectValue, int pos, string iconKey)
    {
        Type = type;
        ID = id;
        Name = name;
        EffectValue = effectValue;
        Pos = pos;
        IconKey = _GetIconKeyPrefixByItemType(Type) + iconKey;
        _inCompleteIconKey = iconKey;
        Count = 1;
    }

    public void SetItemCount(int count)
    {
        Count = count;
    }

    public string GetInCompleteIconKey()
    {
        return _inCompleteIconKey;
    }

    private string _GetIconKeyPrefixByItemType(ItemType type)
    {
        switch(type)
        {
            case ItemType.Potion:
                return "Texture/Icons/Potion/";
            case ItemType.Other:
                return "Texture/Icons/Other/";
        }
        return "Texture/Icons/Other/";
    }

    public static void ReplaceItem(Item item1,Item item2)
    {
        var item = item1;
        item1 = item2;
        item2 = item;
    }

    public int AddNum(int num)
    {
        bool full = Count + num > MaxCount;
        if(full)
        {
            Count = MaxCount;
            return Count + num - MaxCount;
        }
        else
        {
            Count += num;
            return 0;
        }
    }

    public void RemoveNum(int num)
    {
        if (Count <= num || num <= 0)
            return;

        Count -= num;
    }

    public Item Copy(int changeCount = -1)
    {
        var count = changeCount == -1 ? Count : changeCount;
        var item = new Item(Type, ID, count, Name, EffectValue,Pos, GetInCompleteIconKey());
        item.MaxCount = MaxCount;
        return item;
    }
}

public class DropItem
{
    public Item Item;
    public double DropRate;

    public DropItem(Item item,double rate)
    {
        Item = item;
        DropRate = rate;
    }
}
