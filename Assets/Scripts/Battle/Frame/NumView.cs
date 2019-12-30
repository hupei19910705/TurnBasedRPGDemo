using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NumTextType
{
    Heal,
    PhysicalDamage,
    MagicDamage,
    RealDamage,
    MpChange
}

public enum NumMoveDir
{
    Up,
    Down,
    Behind
}

public class NumView : MonoBehaviour
{
    [SerializeField] private Text _numText = null;
    [SerializeField] private Color _healColor = default;
    [SerializeField] private Color _physicalColor = default;
    [SerializeField] private Color _magicColor = default;
    [SerializeField] private Color _realColor = default;
    [SerializeField] private Color _mpColor = default;

    public event Action<GameObject> ReturnObject;

    private Vector3 _scale;
    public IEnumerator Show(ResultModel model,Vector3 scale)
    {
        if (model.ValueType == ResultValueType.None)
            yield break;

        _scale = scale;
        var numType = _GetNumType(model);
        _numText.color = _GetNumColor(numType);
        yield return _MoveTo(model);
        if (ReturnObject != null)
            ReturnObject(gameObject);
    }

    private NumTextType _GetNumType(ResultModel model)
    {
        switch (model.ValueType)
        {
            case ResultValueType.Mp:
                _numText.color = _mpColor;
                return NumTextType.MpChange;
            case ResultValueType.Hp:
                if (!model.IsReduce && model.ChangeValue >= 0)
                    return NumTextType.Heal;
                return _GetNumTypeByEffectWay(model.EffectWay); 
        }

        return NumTextType.RealDamage;
    }

    private NumTextType _GetNumTypeByEffectWay(ValueEffectWay effectWay)
    {
        switch(effectWay)
        {
            case ValueEffectWay.Phisical:
                return NumTextType.PhysicalDamage;
            case ValueEffectWay.Magic:
                return NumTextType.MagicDamage;
            case ValueEffectWay.Real:
                return NumTextType.RealDamage;
        }

        return NumTextType.RealDamage;
    }

    private Color _GetNumColor(NumTextType type)
    {
        switch(type)
        {
            case NumTextType.Heal:
                return _healColor;
            case NumTextType.PhysicalDamage:
                return _physicalColor;
            case NumTextType.MagicDamage:
                return _magicColor;
            case NumTextType.RealDamage:
                return _realColor;
            case NumTextType.MpChange:
                return _mpColor;
        }

        return _physicalColor;
    }

    private IEnumerator _MoveTo(ResultModel model)
    {
        if (!model.IsReduce && model.ChangeValue >= 0)
        {
            _numText.text = "+" + Mathf.Abs(model.ChangeValue);
            yield return _MoveTo(NumMoveDir.Up);
        }
        else if (model.IsReduce && model.ChangeValue <= 0)
        {
            _numText.text = "-" + Mathf.Abs(model.ChangeValue);
            switch (model.ValueType)
            {
                case ResultValueType.Hp:
                    yield return _MoveTo(NumMoveDir.Behind);
                    break;
                case ResultValueType.Mp:
                    yield return _MoveTo(NumMoveDir.Down);
                    break;
            }
        }
    }
    private IEnumerator _MoveTo(NumMoveDir dir)
    {
        Vector3 vec = _GetVecByDirectionAndScale(dir);

        float time = 0f;
        while (time < 2f)
        {
            time += Time.deltaTime;
            transform.position += vec;
            yield return null;
        }
    }

    private Vector3 _GetVecByDirectionAndScale(NumMoveDir dir)
    {
        switch (dir)
        {
            case NumMoveDir.Up:
                return Vector3.up * _scale.y;
            case NumMoveDir.Down:
                return Vector3.down * _scale.y;
            case NumMoveDir.Behind:
                return new Vector3(-1f * _scale.x, 1f * _scale.y, 0f).normalized;
        }

        return Vector3.zero;
    }
}