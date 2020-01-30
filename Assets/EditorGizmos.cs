using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGizmos : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, Vector3.one);
        //Gizmos.DrawCube(Vector3.one, Vector3.one);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position, 1);
    }
}