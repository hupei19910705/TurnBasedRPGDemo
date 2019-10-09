using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SkillEffectView : MonoBehaviour
{
    [SerializeField] private Transform _root = null;

    private Transform _defaultRoot = null;

    private bool _playFinished = false;
    public bool Ballistic { get; private set; }

    public void Init(Transform transform,bool ballistic = false)
    {
        _defaultRoot = transform;
        Ballistic = ballistic;
    }

    public void LocateTo(Transform locate)
    {
        _root.SetParent(locate, false);
        _root.localPosition = Vector3.zero;
        _root.gameObject.SetActive(true);
    }

    public IEnumerator PlaySkillAni(Transform target)
    {
        if (Ballistic)
            yield break;

        _playFinished = false;
        LocateTo(target);

        while (!_playFinished)
            yield return null;
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
        _root.gameObject.SetActive(true);
        var speed = (target - _root.position).normalized * moveSpeed;
        while (Mathf.Abs((target - _root.position).x) > Mathf.Abs(speed.x))
        {
            _root.position += speed;
            yield return null;
        }
        _root.position = target;
        PlayFinish();
    }

    public void PlayFinish()
    {
        _root.gameObject.SetActive(false);
        _root.SetParent(_defaultRoot,false);
        _root.localPosition = Vector3.zero;
        _playFinished = true;
    }
}
