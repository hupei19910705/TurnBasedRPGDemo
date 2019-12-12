using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    //None,
    //Hot,
    //Blind,
    //Sleeping,
    //Comatose,
    //Burning,
    //Poisoned,
    //Tearing,
    //Cursed

    None,
    Hot,
    Blind,                  //致盲
    Sleeping,               //沉睡
    Comatose,               //昏迷
    Burning,                //燃烧
    Poisoned,               //中毒
    Tearing,                //撕裂
    Cursed                  //诅咒
}

public class Buff
{
    public string SingleID
    {
        get
        {
            if (_fromCharacter == null)
                return string.Empty;
            return _fromCharacter.UID + ID;
        }
    }

    public bool IsActive { get { return RoundCount > 0; } }

    public string ID { get; private set; }
    public string Name { get; private set; }
    public BuffType Type { get; private set; }
    public string IconKey { get; private set; }
    public List<string> Effects { get; private set; }
    public int RoundCount { get; private set; }
    private CharacterData _fromCharacter;

    List<EffectData> _effectDatas = new List<EffectData>();

    public Buff(BuffRow row, CharacterData impact)
    {
        ID = row.ID;
        Name = row.Name;
        Type = row.Type;
        IconKey = row.IconKey;
        Effects = row.Effects;
        RoundCount = row.RoundCount;
        _fromCharacter = impact;
        _SetEffectDatas(row);
    }

    private void _SetEffectDatas(BuffRow buffRow)
    {
        _effectDatas.Clear();
        var id0 = buffRow.DataId0;
        var id1 = buffRow.DataId1;
        List<string> tempList = new List<string>
        {
            id0,
            id1
        };
        var ids = tempList.FindAll(id => !string.IsNullOrEmpty(id));
        var rows = CharacterUtility.Instance.GetEffectDataRows(ids);
        if (rows == null || rows.Count == 0)
            return;

        if (rows.ContainsKey(id0))
            _effectDatas.Add(new EffectData(rows[id0], buffRow.DataValue0));
        if (rows.ContainsKey(id1))
            _effectDatas.Add(new EffectData(rows[id1], buffRow.DataValue1));
    }

    public List<EffectModel> BuffEffectThenReturnModels()
    {
        if (_effectDatas == null || _effectDatas.Count == 0 || _fromCharacter == null)
            return null;

        List<EffectModel> result = new List<EffectModel>();

        foreach(var data in _effectDatas)
            result.Add(data.CreateEffectModel(_fromCharacter));

        RoundCount--;
        return result;
    }
}