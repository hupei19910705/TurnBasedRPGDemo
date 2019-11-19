using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNamePopUpPanel : MonoBehaviour
{
    [SerializeField] private InputField _inputField = null;
    [SerializeField] private Button _confirmBtn = null;
    [SerializeField] private Button _cancelBtn = null;

    public event Action<string> OnChangeRecordNameAction;

    public void Show()
    {
        gameObject.SetActive(true);
        _inputField.text = string.Empty;
        _confirmBtn.onClick.AddListener(_OnConfirm);
        _cancelBtn.onClick.AddListener(_OnCancel);
    }

    private void _OnConfirm()
    {
        if (OnChangeRecordNameAction != null && !string.IsNullOrEmpty(_inputField.text))
            OnChangeRecordNameAction(_inputField.text);
        gameObject.SetActive(false);
    }

    private void _OnCancel()
    {
        gameObject.SetActive(false);
    }
}
