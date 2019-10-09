using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemberGroupView : MonoBehaviour
{
    [Header("Location Root Pos")]
    [SerializeField] private List<Transform> Positions = null;

    public void LocateToPos(Transform transform,int pos)
    {
        if (pos > Positions.Count || pos < 0)
            return;
        transform.SetParent(Positions[pos]);
        transform.localPosition = Vector3.zero;
    }
}
