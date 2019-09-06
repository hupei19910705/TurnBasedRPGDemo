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
        while(true)
        {
            //Enter Splash Screen View
            SceneModel.Instance.LoadScene(SceneEnum.SplashScreen);
            yield return null;
            var splashView = FindObjectOfType<SplashScreenPanel>();
            yield return splashView.Run();

            //Enter Map View
            yield return _SwitchScene(SceneEnum.Map);

            //Enter Battle View
            yield return _SwitchScene(SceneEnum.Battle);

            yield return null;
        }
    }

    private IEnumerator _SwitchScene(SceneEnum target)
    {
        SceneModel.Instance.LoadScene(SceneEnum.Loading);
        yield return null;
        var loadingView = FindObjectOfType<LoadingScenePanel>();
        yield return loadingView.Enter(target);
        var targetView = FindObjectOfType<AsyncLoadingScenePanel>();
        yield return targetView.Enter();
    }
}
