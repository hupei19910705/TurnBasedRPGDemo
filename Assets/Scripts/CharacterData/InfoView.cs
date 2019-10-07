using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IInfoView
{
    void Show(string name, int count, string desc, int pos, Action useAction);
    void EndShow();
    event Action HideArrowAndSelectImage;
    int Pos { get; }
}

public class InfoView : MonoBehaviour, IInfoView
{
    [SerializeField] private Text _name = null;
    [SerializeField] private Text _count = null;
    [SerializeField] private Text _desc = null;
    [SerializeField] private Button _useBtn = null;
    [SerializeField] private Button _cancelBtn = null;

    public event Action HideArrowAndSelectImage;
    public int Pos { get; private set; } = -1;

    public void Show(string name,int count ,string desc,int pos, Action useAction)
    {
        gameObject.SetActive(true);

        _name.text = name;
        _count.gameObject.SetActive(count > 0);
        _count.text = "数量:" + count;
        _desc.text = desc;
        Pos = pos;

        _useBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();
        
        _useBtn.onClick.AddListener(() =>
        {
            if (useAction != null)
                useAction();
            EndShow();
        });
        _cancelBtn.onClick.AddListener(EndShow);
    }

    public void EndShow()
    {
        gameObject.SetActive(false);
        if (HideArrowAndSelectImage != null)
            HideArrowAndSelectImage();
    }
}
