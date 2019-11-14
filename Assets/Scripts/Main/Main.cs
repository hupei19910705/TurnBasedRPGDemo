using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.GameUtility;

public class Main : MonoBehaviour
{
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    private GameData _gameData;

    private void Start()
    {
        _parallelCor.Add(_Main());
        StartCoroutine(_parallelCor.Execute());
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator _Main()
    {
        _gameData = AssetModel.Instance.LoadGameDataExcelFile();
        TeamData teamData = _CreateTeamData();

        while (true)
        {
            //Enter Splash Screen Scene
            yield return _SplashScreen();

            while (true)
            {
                //Enter Map Scene
                yield return _Map();

                //Enter Battle Scene
                yield return _Battle(teamData);
                teamData = _CreateTeamData();
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
        yield return splashView.Run();
    }

    private IEnumerator _Map()
    {
        yield return _SwitchScene(SceneEnum.Map);

        var mapPanel = FindObjectOfType<MapPanel>();
        mapPanel.Initialize();
        IMapPresenter mapPresenter = mapPanel.MapPresenter;
        yield return mapPresenter.Run();
    }

    private IEnumerator _Battle(TeamData teamData)
    {
        yield return _SwitchScene(SceneEnum.Battle);

        var enemiesData = _CreateEnemiesData();

        var battlePanel = FindObjectOfType<BattlePanel>();
        battlePanel.Initialize(_gameData, teamData, enemiesData);
        IBattlePresenter battlePresenter = battlePanel.BattlePresenter;
        yield return battlePresenter.Run();
    }
    #region Team Data
    private TeamData _CreateTeamData()
    {
        Dictionary<int, HeroData> heroes = new Dictionary<int, HeroData>
        {
            { 0,_CreateHero("hero_0001", 0, 0, 1)},
            { 3,_CreateHero("hero_0002", 0, 3, 1)},
            { 2,_CreateHero("hero_0003", 0, 2, 1)}
        };

        var teamData = new TeamData(heroes, new Dictionary<int, Item>());
        teamData.AddItems(new List<Item>
        {
            _CreateItem("10000",0,3),
            _CreateItem("10001",2,120),
            _CreateItem("10000",5,3),
            _CreateItem("20000",9,5)
        });

        return teamData;
    }

    private HeroData _CreateHero(string heroId,double exp,int pos,int level)
    {
        if(_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var heroRow = _gameData.HeroTable[heroId];
        var heroJob = _gameData.HeroJobTable[heroRow.Job];
        return new HeroData(heroRow, heroJob, exp, pos, level);
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
            {0,_CreateEnemyData("enemy_0001",new List<DropItem>{ new DropItem( _CreateItem("10000"),10)},0,1) },
            {1,_CreateEnemyData("enemy_0002",new List<DropItem>{ new DropItem( _CreateItem("10000"),20)},1,1) },
            {2,_CreateEnemyData("enemy_0003",new List<DropItem>{ new DropItem( _CreateItem("10001"),10)},2,1) },
            {5,_CreateEnemyData("enemy_0004",new List<DropItem>{ new DropItem( _CreateItem("20000"),20)},5,1) }
        };
    }

    private EnemyData _CreateEnemyData(string enemyId,List<DropItem> dropItems, int pos, int level)
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData is Null");
            return null;
        }

        var enemyRow = _gameData.EnemyTable[enemyId];
        return new EnemyData(enemyRow, dropItems, pos, level);
    }
    #endregion
}
