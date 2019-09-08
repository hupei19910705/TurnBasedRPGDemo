using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IMapView
{
    void Enter();
    event Action LeaveToBattle;
}
public class MapView : MonoBehaviour, IMapView
{
    [SerializeField] private Button _battleBtn = null;

    public event Action LeaveToBattle;

    public void Enter()
    {
        gameObject.SetActive(true);
        _Register();
    }

    private void _Register()
    {
        _UnRegister();
        _battleBtn.onClick.AddListener(_Leave);
    }

    private void _UnRegister()
    {
        _battleBtn.onClick.RemoveAllListeners();
    }

    private void _Leave()
    {
        if (LeaveToBattle != null)
            LeaveToBattle();
    }
}
