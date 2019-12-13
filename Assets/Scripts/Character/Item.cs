using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    RedPotion,
    BluePotion,
    Other
}

public enum ItemTargetType
{
    Self,
    Opposite,
    Both
}

public class Item : IUseData
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public ItemType Type { get; private set; }
    public ItemTargetType TargetType { get; private set; }
    private string _oriIconKey;
    public string IconKey { get; private set; }
    public string Desc { get; private set; }
    public int MaxCount { get; private set; }
    public int Count { get; private set; }
    public EffectData _effectData;
    public List<string> _buffs;

    public bool IsFull { get { return Count >= MaxCount; } }
    public bool CanUseToSelf { get { return TargetType == ItemTargetType.Both || TargetType == ItemTargetType.Self; } }
    public bool CanUseToOpposite { get { return TargetType == ItemTargetType.Both || TargetType == ItemTargetType.Opposite; } }

    public Item(ItemRow itemRow, int count = -99)
    {
        ID = itemRow.ID;
        Name = itemRow.Name;
        Type = itemRow.Type;
        TargetType = itemRow.TargetType;
        _oriIconKey = itemRow.IconKey;
        IconKey = _GetIconKeyPrefixByItemType(Type) + _oriIconKey;
        Desc = itemRow.Desc;
        MaxCount = itemRow.MaxCount;
        Count = count;
        _SetEffectData(itemRow.UseToDataId, itemRow.UseToDataValue);
        _buffs = itemRow.UseToBuffIds;
    }

    public Item(string id, string name, ItemType type, ItemTargetType targetType, string iconKey,string desc,
        int maxCount, int count, EffectData effectData,List<string> buffs)
    {
        ID = id;
        Name = name;
        Type = type;
        TargetType = targetType;
        _oriIconKey = iconKey;
        IconKey = _GetIconKeyPrefixByItemType(Type) + _oriIconKey;
        Desc = desc;
        MaxCount = maxCount;
        Count = count;
        _effectData = effectData;
        _buffs = buffs;
    }

    private void _SetEffectData(string id,float value)
    {
        var row = CharacterUtility.Instance.GetEffectDataRow(id);

        if (row == null)
            return;

        _effectData = new EffectData(row, value);
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
        return new Item(ID, Name, Type, TargetType, _oriIconKey, Desc, MaxCount, count, _effectData, _buffs);
    }

    public EffectModel GetImmediatelyEffectModel(CharacterData from = null)
    {
        return _effectData.CreateEffectModel(from);
    }

    public List<Buff> GetBuffs(CharacterData from = null)
    {
        List<BuffRow> buffRows = GetBuffRows();

        if (buffRows == null || buffRows.Count == 0)
            return null;

        List<Buff> result = new List<Buff>();
        foreach (var row in buffRows)
        {
            var buff = new Buff(row, from);
            result.Add(buff);
        }

        return result;
    }

    public List<BuffRow> GetBuffRows()
    {
        return CharacterUtility.Instance.GetBuffRows(_buffs);
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
