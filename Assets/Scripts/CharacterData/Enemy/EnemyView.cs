using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class EnemyView : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Transform _skillEffectPos = null;
    [SerializeField] private Slider _hpSlider = null;
    [SerializeField] private Transform _leftLocate = null;
    [SerializeField] private Transform _topLocate = null;
    [SerializeField] private Transform _bottomLocate = null;
    [SerializeField] private Transform _root = null;

    public Transform FrontLocate { get { return _leftLocate; } }
    public Transform TopLocate { get { return _topLocate; } }
    public Transform BottomLocate { get { return _bottomLocate; } }
    public Transform SkillEffectPos { get { return _skillEffectPos; } }

    public event Action<int> SelectEnemy;
    public event Action<double> EnemyDamageEffect;

    private const string ENEMY_HIT_TRIGGER_KEY = "BeHit";
    private const float MOVING_SPEED = 50f;
    private const string GENERAL_ATTACK_TRIGGER_KEY = "GeneralAttack";

    private bool _isAttacking = false;
    private EnemyData _enemyData;
    private double _attackMultiple = 1;

    public void SetData(EnemyData data)
    {
        _enemyData = data;
        ChangeHpSliderValue();
    }

    public void ChangeHpSliderValue()
    {
        var value = (float)(_enemyData.CurrentHp / _enemyData.MaxHp * 100f);
        _hpSlider.value = value;
        _CheckAlive();
    }

    private void _CheckAlive()
    {
        if (!_enemyData.IsAlive)
            _root.gameObject.SetActive(false);
    }

    public void BeHit()
    {
        ChangeHpSliderValue();
        _animator.SetTrigger(ENEMY_HIT_TRIGGER_KEY);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SelectEnemy != null && _enemyData.IsAlive)
            SelectEnemy(_enemyData.Pos);
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
    }

    private IEnumerator _MoveToTarget(Vector3 target)
    {
        var speed = (target - _root.position).normalized * MOVING_SPEED;
        while (Mathf.Abs((target - _root.position).x) > Mathf.Abs(speed.x))
        {
            _root.position += speed;
            yield return null;
        }
        _root.position = target;
    }

    public void AttackFinish()
    {
        _isAttacking = false;
    }

    public void DamageEffect()
    {
        if (EnemyDamageEffect != null)
            EnemyDamageEffect(_enemyData.Attack * _attackMultiple);
    }
}
