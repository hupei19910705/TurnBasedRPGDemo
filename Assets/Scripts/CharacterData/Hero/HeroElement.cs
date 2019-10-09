using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class HeroElement : MonoBehaviour
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

    public event Action<bool,int> SelectHeroElement;

    private HeroData _heroData;

    public void SetData(HeroData data,ToggleGroup toggleGroup)
    {
        _heroData = data;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(SelectElement);
        _toggle.group = toggleGroup;
        SelectElement(false);
        _LoadHeadImage(data.HeadImageKey);
        _nameText.text = data.Name;
        _jobText.text = data.Job.ToString();
        FreshHeroElementStatus();
    }

    private void _LoadHeadImage(string path)
    {
        _headImage.sprite = Resources.Load<Sprite>(path);
    }

    public void SelectElement(bool select)
    {
        _indicator.SetActive(select);
        if (SelectHeroElement != null)
            SelectHeroElement(select, _heroData.Pos);
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

    public void FreshHeroElementStatus()
    {
        _levelText.text = _heroData.Level.ToString();
        _expText.text = string.Format("{0}/{1}", _heroData.Exp, BattleUtility.Instance.GetLevelUpExpByLevel(_heroData.Level));
        _hpText.text = string.Format("{0}/{1}", _heroData.CurrentHp, _heroData.MaxHp);
        _mpText.text = string.Format("{0}/{1}", _heroData.CurrentMp, _heroData.MaxMp);
        var lockToggle = !_heroData.IsAlive || _heroData.IsTurnEnd;
        if(lockToggle)
            LockToggle(true);
    }
}
