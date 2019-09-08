using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapPresenter
{
    IEnumerator Run();
}

public class MapPresenter : IMapPresenter
{
    private bool _leave = false;
    private IMapView _view;

    public MapPresenter(IMapView view)
    {
        _view = view;
    }

    public IEnumerator Run()
    {
        _RegisterEvents();
        _view.Enter();
        while (!_leave)
            yield return null;
    }

    private void _RegisterEvents()
    {
        _view.LeaveToBattle += _LeaveToBattle;
    }

    private void _UnRegisterEvents()
    {
        _view.LeaveToBattle -= _LeaveToBattle;
    }

    private void _LeaveToBattle()
    {
        _leave = true;
    }
}
