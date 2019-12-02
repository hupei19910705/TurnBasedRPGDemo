using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    NoValueChange,
    HpChange,
    MpChange,
    HpMpChange
}

public enum SpecialBuffType
{
    None,
    Blind
}

public class Buff
{
    public string From { get; private set; }
    public string ID { get; private set; }
    private int _effectValue;
    private int _duration;
    private BuffType _type;
    private SpecialBuffType _sType;
    private EffectType _effectType;

    public bool IsActive { get { return _duration > 0; } }

    public Buff(string from,int effectValue,int duration,BuffRow row)
    {
        From = from;
        ID = row.ID;

        _effectValue = effectValue;
        _duration = duration;
        _type = row.Type;
        _sType = row.SType;
        _effectType = row.EffectType;
    }

    public int TakeEffect(int def)
    {
        _duration--;
        var changeValue = _effectValue;
        switch(_effectType)
        {
            case EffectType.Physical:
            case EffectType.Magic:
                changeValue -= def;
                break;
            case EffectType.Real:
                break;
            case EffectType.Mp:
                break;
        }

        return changeValue;

        //switch (_type)
        //{
        //    case BuffType.NoValueChange:
        //        break;
        //    case BuffType.HpChange:
        //        break;
        //    case BuffType.MpChange:
        //        break;
        //    case BuffType.HpMpChange:
        //        break;
        //}
    }
}