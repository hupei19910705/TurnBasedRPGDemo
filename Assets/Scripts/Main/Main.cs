using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.GameUtility;

public class Main : MonoBehaviour
{
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    private void Start()
    {
        _parallelCor.Add(_Main());
        StartCoroutine(_parallelCor.Execute());
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator _Main()
    {
        TeamData teamData = _FakeTeamData();

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
                teamData = _FakeTeamData();
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

        var enemiesData = _FakeEnemiesData();

        var battlePanel = FindObjectOfType<BattlePanel>();
        battlePanel.Initialize(teamData, enemiesData);
        IBattlePresenter battlePresenter = battlePanel.BattlePresenter;
        yield return battlePresenter.Run();
    }

    private TeamData _FakeTeamData()
    {
        const string warriorHeadKey = "warrior-sheet-249x100";
        const string wizardHeadKey = "wizard-sheet-b-161x106";
        const string deathKey = "S_Death01";

        //Fake Team Members
        var member0 = new HeroData("大壮", HeroJob.Warrior, 200, 80, warriorHeadKey, deathKey, 80, 5, 0,1);
        member0.SetSkills(new Dictionary<SkillType, List<Skill>>
        {
            { SkillType.GeneralAttack,new List<Skill>{ new Skill(SkillType.GeneralAttack,"10000","普通攻击")} },
            { SkillType.Physical,new List<Skill>{ new Skill(SkillType.GeneralAttack,"20001","物理技能1", "S_Physic01") } }
        });

        var member1 = new HeroData("二柱", HeroJob.Wizard, 150, 120, wizardHeadKey, deathKey, 130, 3, 3,1);
        member1.SetSkills(new Dictionary<SkillType, List<Skill>>
        {
            { SkillType.GeneralAttack,new List<Skill>{ new Skill(SkillType.GeneralAttack,"10000","普通攻击")} },
            { SkillType.Magic,new List<Skill>{ new Skill(SkillType.GeneralAttack,"30001","魔法技能1", "S_Magic01") } }
        });

        var member2 = new HeroData("三柱", HeroJob.Warrior, 200, 80, warriorHeadKey, deathKey, 80, 5, 2,1);
        member2.SetSkills(new Dictionary<SkillType, List<Skill>>
        {
            { SkillType.GeneralAttack,new List<Skill>{ new Skill(SkillType.GeneralAttack,"10000","普通攻击")} },
            { SkillType.Physical,new List<Skill>{ new Skill(SkillType.GeneralAttack,"20001", "物理技能1", "S_Physic01") } }
        });
        Dictionary<int, HeroData> heroes = new Dictionary<int, HeroData>
        {
            { member0.Pos,member0},
            { member1.Pos,member1},
            { member2.Pos,member2},
        };

        //Fake BackPack
        var item0 = new Item(ItemType.RedPotion, "10000", 3,"红药水",50,0, "P_Red03");
        var item1 = new Item(ItemType.RedPotion, "10001", 18,"大红药水",200,2, "P_Red01");
        var item2 = new Item(ItemType.RedPotion, "10000", 3,"红药水",50,5,"P_Red03");
        var item3 = new Item(ItemType.BluePotion, "20000", 5,"蓝药水",50,9, "P_Blue03");

        //Fake TeamData
        var teamData = new TeamData(heroes,new Dictionary<int, Item>());
        teamData.AddItems(new List<Item> { item0, item1, item2, item3 });

        return teamData;
    }

    private Dictionary<int,EnemyData> _FakeEnemiesData()
    {
        var enemy0 = new EnemyData("蛇", EnemyType.Snake, 300, 30, 2, 0, 1, 30);
        enemy0.SetDropItems(new List<DropItem> { new DropItem(new Item(ItemType.RedPotion, "10000", "红药水", 50, 0, "P_Red03"), 10) });
        var enemy1 = new EnemyData("猪", EnemyType.Pig, 400, 30, 2, 1, 1, 40);
        enemy1.SetDropItems(new List<DropItem> { new DropItem(new Item(ItemType.RedPotion, "10000", "红药水", 50, 0, "P_Red03"), 20) });
        var enemy2 = new EnemyData("黑猪", EnemyType.DarkPig, 500, 40, 2, 2, 1, 50);
        enemy2.SetDropItems(new List<DropItem> { new DropItem(new Item(ItemType.RedPotion, "10001", "大红药水", 200, 2, "P_Red01"), 10) });
        var enemy3 = new EnemyData("蝙蝠", EnemyType.Bat, 250, 50, 1, 5, 1, 50);
        enemy3.SetDropItems(new List<DropItem> { new DropItem(new Item(ItemType.BluePotion, "20000", "蓝药水", 50, 9, "P_Blue03"), 20) });

        return new Dictionary<int, EnemyData>
        {
            {enemy0.Pos,enemy0 },
            {enemy1.Pos,enemy1 },
            {enemy2.Pos,enemy2 },
            {enemy3.Pos,enemy3 },
        };
    }
}
