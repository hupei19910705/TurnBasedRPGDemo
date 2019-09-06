using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.GameUtility;

public class BattlePanel : AsyncLoadingScenePanel
{
    private bool _leave = false;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public override IEnumerator Enter()
    {
        while(!_leave)
            yield return null;
    }
}
