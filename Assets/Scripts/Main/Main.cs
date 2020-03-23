using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.GameUtility;
using System.Linq;
using System;

public class Main : MonoBehaviour
{
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    private GameData _gameData;
    private GameRecord _curRecord;

    private const string GAMEDATA_EXCEL_FILE_PATH = "/StreamingAssets/GameData.xlsx";

    private void Start()
    {
        _parallelCor.Add(_Main());
        StartCoroutine(_parallelCor.Execute());
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator _Main()
    {
        _gameData = AssetModel.Instance.LoadGameDataExcelFile(GAMEDATA_EXCEL_FILE_PATH);
        CharacterUtility.Instance.Init(_gameData);

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

        splashView.Init( _gameData);

        yield return splashView.Run();
        _curRecord = GameUtility.Instance.GetCurGameRecord();
    }

    private IEnumerator _Map()
    {
        yield return _SwitchScene(SceneEnum.Map);

        var mapPanel = FindObjectOfType<MapPanel>();
        mapPanel.Initialize();
        IMapPresenter mapPresenter = mapPanel.MapPresenter;
        yield return mapPresenter.Run();
        GameUtility.Instance.Save();
    }

    private IEnumerator _Battle(PlayerData playerData)
    {
        yield return _SwitchScene(SceneEnum.Battle);

        var enemiesData = _CreateEnemiesData();

        var battlePanel = FindObjectOfType<BattlePanel>();
        battlePanel.Initialize(_gameData, playerData, enemiesData);
        IBattlePresenter battlePresenter = battlePanel.BattlePresenter;
        yield return battlePresenter.Run();
        GameUtility.Instance.Save();
    }
    #region Team Data
    private PlayerData _CreatePlayerData()
    {
        Dictionary<string, HeroData> heroes = new Dictionary<string, HeroData>();
        foreach (var heroRecord in _curRecord.HeroRecord.Values)
        {
            var hero = _CreateHero(heroRecord.ID, heroRecord.Exp, heroRecord.Level, heroRecord.UID);
            heroes.Add(hero.UID, hero);
        }

        Dictionary<string, Item> items = new Dictionary<string, Item>();
        foreach(var pair in _curRecord.ItemRecord)
        {
            var itemRecord = pair.Value;
            var item = _CreateItem(itemRecord.ID, itemRecord.Count);
            items.Add(pair.Key, item);
        }

        return new PlayerData(heroes, _curRecord.TeamRecord, items, _gameData.ConstantData.BACKPACK_MAX_SIZE);
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
        var skills = CharacterUtility.Instance.GetUnLockHeroSkills(heroRow.Job, level);

        if (string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();
        return new HeroData(uid, heroRow, heroJob, exp, level, skills);
    }

    private Item _CreateItem(string itemId,int count = 1)
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var itemRow = _gameData.ItemTable[itemId];
        return new Item(itemRow, count);
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
        var skills = CharacterUtility.Instance.GetUnLockEnemySkills(enemyRow.Type, level);

        if(string.IsNullOrEmpty(uid))
            uid = GameUtility.GenerateOrderId();
        return new EnemyData(uid, enemyRow, dropItems, level, skills);
    }
    #endregion
}
