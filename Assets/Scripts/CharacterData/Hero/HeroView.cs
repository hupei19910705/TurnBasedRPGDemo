using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeroView : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Transform _skillEffectPos = null;
    [SerializeField] private Transform _leftLocate = null;
    [SerializeField] private Transform _rightLocate = null;
    [SerializeField] private Transform _topLocate = null;
    [SerializeField] private Transform _bottomLocate = null;
    [SerializeField] private Slider _hpSlider = null;
    [SerializeField] private Slider _mpSlider = null;
    [SerializeField] private Transform _root = null;

    public Transform LeftLocate { get { return _leftLocate; } }
    public Transform FrontLocate { get { return _rightLocate; } }
    public Transform TopLocate { get { return _topLocate; } }
    public Transform BottomLocate { get { return _bottomLocate; } }
    public Transform SkillEffectPos { get { return _skillEffectPos; } }

    public event Action<double> HeroDamageEffect;
    public event Action<int> HeroEndTurn;
    public event Action<int> SetTargetHero;

    private const float MOVING_SPEED = 50f;
    private const string GENERAL_ATTACK_TRIGGER_KEY = "GeneralAttack";
    private const string MEMBER_HIT_TRIGGER_KEY = "BeHit";

    private bool _isAttacking = false;
    private HeroData _heroData;
    private double _attackMultiple = 1;

    public void SetData(HeroData data)
    {
        _heroData = data;
        ChangeHpSliderValue();
        ChangeMpSliderValue();
    }

    public void ChangeHpSliderValue()
    {
        var value = (float)(_heroData.CurrentHp / _heroData.MaxHp * 100f);
        _hpSlider.value = value;
    }

    public void ChangeMpSliderValue()
    {
        var value = (float)(_heroData.CurrentMp / _heroData.MaxMp * 100f);
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
        if (HeroEndTurn != null)
            HeroEndTurn(_heroData.Pos);
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
        if (HeroDamageEffect != null)
            HeroDamageEffect(_heroData.Attack * _attackMultiple);
    }

    public void BeHit()
    {
        ChangeHpSliderValue();
        ChangeMpSliderValue();
        _animator.SetTrigger(MEMBER_HIT_TRIGGER_KEY);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SetTargetHero != null)
            SetTargetHero(_heroData.Pos);
    }
}
