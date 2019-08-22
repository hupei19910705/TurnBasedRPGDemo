using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;
using Utility.GameUtility;

public class SplashScreenView : MonoBehaviour
{
    [SerializeField]
    private Button _enterGameBtn = null;

    private bool _leave = false;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public IEnumerator Run()
    {
        _leave = false;
        _Register();
        while (!_leave)
            yield return null;
    }

    private void _Register()
    {
        _UnRegister();
        _enterGameBtn.onClick.AddListener(_Leave);
    }

    private void _UnRegister()
    {
        _enterGameBtn.onClick.RemoveAllListeners();
    }

    private void _Leave()
    {
        _leave = true;
    }
}
