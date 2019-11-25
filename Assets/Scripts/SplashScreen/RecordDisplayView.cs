using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordDisplayView : MonoBehaviour
{
    [SerializeField] private Text _title = null;
    [SerializeField] private HeroPosView[] _teamHeroViews = null;
    [SerializeField] private HeroDisplayView _heroDisplay = null;
    [SerializeField] private ChangeNamePopUpPanel _changeRecordNameView = null;

    [Header("Button")]
    [SerializeField] private Button _changeTitleBtn = null;
    [SerializeField] private Button _removeBtn = null;

    public event Action<int> OnRemoveRecordAction;
    public event Action<int,string> OnChangeRecordNameAction;

    private GameData _gameData;
    private HeroData[] _heroDatas;

    private int _recordId = -1;

    public void Init(GameData gameData)
    {
        _gameData = gameData;
    }

    public void Show(GameRecord record)
    {
        _recordId = record.RecordID;
        _title.text = record.RecordName;
        _InitHeroDatas(record.TeamRecord, record.HeroRecord);
        _SetTeamHeroes();
        _HideHeroDisplay();
        _Register();
        
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void _Register()
    {
        _UnRegister();

        _changeRecordNameView.OnChangeRecordNameAction += _OnChangeRecordName;

        _changeTitleBtn.onClick.AddListener(_ChangeRecordName);
        _removeBtn.onClick.AddListener(_RemoveRecord);
    }

    private void _UnRegister()
    {
        _changeRecordNameView.OnChangeRecordNameAction -= _OnChangeRecordName;

        _changeTitleBtn.onClick.RemoveAllListeners();
        _removeBtn.onClick.RemoveAllListeners();
    }

    private void _InitHeroDatas(Dictionary<int,string> teamHeroes,Dictionary<string,HeroRecordData> heroRecords)
    {
        _heroDatas = new HeroData[_teamHeroViews.Length];

        foreach(var pair in teamHeroes)
        {
            if (pair.Key < 0)
                continue;

            var heroRecord = heroRecords[pair.Value];
            var heroRow = _gameData.HeroTable[heroRecord.ID];
            var heroJob = _gameData.HeroJobTable[heroRow.Job];
            var skills = CharacterUtility.Instance.GetUnLockHeroSkills(heroRow.Job, heroRecord.Level);

            var heroData = new HeroData(heroRecord.UID, heroRow, heroJob, heroRecord.Exp, heroRecord.Level, skills);
            _heroDatas[pair.Key] = heroData;
        }
    }

    private void _SetTeamHeroes()
    {
        for (int i = 0;i<_heroDatas.Length;i++)
        {
            var heroView = _teamHeroViews[i];
            heroView.OnSelectAction -= _OnSelectTeamHero;
            heroView.OnSelectAction += _OnSelectTeamHero;
            heroView.SetData(i, _heroDatas[i]);
        }
    }

    private void _OnSelectTeamHero(bool select,int pos)
    {
        if (select)
            _ShowHeroDisplay(pos);
        else
            _HideHeroDisplay();
    }

    private void _ShowHeroDisplay(int pos)
    {
        var heroData = _heroDatas[pos];
        if (heroData == null)
            return;

        var maxExp = _gameData.LevelExpTable[heroData.Level];
        _heroDisplay.SetData(pos, maxExp, heroData);
        _heroDisplay.gameObject.SetActive(true);
    }

    private void _HideHeroDisplay()
    {
        _heroDisplay.gameObject.SetActive(false);
    }

    private void _RemoveRecord()
    {
        if (OnRemoveRecordAction != null && _recordId >=0)
            OnRemoveRecordAction(_recordId);

        Hide();
    }

    private void _ChangeRecordName()
    {
        _changeRecordNameView.Show();
    }

    private void _OnChangeRecordName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        if (OnChangeRecordNameAction != null)
            OnChangeRecordNameAction(_recordId, name);

        _title.text = name;
    }
}
