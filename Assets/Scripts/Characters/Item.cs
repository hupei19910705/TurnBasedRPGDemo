using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{

}

public class Item
{
    public ItemType Type { get; private set; }
    public string ID { get; private set; }
    public int Count { get; private set; }

    public int MaxCount = 99;

    public Item(ItemType type,string id,int count)
    {
        Type = type;
        ID = id;
        Count = count;
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
}
