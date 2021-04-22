using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class test : MonoBehaviour
{
    public Transform t = null;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation =  Quaternion.LookRotation(t.position.normalized);

        
    }
}
