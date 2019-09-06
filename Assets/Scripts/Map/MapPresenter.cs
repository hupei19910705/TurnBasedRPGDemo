using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapPresenter
{
    IEnumerator Run();
}

public class MapPresenter : IMapPresenter
{
    public IEnumerator Run()
    {
        yield return null;
    }
}
