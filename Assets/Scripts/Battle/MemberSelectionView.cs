using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberSelectionView : MonoBehaviour
{
    [SerializeField] private Button _attackBtn = null;
    [SerializeField] private Toggle _showSkillToggle = null;
    [SerializeField] private GameObject _skillIndicator = null;
    [SerializeField] private Toggle _showItemToggle = null;
    [SerializeField] private GameObject _itemIndicator = null;
    [SerializeField] private Button _skipBtn = null;
    [SerializeField] private ToggleGroup _toggleGroup = null;

    public event Action OnGeneralAttack;
    public event Action<bool> ListSkills;
    public event Action<bool> ListItems;
    public event Action OnSkip;

    public void Init()
    {
        _attackBtn.onClick.AddListener(() => OnGeneralAttack?.Invoke());
        _showSkillToggle.onValueChanged.AddListener(_SelectSkillToggle);
        _showItemToggle.onValueChanged.AddListener(_SelectItemToggle);
        _skipBtn.onClick.AddListener(() => OnSkip?.Invoke());

        _showSkillToggle.group = _toggleGroup;
        _showItemToggle.group = _toggleGroup;
    }

    public void Show(bool isShow)
    {
        gameObject.SetActive(isShow);
        _SelectSkillToggle(false);
        _SelectItemToggle(false);
    }

    private void _SelectSkillToggle(bool select)
    {
        _showSkillToggle.isOn = select;
        _skillIndicator.SetActive(select);
        if (ListSkills != null)
            ListSkills(select);
    }

    private void _SelectItemToggle(bool select)
    {
        _showItemToggle.isOn = select;
        _itemIndicator.SetActive(select);
        if (ListItems != null)
            ListItems(select);
    }
}
