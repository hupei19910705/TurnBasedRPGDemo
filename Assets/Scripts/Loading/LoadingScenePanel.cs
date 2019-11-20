using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class LoadingScenePanel : MonoBehaviour
{
    [SerializeField]
    private Transform _processBar = null;
    [SerializeField]
    private float _startPosX = 0f;
    [SerializeField]
    private float _endPosX = 0f;
    [SerializeField]
    private Text _processText = null;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public IEnumerator Enter(SceneEnum target)
    {
        _processBar.localPosition = new Vector3(_startPosX, _processBar.localPosition.y, _processBar.localPosition.z);
        _processText.text = "0%";

        yield return SceneModel.Instance.LoadSceneAsync(target, new SingleProcessBar(_processBar, _startPosX, _endPosX, _processText));
    }
}


