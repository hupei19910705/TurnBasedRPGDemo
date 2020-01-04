using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _image = null;
    [SerializeField] private Text _count = null;
    [SerializeField] private Image _selectImage = null;
    [SerializeField] private Image _meetImage = null;

    public event Action<int> ClickAction;
    public int Pos { get; private set; }
    private bool _isLockPointer = false;

    public void SetData(Item item, int pos = -1)
    {
        if (item == null)
        {
            _image.gameObject.SetActive(false);
            _count.gameObject.SetActive(false);
            _image.sprite = null;
        }
        else
        {
            _image.gameObject.SetActive(true);
            _count.gameObject.SetActive(true);
            _image.sprite = Resources.Load<Sprite>(item.IconKey);
            _count.text = string.Format("x{0}", item.Count);
        }

        Pos = pos;
        _selectImage.enabled = false;
        _meetImage.enabled = false;
        _isLockPointer = false;
    }

    public void HideSelectImage()
    {
        _selectImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isLockPointer)
            return;

        if (ClickAction != null)
        {
            ClickAction(Pos);
            _selectImage.enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isLockPointer)
            return;

        _meetImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isLockPointer)
            return;

        _meetImage.enabled = false;
    }

    public void LockPointer(bool isLock)
    {
        _isLockPointer = isLock;
    }
}
