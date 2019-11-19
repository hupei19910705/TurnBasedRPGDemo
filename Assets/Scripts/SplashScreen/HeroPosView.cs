using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroPosView : MonoBehaviour
{
    [SerializeField] private Text _name = null;
    [SerializeField] private Text _level = null;
    [SerializeField] private Toggle _toggle = null;
    [SerializeField] private Image _heroImage = null;

    public event Action<bool, int> OnSelectAction;

    private int _pos;

    public void SetData(int pos,HeroData data)
    {
        if (data == null)
        {
            _Reset();
            return;
        }

        _pos = pos;
        _name.text = data.Name;
        _level.text = "LV" + data.Level.ToString();
        _heroImage.sprite = Resources.Load<Sprite>(data.HeadImageKey);
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.isOn = false;
        _toggle.onValueChanged.AddListener(_Select);
    }

    private void _Select(bool select)
    {
        if (OnSelectAction != null)
            OnSelectAction(select, _pos);
    }

    private void _Reset()
    {
        _pos = -1;
        _name.text = string.Empty;
        _level.text = string.Empty;
        _heroImage.sprite = null;
        _toggle.onValueChanged.RemoveAllListeners();
    }
}
