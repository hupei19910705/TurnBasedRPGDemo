using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;

public class MapPanel : MonoBehaviour
{ 
    [SerializeField] private MapView _mapView = null;
    
    [HideInInspector]
    public IMapPresenter MapPresenter { get; private set; }

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public void Initialize()
    {
        MapPresenter = new MapPresenter(_mapView) as IMapPresenter;
    }
}
