using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class MemberElement : MonoBehaviour
{
    [SerializeField] private Image _headImage = null;
    [SerializeField] private Text _nameText = null;
    [SerializeField] private Text _jobText = null;
    [SerializeField] private Text _levelText = null;
    [SerializeField] private Text _expText = null;
    [SerializeField] private Text _hpText = null;
    [SerializeField] private Text _mpText = null;
    [SerializeField] private GameObject _indicator = null;
    [SerializeField] private Toggle _toggle = null;

    public event Action<bool,int> SelectMember;

    private TeamMemberData _memberData;

    public void SetData(TeamMemberData data,ToggleGroup toggleGroup)
    {
        _memberData = data;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(SelectElement);
        _toggle.group = toggleGroup;
        SelectElement(false);
        _LoadHeadImage(data.HeadImageKey);
        _nameText.text = data.Name;
        _jobText.text = data.Job.ToString();
        UpdateStatus();
    }

    private void _LoadHeadImage(string path)
    {
        _headImage.sprite = Resources.Load<Sprite>(path);
    }

    public void SelectElement(bool select)
    {
        _indicator.SetActive(select);
        if (SelectMember != null)
            SelectMember(select, _memberData.Pos);
    }

    public void LockToggle(bool isLock)
    {
        _toggle.interactable = !isLock;
        _toggle.isOn = false;
    }

    public void ChangeToggleValue(bool value)
    {
        _toggle.isOn = value;
    }

    public void UpdateStatus()
    {
        _levelText.text = _memberData.Level.ToString();
        _expText.text = string.Format("{0}/{1}", _memberData.Exp, BattleUtility.Instance.GetLevelUpExpByLevel(_memberData.Level));
        _hpText.text = string.Format("{0}/{1}", _memberData.CurrentHp, _memberData.MaxHp);
        _mpText.text = string.Format("{0}/{1}", _memberData.CurrentMp, _memberData.MaxMp);
        LockToggle(!_memberData.IsAlive || _memberData.IsTurnEnd);
    }
}
