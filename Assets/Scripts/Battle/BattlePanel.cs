using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class BattlePanel : MonoBehaviour
{
    [SerializeField] private BattleView _battleView = null;

    [HideInInspector] public IBattlePresenter BattlePresenter { get; private set; }
    
    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public void Initialize(TeamData teamData,Dictionary<int,EnemyData> enemiesData)
    {
        BattlePresenter = new BattlePresenter(_battleView, teamData, enemiesData) as IBattlePresenter;
    }
}
