using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class BattlePanel : MonoBehaviour
{
    [SerializeField] private BattleView _battleView = null;
    [SerializeField] private InfoView _infoView = null;

    [HideInInspector] public IBattlePresenter BattlePresenter { get; private set; }
    
    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public void Initialize(GameData gameData,PlayerData playerData,Dictionary<int,EnemyData> enemiesData)
    {
        _battleView.Initialize(gameData,_infoView);
        BattlePresenter = new BattlePresenter(_battleView, playerData, enemiesData, gameData) as IBattlePresenter;
    }
}
