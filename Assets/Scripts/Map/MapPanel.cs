using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class MapPanel : AsyncLoadingScenePanel
{ 
    [SerializeField] private Button _battleBtn = null;

    private bool _leave = false;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public override IEnumerator Enter()
    {
        _Register();

        while (!_leave)
            yield return null;
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
        _leave = true;
    }
}
