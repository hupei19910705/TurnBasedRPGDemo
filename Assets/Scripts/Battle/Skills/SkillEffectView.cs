using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SkillEffectView : MonoBehaviour
{
    private Transform _defaultRoot = null;
    private ParallelCoroutines _parallelCor;

    private bool _playFinished = false;
    private bool _isOverTime = false;
    private int _roundCount = 0;
    public bool Ballistic { get; private set; }
    public SkillEffectViewType Type { get; private set; }
    public string ID { get; private set; }

    private Action<SkillEffectView> _onDisableAction;

    public void Init(string id,SkillEffectViewType type, Transform transform,Action<SkillEffectView> disableAction,
        bool isOverTime = false, bool ballistic = false)
    {
        ID = id;
        Type = type;
        _defaultRoot = transform;
        _onDisableAction = disableAction;
        Ballistic = ballistic;
        _isOverTime = isOverTime;
        if (isOverTime)
            _parallelCor = new ParallelCoroutines();
    }

    public void LocateTo(Transform locate)
    {
        transform.SetParent(locate, false);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);
    }

    public IEnumerator PlaySkillAni(Transform target)
    {
        if (Ballistic)
            yield break;

        _playFinished = false;
        LocateTo(target);

        if (_isOverTime)
        {
            if (!_parallelCor.Running)
                StartCoroutine(_parallelCor.Execute());
            _parallelCor.Add(_WateToFinish());
        }
        else
        {
            while (!_playFinished)
                yield return null;
        }
    }

    private IEnumerator _WateToFinish()
    {
        while (_roundCount > 0)
            yield return null;

        PlayFinish();
    }

    public IEnumerator PlaySkillAni(Transform target, float speed)
    {
        if (!Ballistic)
            yield break;

        _playFinished = false;
        yield return _MoveToTarget(target.position, speed);
    }

    private IEnumerator _MoveToTarget(Vector3 target,float moveSpeed)
    {
        gameObject.SetActive(true);
        var speed = (target - transform.position).normalized * moveSpeed;
        while (Mathf.Abs((target - transform.position).x) > Mathf.Abs(speed.x))
        {
            transform.position += speed;
            yield return null;
        }
        transform.position = target;
        PlayFinish();
    }

    public void PlayFinish()
    {
        gameObject.SetActive(false);
        transform.SetParent(_defaultRoot,false);
        transform.localPosition = Vector3.zero;
        _playFinished = true;
        if (_onDisableAction != null)
            _onDisableAction(this);
    }

    public void CopyTo(SkillEffectView target)
    {
        target.Init(ID, Type, _defaultRoot, _onDisableAction, _isOverTime, Ballistic);
    }

    public void SetRoundCount(int count)
    {
        _roundCount = count;
    }

    public bool EndRound(int round = 1)
    {
        _roundCount -= round;
        return _roundCount <= 0;
    }
}