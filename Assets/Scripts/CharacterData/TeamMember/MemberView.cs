using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemberView : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Transform _skillEffectPos = null;
    [SerializeField] private Transform _leftLocate = null;
    [SerializeField] private Transform _rightLocate = null;
    [SerializeField] private Slider _hpSlider = null;
    [SerializeField] private Slider _mpSlider = null;
    [SerializeField] private Transform _root = null;

    public Transform LeftLocate { get { return _leftLocate; } }
    public Transform FrontLocate { get { return _rightLocate; } }
    public Transform SkillEffectPos { get { return _skillEffectPos; } }

    public event Action<double> MemberDamageEffect;
    public event Action<int> MemberEndTurn;

    private const float MOVING_SPEED = 50f;
    private const string GENERAL_ATTACK_TRIGGER_KEY = "GeneralAttack";
    private const string MEMBER_HIT_TRIGGER_KEY = "BeHit";

    private bool _isAttacking = false;
    private TeamMemberData _memberData;
    private double _attackMultiple = 1;

    public void SetData(TeamMemberData data)
    {
        _memberData = data;
        ChangeHpSliderValue();
        ChangeMpSliderValue();
    }

    public void ChangeHpSliderValue()
    {
        var value = (float)(_memberData.CurrentHp / _memberData.MaxHp * 100f);
        _hpSlider.value = value;
    }

    public void ChangeMpSliderValue()
    {
        var value = (float)(_memberData.CurrentMp / _memberData.MaxMp * 100f);
        _mpSlider.value = value;
    }

    public IEnumerator GeneralAttack(Vector3 target)
    {
        var oriPos = _root.position;

        yield return _MoveToTarget(target);

        _isAttacking = true;
        _animator.SetTrigger(GENERAL_ATTACK_TRIGGER_KEY);
        while (_isAttacking)
            yield return null;

        yield return _MoveToTarget(oriPos);
        _root.localPosition = Vector3.zero;
        if (MemberEndTurn != null)
            MemberEndTurn(_memberData.Pos);
    }

    private IEnumerator _MoveToTarget(Vector3 target)
    {
        var speed = (target - _root.position).normalized * MOVING_SPEED;
        while (Mathf.Abs((target - _root.position).x) > Mathf.Abs(speed.x))
        {
            _root.position += speed;
            yield return null;
        }
    }

    public void AttackFinish()
    {
        _isAttacking = false;
    }

    public void DamageEffect()
    {
        if (MemberDamageEffect != null)
            MemberDamageEffect(_memberData.Attack * _attackMultiple);
    }

    public void BeHit()
    {
        ChangeHpSliderValue();
        ChangeMpSliderValue();
        _animator.SetTrigger(MEMBER_HIT_TRIGGER_KEY);
    }
}
