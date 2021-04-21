using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class testVert : MonoBehaviour
{

    public Vector3 lookatPos1;
    public Vector3 lookatPos2;
    public Vector3 lookatPos3;


    MeshFilter mf;
    public Mesh ms;
    public Mesh msOri;

    private void Start()
    {
        mf = this.GetComponent<MeshFilter>();
        
        mf.sharedMesh = Mesh.Instantiate(msOri);
        ms = Mesh.Instantiate(mf.sharedMesh);
    }

    private void Update()
    {

        float _meshMaxP = 0;
        float _meshMinP = 0;

        for (int i = 0; i < ms.vertexCount; i++)
        {
            _meshMinP = Mathf.Min(ms.vertices[i].z, _meshMinP);
            _meshMaxP = Mathf.Max(ms.vertices[i].z, _meshMaxP);
        }

        for (int i = 0; i < ms.vertexCount; i++)
        {
            float xyLength = Mathf.Sqrt((ms.vertices[i].x * ms.vertices[i].x) + (ms.vertices[i].y * ms.vertices[i].y));

            float Weight = GetFloatNormal(3, _meshMinP, _meshMaxP);

            Quaternion.LookRotation(lookatPos1);

            if (Weight < 0.33)
            {
                ms.vertices[i] = Quaternion.LookRotation(lookatPos1) * ms.vertices[i].normalized * xyLength;
            }
            else if (Weight < 0.66)
            {
                ms.vertices[i] = Quaternion.LookRotation(lookatPos2) * ms.vertices[i].normalized * xyLength;

            }
            else
            {
                ms.vertices[i] = Quaternion.LookRotation(lookatPos3) * ms.vertices[i].normalized * xyLength;

            }
        }


    }



    //숫자 사이 값을 0~1로 반환
    float GetFloatNormal(float get, float min, float max)
    {
        return (get - min) / (max - min);
    }


}
