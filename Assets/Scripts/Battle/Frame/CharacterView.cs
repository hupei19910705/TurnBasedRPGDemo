using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

public class CharacterView : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] protected Animator _animator = null;
    [SerializeField] protected Transform _skillEffectPos = null;
    [SerializeField] protected Transform _leftLocate = null;
    [SerializeField] protected Transform _rightLocate = null;
    [SerializeField] protected Transform _topLocate = null;
    [SerializeField] protected Transform _bottomLocate = null;
    [SerializeField] protected Slider _hpSlider = null;
    [SerializeField] protected Slider _mpSlider = null;
    [SerializeField] protected HealAndDamageNumManager _numManager = null;

    public Transform TopLocate { get { return _topLocate; } }
    public Transform BottomLocate { get { return _bottomLocate; } }
    public virtual Transform BackLocate { get { return _leftLocate; } }
    public virtual Transform FrontLocate { get { return _rightLocate; } }

    public Transform SkillEffectPos { get { return _skillEffectPos; } }

    protected const float MOVING_SPEED = 50f;
    protected const string GENERAL_ATTACK_TRIGGER_KEY = "GeneralAttack";
    protected const string SKILL_ATTACK_TRIGGER_KEY = "Skill";
    protected const string BE_HIT_TRIGGER_KEY = "BeHit";

    protected CharacterData _data;
    protected int _pos = -1;
    protected Vector3 _oriPosition;
    protected List<ResultModel> _cacheModels = new List<ResultModel>();
    protected ParallelCoroutines _parallelCor;

    public event Action<int> SelectAction;

    public void SetData(CharacterData data,int pos)
    {
        gameObject.SetActive(true);
        _data = data;
        _pos = pos;
        _oriPosition = transform.position;
        _numManager.Init(_skillEffectPos);
        ChangeHpSliderValue();
        ChangeMpSliderValue();
        if(_parallelCor == null)
            _parallelCor = new ParallelCoroutines();
        _parallelCor.Clear();
        StartCoroutine(_parallelCor.Execute());
    }

    public void PlayBeHitAni()
    {
        _animator.SetTrigger(BE_HIT_TRIGGER_KEY);
    }

    #region Status
    public void ChangeHpSliderValue()
    {
        if (_hpSlider == null)
            return;

        var value = (float)_data.CurrentHp / _data.MaxHp * 100f;
        _hpSlider.value = value;
        _CheckAlive();
    }

    public void ChangeMpSliderValue()
    {
        if (_mpSlider == null)
            return;

        var value = (float)_data.CurrentMp / _data.MaxMp * 100f;
        _mpSlider.value = value;
    }

    public void AddNumText(List<ResultModel> models)
    {
        _cacheModels.AddRange(models);
    }

    public void ShowNumText()
    {
        if (_cacheModels == null || _cacheModels.Count == 0)
            return;

        _numManager.Show(_cacheModels);
        _cacheModels.Clear();
    }

    protected virtual void _CheckAlive()
    {
        if (!_data.IsAlive)
            _parallelCor.Add(_DestroyView(1f));
    }

    protected IEnumerator _DestroyView(float time)
    {
        yield return MyCoroutine.Sleep(time);
        gameObject.SetActive(false);
    }
    #endregion

    #region Pointer Event
    public void OnPointerDown(PointerEventData eventData)
    {
        _Select();
    }

    protected virtual void _Select()
    {
        if (SelectAction != null && _pos != -1)
            SelectAction(_pos);
    }
    #endregion

    #region Attack
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
        transform.localPosition = Vector3.zero;
    }

    protected IEnumerator _MoveToTarget(Vector3 target)
    {
        var speed = (target - transform.position).normalized * MOVING_SPEED;
        while (Mathf.Abs((target - transform.position).x) > Mathf.Abs(speed.x))
        {
            transform.position += speed;
            yield return null;
        }
        transform.position = target;
    }
    #endregion

    private void OnDisable()
    {
        if(_parallelCor != null)
            _parallelCor.Stop();
    }
}
