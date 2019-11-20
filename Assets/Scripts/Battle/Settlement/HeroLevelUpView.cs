using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroLevelUpView : MonoBehaviour
{
    [SerializeField] private Text _name = null;
    [SerializeField] private Image _heroImage = null;
    [SerializeField] private Text _level = null;
    [SerializeField] private ProcessBar _expBar = null;
    [SerializeField] private GameObject _levelUpTip = null;

    private HeroLevelExpData _levelExpData;
    private int _curLevel;

    public void Show(string name,string imageKey,HeroLevelExpData levelExpData)
    {
        _name.text = name;
        _heroImage.sprite = Resources.Load<Sprite>(imageKey);
        _levelExpData = levelExpData;
        _curLevel = _levelExpData.OldLevel;
        _level.text = _curLevel.ToString();
        _levelUpTip.SetActive(false);

        _expBar.Init();
        _UpdateExpBar();
    }

    private void _UpdateExpBar()
    {
        float startRate = _levelExpData.OldExpRate;
        float targetRate = (_levelExpData.NewLevel - _levelExpData.OldLevel) * 100f + _levelExpData.NewExpRate;

        _expBar.FullProcessBarAction -= _LevelUp;
        _expBar.FullProcessBarAction += _LevelUp;

        _expBar.ContinuousMoveToTargetRate(startRate, targetRate);
    }

    private void _LevelUp()
    {
        _curLevel++;
        _level.text = _curLevel.ToString();
        _levelUpTip.SetActive(true);
    }
}
