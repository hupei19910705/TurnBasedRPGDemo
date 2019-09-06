using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MemberJob
{
    Warrior,
    Wizard
}

public class TeamMemberData
{
    private static int _serial = 0;
    private string _id;
    public string ID
    {
        get { return _id; }
        set
        {
            if (_serial < int.MaxValue)
                _serial++;
            else
                _serial = 0;
            _id = string.Format("member_{0}", _serial.ToString().PadLeft(6));
        }
    }
    public string Name { get; private set; }
    public MemberJob Job { get; private set; }
    public int OriginHp { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxMp { get; private set; }
    public int CurrentMp { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public string HeadImageKey { get; private set; }
    public string DeathImageKey { get; private set; }
    public double Attack { get; private set; }
    public double Defence { get; private set; }
    public Dictionary<SkillType,List<Skill>> Skills { get; private set; }
    public Dictionary<ItemType,List<Item>> BackPack { get; private set; }

    public TeamMemberData(string name,MemberJob job,int hp,int mp,string headImageKey,string deathImageKey,double attack,double defence)
    {
        Name = name;
        Job = job;
        OriginHp = MaxHp = CurrentHp = hp;
        MaxMp = CurrentMp = mp;
        HeadImageKey = headImageKey;
        DeathImageKey = deathImageKey;
        Attack = attack;
        Defence = defence;
    }

    public void AddSkill(Skill skill)
    {
        if(Skills.ContainsKey(skill.Type))
        {
            foreach(var s in Skills[skill.Type])
            {
                if (!string.Equals(s.ID, skill.ID))
                    Skills[skill.Type].Add(skill);
            }
        }
        else
            Skills.Add(skill.Type, new List<Skill> { skill });
    }

    public void AddItem(Item item)
    {
        if(BackPack.ContainsKey(item.Type))
        {
            foreach (var m in BackPack[item.Type])
            {
                if (string.Equals(m.ID, item.ID))
                {
                    var extra = m.AddNum(item.Count);
                    _AddExistItemNum(item,extra);
                }
            }
        }
        else
            _AddExistItemNum(item, item.Count);
    }

    private void _AddExistItemNum(Item item,int count)
    {
        while (count > 0)
        {
            if (count <= item.MaxCount)
                BackPack[item.Type].Add(new Item(item.Type, item.ID, count));
            else
            {
                BackPack[item.Type].Add(new Item(item.Type, item.ID, item.MaxCount));
                count -= item.MaxCount;
            }
        }
    }

    public void SplitItem(Item item,int count)
    {
        if (!BackPack.ContainsKey(item.Type) || item.Count < count)
            return;
        item.RemoveNum(count);
        BackPack[item.Type].Add(new Item(item.Type, item.ID, count));
    }
}
