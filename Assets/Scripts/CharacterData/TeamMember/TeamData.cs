using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamData
{
    public Dictionary<int, TeamMemberData> Members = new Dictionary<int, TeamMemberData>();
    public Dictionary<ItemType, List<Item>> BackPack { get; private set; }

    public TeamData(Dictionary<int, TeamMemberData> members, Dictionary<ItemType, List<Item>> backpack)
    {
        Members = members;
        BackPack = backpack;
    }

    public void AddItems(List<Item> items)
    {
        for (int i = items.Count - 1; i >= 0; i--)
            AddItem(items[i]);
    }

    public void AddItem(Item item)
    {
        if (BackPack.ContainsKey(item.Type))
        {
            foreach (var m in BackPack[item.Type])
            {
                if (string.Equals(m.ID, item.ID))
                {
                    var extra = m.AddNum(item.Count);
                    _AddExistItemNum(item, extra);
                }
            }
        }
        else
        {
            BackPack.Add(item.Type, new List<Item> ());
            _AddExistItemNum(item, item.Count);
        }
    }

    private void _AddExistItemNum(Item item, int count)
    {
        while (count > 0)
        {
            if (count <= item.MaxCount)
            {
                BackPack[item.Type].Add(item.Copy(count));
                break;
            }
            else
            {
                BackPack[item.Type].Add(item.Copy(item.MaxCount));
                count -= item.MaxCount;
            }
                
        }
    }

    public void SplitItem(Item item, int count)
    {
        if (!BackPack.ContainsKey(item.Type) || item.Count < count)
            return;
        item.RemoveNum(count);
        BackPack[item.Type].Add(item.Copy(count));
    }
}
