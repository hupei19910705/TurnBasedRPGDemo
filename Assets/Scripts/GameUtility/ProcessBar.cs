using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ProcessBar : MonoBehaviour,IDisposable
{
    [SerializeField] private Transform _processBar = null;
    [SerializeField] private float _speed = 0f;
    [SerializeField] private MoveDirection _direction = MoveDirection.Horizontal;

    private enum MoveDirection
    {
        Horizontal,
        Vertical
    }

    public event Action FullProcessBarAction;

    private const float END_POS = 0;

    private float _startPos;

    private ParallelCoroutines _parallelCor;

    public void Init()
    {
        Dispose();

        switch (_direction)
        {
            case MoveDirection.Horizontal:
                _startPos = -_processBar.GetComponent<RectTransform>().sizeDelta.x;
                break;
            case MoveDirection.Vertical:
                _startPos = -_processBar.GetComponent<RectTransform>().sizeDelta.y;
                break;
        }

        if (_speed < 0f)
            _speed = -_speed;

        _UpdateBar(0f);
        _parallelCor = new ParallelCoroutines();
        StartCoroutine(_parallelCor.Execute());
    }

    public void MoveToTargetRate(float startRate,float targetRate,bool ignoreProcess = false)
    {
        if (!_parallelCor.Finished)
            _parallelCor.Clear();

        _UpdateBar(startRate);
        _parallelCor.Add(_MoveToTargetRate(startRate, targetRate, ignoreProcess));
    }

    public void ContinuousMoveToTargetRate(float startRate, float targetRate, bool ignoreProcess = false)
    {
        if (!_parallelCor.Finished)
            _parallelCor.Clear();

        _parallelCor.Add(_ContinuousUpdateBar(startRate, targetRate, ignoreProcess));
    }

    private IEnumerator _ContinuousUpdateBar(float startRate, float targetRate, bool ignoreProcess = false)
    {
        _UpdateBar(startRate);
        while (targetRate > 100f)
        {
            targetRate -= 100f;
            yield return _MoveToTargetRate(startRate, 100f, ignoreProcess);
            if (FullProcessBarAction != null)
                FullProcessBarAction();
            startRate = 0f;
            _UpdateBar(startRate);
        }
        yield return _MoveToTargetRate(startRate, targetRate, ignoreProcess);
    }

    private IEnumerator _MoveToTargetRate(float startRate, float targetRate, bool ignoreProcess = false)
    {
        targetRate = Mathf.Clamp(targetRate, 0f, 100f);
        targetRate = (float)Math.Round(targetRate, 2, MidpointRounding.AwayFromZero);
        
        if(!ignoreProcess)
        {
            var offset = targetRate > startRate ? _speed : -_speed;
            while (Mathf.Abs(targetRate - startRate) >= _speed)
            {
                startRate += offset;
                _UpdateBar(startRate);
                yield return null;
            }
        }
        
        _UpdateBar(targetRate);
    }

    private void _UpdateBar(float targetRate)
    {
        var point = Mathf.Lerp(_startPos, END_POS, targetRate / 100f);

        switch (_direction)
        {
            case MoveDirection.Horizontal:
                _processBar.localPosition = new Vector3(point, _processBar.localPosition.y, _processBar.localPosition.z);
                break;
            case MoveDirection.Vertical:
                _processBar.localPosition = new Vector3(_processBar.localPosition.x, point, _processBar.localPosition.z);
                break;
        }
    }

    public void Dispose()
    {
        if(_parallelCor != null)
        {
            _parallelCor.Stop();
            _parallelCor = null;
        }
    }
}
