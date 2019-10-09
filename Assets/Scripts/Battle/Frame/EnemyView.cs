using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class EnemyView : CharacterView
{
    public override Transform FrontLocate { get { return _leftLocate; } }

    protected override void _Select()
    {
        if(_data.IsAlive)
            base._Select();
    }
}
