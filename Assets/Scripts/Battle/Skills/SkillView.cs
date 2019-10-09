using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _image = null;
    [SerializeField] private Image _selectImage = null;
    [SerializeField] private Image _meetImage = null;
    [SerializeField] private Image _unUseAbleImage = null;

    public event Action ClickAction;
    public string ID { get; private set; }

    public void SetData(Skill skill,bool useAble = true)
    {
        ID = skill.ID;
        _image.sprite = Resources.Load<Sprite>(skill.ImageKey);
        _selectImage.enabled = false;
        _meetImage.enabled = false;
        _unUseAbleImage.enabled = !useAble;
    }

    public void HideSelectImage()
    {
        _selectImage.enabled = false;
    }

    public void SetSkillViewUseAble(bool useAble)
    {
        _unUseAbleImage.enabled = !useAble;
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
