using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IInfoView
{
    void Show(string name,string subText, string desc, bool useAble = true);
    void EndShow();
    event Action HideArrowAndSelectImage;
    event Action OnUseAction;
}

public class InfoView : MonoBehaviour, IInfoView
{
    [SerializeField] private Text _name = null;
    [SerializeField] private Text _subText = null;
    [SerializeField] private Text _desc = null;
    [SerializeField] private Text _tip = null;
    [SerializeField] private Button _useBtn = null;
    [SerializeField] private Button _cancelBtn = null;

    [SerializeField] private Color _titleColor = default;

    public event Action HideArrowAndSelectImage;
    public event Action OnUseAction;

    public void Show(string name,string subText ,string desc,bool useAble = true)
    {
        gameObject.SetActive(true);

        _name.text = name;
        _subText.text = subText;
        _desc.text = desc;

        _useBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();

        _useBtn.onClick.AddListener(_UseSKill);
        _cancelBtn.onClick.AddListener(EndShow);

        _tip.gameObject.SetActive(!useAble);
        if (!useAble)
        {
            _tip.color = Color.red;
            _tip.text = "无法使用";
            _useBtn.interactable = false;
            _subText.color = Color.red;
        }
        else
        {
            _useBtn.interactable = true;
            _subText.color = _titleColor;
        }
    }

    private void _UseSKill()
    {
        if (OnUseAction != null)
            OnUseAction();

        EndShow();
    }

    public void EndShow()
    {
        gameObject.SetActive(false);
        if (HideArrowAndSelectImage != null)
            HideArrowAndSelectImage();
    }
}
