using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class ResultPopupPanel : MonoBehaviour
{
    [SerializeField] private Text _title = null;
    [SerializeField] private GeneralObjectPool _heroPool = null;
    [SerializeField] private Transform _heroAnchor = null;
    [SerializeField] private GeneralObjectPool _itemPool = null;
    [SerializeField] private Transform _itemAnchor = null;
    [SerializeField] private Button _confirmBtn = null;

    public void Show(bool win,Dictionary<string,HeroData> heroes, Dictionary<string, HeroLevelExpData> levelExpDatas,
        List<Item> items,Action confirmCallBack)
    {
        gameObject.SetActive(true);
        _title.text = win ? "胜利" : "失败";
        _CreateHeroes(heroes, levelExpDatas);
        _CreateItems(items);

        _confirmBtn.onClick.RemoveAllListeners();
        _confirmBtn.onClick.AddListener(() =>
        {
            if (confirmCallBack != null)
                confirmCallBack();
            gameObject.SetActive(false);
        });
    }

    private void _CreateHeroes(Dictionary<string, HeroData> heroes, Dictionary<string, HeroLevelExpData> levelExpDatas)
    {
        _heroPool.InitPool();

        foreach (var pair in levelExpDatas)
        {
            var obj = _heroPool.GetInstance();
            var trans = obj.GetComponent<Transform>();
            trans.SetParent(_heroAnchor);

            var heroName = heroes[pair.Key].Name;
            var heroImageKey = heroes[pair.Key].HeadImageKey;
            var view = obj.GetComponent<HeroLevelUpView>();
            view.Show(heroName, heroImageKey, pair.Value);
        }
    }

    private void _CreateItems(List<Item> items)
    {
        _itemPool.InitPool();

        for(int i = 0;i<items.Count;i++)
        {
            var obj = _itemPool.GetInstance();
            var trans = obj.GetComponent<Transform>();
            trans.SetParent(_itemAnchor);

            var view = obj.GetComponent<ItemView>();
            view.SetData(items[i]);
            view.LockPointer(true);
        }
    }
}
