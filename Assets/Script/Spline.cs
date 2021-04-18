using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{


    //생성
    static public void Spawn(GameObject getP, LineNode before, Vector3 pos, ref List<LineNode> list)
    {
        LineNode inst = Instantiate(Resources.Load("Node") as GameObject).GetComponent<LineNode>();

        inst.transform.SetParent(getP.transform);
        inst.transform.position = pos;

        //inst._editor = this; //에디터 정보 주기

        if (before == null)
        {
            inst.Spawn();
            Debug.Log("NO BEFORE");
        }
        else
        {
            inst.Spawn(before);
            Debug.Log("YES BEFORE");
        }

        //inst.m_parent = getP.GetComponent<LineNode>();

        list.Add(inst);
    }

}
