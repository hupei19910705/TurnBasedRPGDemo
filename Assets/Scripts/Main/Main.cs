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
        //Enter Splash Screen View
        SceneModel.Instance.LoadScene(SceneEnum.SplashScreen);
        yield return null;
        var splashView = FindObjectOfType<SplashScreenView>();
        yield return splashView.Run();

        //Enter Map View
        SceneModel.Instance.LoadScene(SceneEnum.Loading);
        yield return null;
        var loadingView = FindObjectOfType<LoadingSceneView>();
        yield return loadingView.Enter();
        


        yield return null;
    }
}
