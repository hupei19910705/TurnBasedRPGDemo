using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordElementView : MonoBehaviour,IDisposable
{
    [SerializeField] private Toggle _toggle = null;
    [SerializeField] private Text _name = null;
    [SerializeField] private Text _time = null;
    [SerializeField] private Text _heroCount = null;
    [SerializeField] private Text _itemCount = null;

    public event Action<int, bool> OnSelectRecordElement;

    private int _recordId = -1;

    public void SetData(GameRecord record,ToggleGroup toggleGroup)
    {
        _recordId = int.Parse(record.RecordID);
        _name.text = record.RecordName;
        _time.text = record.UpdateTime;
        _heroCount.text = record.HeroRecord.Count.ToString();
        _itemCount.text = record.ItemRecord.Count.ToString();
        _toggle.group = toggleGroup;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(_SelectElement);
        _toggle.isOn = false;
    }

    public void ChangeToggleStatus(bool select)
    {
        _toggle.isOn = select;
    }

    private void _SelectElement(bool select)
    {
        if (OnSelectRecordElement != null)
            OnSelectRecordElement(_recordId, select);
    }

    public void Dispose()
    {
        _recordId = -1;
        _name.text = string.Empty;
        _time.text = string.Empty;
        _heroCount.text = string.Empty;
        _itemCount.text = string.Empty;
        _toggle.group = null;
        _toggle.onValueChanged.RemoveAllListeners();
    }
}
