using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class test : MonoBehaviour
{
    public Vector3 lookAtPos;

    public GameObject rectPos = null;



    void Update()
    {
        rectPos.transform.position = this.transform.position +
            Quaternion.LookRotation(lookAtPos) * Vector3.forward;
    }
}
