using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility.GameUtility
{
    public enum SceneEnum
    {
        Main,
        SplashScreen,
        Map,
        Battle,
        Loading
    }

    public class SceneModel
    {
        private static SceneModel _instance;
        public static SceneModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SceneModel();
                return _instance;
            }
        }
        #region Public Func
        public void GoToStartScene()
        {
            if (GameObject.Find("/Main") == null)
                LoadScene(SceneEnum.Main);
        }

        public void LoadScene(SceneEnum targetScene)
        {
            SceneManager.LoadScene(_GetSceneNameByEnum(targetScene));
        }

        public IEnumerator LoadSceneAsync(SceneEnum targetScene,ProcessBar process)
        {
            var async = SceneManager.LoadSceneAsync(_GetSceneNameByEnum(targetScene));
            async.allowSceneActivation = false;
            var progress = 0f;
            while (!async.isDone)
            {
                progress = async.progress;
                if (progress >= 0.9f)
                {
                    process.UpdateBar(100f);
                    if (!async.allowSceneActivation)
                        async.allowSceneActivation = true;
                }
                else
                    process.UpdateBar(progress * 100);
                yield return null;
            }
        }

        public bool IsTargetScene(SceneEnum target)
        {
            return string.Equals(SceneManager.GetActiveScene().name, _GetSceneNameByEnum(target));
        }
        #endregion

        #region Private Func
        private string _GetSceneNameByEnum(SceneEnum scene)
        {
            return scene.ToString();
        }
        #endregion
    }

    public class ProcessBar
    {
        private Transform _processBar;
        private float _startPosX;
        private float _endPosX;
        private Text _processText;

        public ProcessBar(Transform bar, float start, float end, Text text)
        {
            _processBar = bar;
            _startPosX = start;
            _endPosX = end;
            _processText = text;
        }

        public void UpdateBar(float rate)
        {
            rate = Mathf.Clamp(rate, 0f, 100f);
            rate = (float)Math.Round(rate, 2, MidpointRounding.AwayFromZero);
            var point = Mathf.Lerp(_startPosX, _endPosX, rate / 100f);
            _processBar.localPosition = new Vector3(point, _processBar.localPosition.y, _processBar.localPosition.z);
            _processText.text = string.Format("{0}%", rate);
        }
    }
}
