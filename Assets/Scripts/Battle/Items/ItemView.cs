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

    public event Action ClickAction;
    public int Pos { get; private set; }

    public void SetData(Item item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }
            
        Pos = item.Pos;
        _image.sprite = Resources.Load<Sprite>(item.IconKey);
        _count.text = string.Format("x{0}", item.Count);
        _selectImage.enabled = false;
        _meetImage.enabled = false;
    }

    public void HideSelectImage()
    {
        _selectImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ClickAction != null)
            ClickAction();
        _selectImage.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _meetImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _meetImage.enabled = false;
    }
}
