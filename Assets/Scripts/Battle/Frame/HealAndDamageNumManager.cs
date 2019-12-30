using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.GameUtility;

public class HealAndDamageNumManager : MonoBehaviour
{
    [SerializeField] private GeneralObjectPool _pool = null;

    private Transform _root = null;
    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    public void Init(Transform root)
    {
        _root = root;
        _pool.InitPool();
        StartCoroutine(_parallelCor.Execute());
    }

    public void Show(List<ResultModel> models)
    {
        if (models == null || models.Count == 0)
            return;

        _ShowNumText(models[0], false);
        if (models.Count > 1)
        {
            for (int i = 1; i < models.Count; i++)
                _ShowNumText(models[i], true);
        }
    }

    private void _ShowNumText(ResultModel model,bool wait)
    {
        var instance = _pool.GetInstance();
        var trans = instance.transform;
        trans.SetParent(_root);
        trans.localPosition = Vector3.zero;

        var view = instance.GetComponent<NumView>();
        view.ReturnObject -= _pool.ReturnInstance;
        view.ReturnObject += _pool.ReturnInstance;
        _parallelCor.Add(_OnShowNumText(view, model, wait));
    }

    private IEnumerator _OnShowNumText(NumView view, ResultModel model, bool wait)
    {
        if (wait)
            yield return MyCoroutine.Sleep(0.5f);

        yield return view.Show(model, _root.localScale);
    }
}