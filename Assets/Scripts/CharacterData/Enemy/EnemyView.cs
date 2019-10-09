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

    public event Action<int> SelectEnemyView;

    private const string ENEMY_HIT_TRIGGER_KEY = "BeHit";
    private const float MOVING_SPEED = 50f;
    private const string GENERAL_ATTACK_TRIGGER_KEY = "GeneralAttack";
    private const string SKILL_ATTACK_TRIGGER_KEY = "Skill";

    private EnemyData _enemyData;
    private Vector3 _oriPosition;

    public void SetData(EnemyData data)
    {
        _enemyData = data;
        _oriPosition = _root.position;
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
        if (SelectEnemyView != null && _enemyData.IsAlive)
            SelectEnemyView(_enemyData.Pos);
    }

    public IEnumerator AttackAni(Vector3 target, bool isRemote, bool isSkill = false)
    {
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            yield return null;

        if (!isRemote)
            yield return _MoveToTarget(target);

        var trigger = isSkill ? SKILL_ATTACK_TRIGGER_KEY : GENERAL_ATTACK_TRIGGER_KEY;
        _animator.SetTrigger(trigger);
    }

    public IEnumerator BackToOriPosition()
    {
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            yield return null;

        yield return _MoveToTarget(_oriPosition);
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
}
