﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.GameUtility;
using System.Linq;

public class Main : MonoBehaviour
{
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    private GameData _gameData;
    private GameRecords _gameRecords;
    private GameRecord _curRecord;

    private const string GAMEDATA_EXCEL_FILE_PATH = "/Data/GameData.xlsx";

    private void Start()
    {
        _parallelCor.Add(_Main());
        StartCoroutine(_parallelCor.Execute());
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator _Main()
    {
        _gameData = AssetModel.Instance.LoadGameDataExcelFile(GAMEDATA_EXCEL_FILE_PATH);
        
        while (true)
        {
            //Enter Splash Screen Scene
            yield return _SplashScreen();

            while (true)
            {
                PlayerData playerData = _CreatePlayerData();
                //Enter Map Scene
                yield return _Map();

                //Enter Battle Scene
                yield return _Battle(playerData);
            }
        }
    }

    private IEnumerator _SwitchScene(SceneEnum target)
    {
        SceneModel.Instance.LoadScene(SceneEnum.Loading);
        yield return null;
        var loadingView = FindObjectOfType<LoadingScenePanel>();
        yield return loadingView.Enter(target);
    }

    private IEnumerator _SplashScreen()
    {
        SceneModel.Instance.LoadScene(SceneEnum.SplashScreen);
        yield return null;
        var splashView = FindObjectOfType<SplashScreenPanel>();

        _gameRecords = GameUtility.Instance.Load();
        splashView.Init(_gameRecords, _gameData);

        yield return splashView.Run();
        _curRecord = _gameRecords[splashView.SelectRecordId];
    }

    private IEnumerator _Map()
    {
        yield return _SwitchScene(SceneEnum.Map);

        var mapPanel = FindObjectOfType<MapPanel>();
        mapPanel.Initialize();
        IMapPresenter mapPresenter = mapPanel.MapPresenter;
        yield return mapPresenter.Run();
        GameUtility.Instance.Save(_gameRecords);
    }

    private IEnumerator _Battle(PlayerData playerData)
    {
        yield return _SwitchScene(SceneEnum.Battle);

        var enemiesData = _CreateEnemiesData();

        var battlePanel = FindObjectOfType<BattlePanel>();
        battlePanel.Initialize(_gameData, playerData, enemiesData);
        IBattlePresenter battlePresenter = battlePanel.BattlePresenter;
        yield return battlePresenter.Run();
        GameUtility.Instance.Save(_gameRecords);
    }
    #region Team Data
    private PlayerData _CreatePlayerData()
    {
        Dictionary<string, HeroData> heroes = new Dictionary<string, HeroData>();
        foreach (var heroRecord in _curRecord.HeroRecord)
        {
            var hero = _CreateHero(heroRecord.ID, heroRecord.Exp, heroRecord.Level, heroRecord.UID);
            heroes.Add(hero.UID, hero);
        }

        List<Item> items = new List<Item>();
        foreach(var itemRecord in _curRecord.ItemRecord)
        {
            var item = _CreateItem(itemRecord.ID, itemRecord.Pos, itemRecord.Count);
            items.Add(item);
        }

        return new PlayerData(heroes, _curRecord.TeamRecord, items);
    }

    private HeroData _CreateHero(string heroId,double exp,int level, string uid = "")
    {
        if(_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var heroRow = _gameData.HeroTable[heroId];
        var heroJob = _gameData.HeroJobTable[heroRow.Job];
        if (string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();
        return new HeroData(uid,heroRow, heroJob, exp, level);
    }

    private Item _CreateItem(string itemId, int pos = -1, int count = -1)
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var itemRow = _gameData.ItemTable[itemId];
        return new Item(itemRow, pos, count);
    }
    #endregion

    #region Enemy Data
    private Dictionary<int, EnemyData> _CreateEnemiesData()
    {
        return new Dictionary<int, EnemyData>
        {
            {0,_CreateEnemyData("enemy_0001",new List<DropItem>{ new DropItem( _CreateItem("10000"),10)},1) },
            {1,_CreateEnemyData("enemy_0002",new List<DropItem>{ new DropItem( _CreateItem("10000"),20)},1) },
            {2,_CreateEnemyData("enemy_0003",new List<DropItem>{ new DropItem( _CreateItem("10001"),10)},1) },
            {5,_CreateEnemyData("enemy_0004",new List<DropItem>{ new DropItem( _CreateItem("20000"),20)},1) }
        };
    }

    private EnemyData _CreateEnemyData(string enemyId,List<DropItem> dropItems, int level,string uid = "")
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var enemyRow = _gameData.EnemyTable[enemyId];
        if(string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();
        return new EnemyData(uid, enemyRow, dropItems, level);
    }
    #endregion
}
