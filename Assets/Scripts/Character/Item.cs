﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    RedPotion,
    BluePotion,
    Other
}

public class Item : IUseData
{
    public ItemType Type;
    public string ID;
    public int Count;
    public string Name;
    public int EffectValue;
    public int Pos;
    public string IconKey;
    public string Desc;
    private string _inCompleteIconKey = string.Empty;

    public int MaxCount = 99;

    public bool IsFull { get { return Count >= MaxCount; } }

    public Item(ItemRow itemRow, int pos = -1, int count = -99)
    {
        ID = itemRow.ID;
        Type = itemRow.Type;
        Name = itemRow.Name;
        EffectValue = itemRow.EffectValue;
        IconKey = _GetIconKeyPrefixByItemType(Type) + itemRow.IconKey;
        Count = count == -99 ? 1 : count;
        Pos = pos;
        Desc = _GetDescription();
        _inCompleteIconKey = itemRow.IconKey;
    }

    public Item(ItemType type,string id,int count,string name,int effectValue,int pos,string iconKey)
    {
        Type = type;
        ID = id;
        Count = count == -99 ? 1 : count;
        Name = name;
        EffectValue = effectValue;
        Pos = pos;
        IconKey = _GetIconKeyPrefixByItemType(Type) + iconKey;
        Desc = _GetDescription();
        _inCompleteIconKey = iconKey;
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
            case ItemType.RedPotion:
            case ItemType.BluePotion:
                return "Texture/Icons/Potion/";
            case ItemType.Other:
                return "Texture/Icons/Other/";
        }
        return "Texture/Icons/Other/";
    }

    private string _GetDescription()
    {
        var effectTarget = string.Empty;
        switch(Type)
        {
            case ItemType.RedPotion:
                effectTarget = "HP";
                break;
            case ItemType.BluePotion:
                effectTarget = "MP";
                break;
            default:
                return Name;
        }
        var effectValue = EffectValue.ToString();
        if (EffectValue > 0)
            effectValue = "+" + EffectValue.ToString();
        return string.Format("{0}\n{1} {2}", Name, effectTarget, effectValue);
    }

    public static void ReplaceItem(Item item1,Item item2)
    {
        var item = item1;
        item1 = item2;
        item2 = item;
    }

    public int AddNum(int num)
    {
        int total = Count + num;
        if(total >= MaxCount)
        {
            Count = MaxCount;
            return total - MaxCount;
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
        {
            Count = 0;
            return;
        }

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