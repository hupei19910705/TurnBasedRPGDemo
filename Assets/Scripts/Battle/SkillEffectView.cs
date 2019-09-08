using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SkillEffectView : MonoBehaviour
{
    [SerializeField] private Transform _root = null;

    private Transform _defaultRoot = null;

    private const float MOVING_SPEED = 50f;
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    public void Init(Transform transform)
    {
        _defaultRoot = transform;
    }

    public void PlaySkillEffect(Transform target,bool needMove = false)
    {
        if(!needMove)
        {
            _root.SetParent(target,false);
            _root.localPosition = Vector3.zero;
            _root.gameObject.SetActive(true);
        }
        else
        {
            _parallelCor.Add(_MoveToTarget(target.position));
            StartCoroutine(_parallelCor.Execute());
        }
    }

    private IEnumerator _MoveToTarget(Vector3 target)
    {
        _root.gameObject.SetActive(true);
        var speed = (target - _root.position).normalized * MOVING_SPEED;
        while (Mathf.Abs((target - _root.position).x) > Mathf.Abs(speed.x))
        {
            _root.position += speed;
            yield return null;
        }
        _root.position = target;
        PlayFinish();
        StopCoroutine(_parallelCor.Execute());
    }

    public void PlayFinish()
    {
        _root.gameObject.SetActive(false);
        _root.SetParent(_defaultRoot,false);
        _root.localPosition = Vector3.zero;
    }
}
