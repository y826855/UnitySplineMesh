using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class LineNode : MonoBehaviour
{
    public bool _isRoot = false;

    public LineNode _rootNode = null;
    public List<LineNode> _children = new List<LineNode>();

    //이전, 이후 노드
    public LineNode _nextNode   = null;
    public LineNode _beforeNode = null;
    public LineNode _parent = null;


    //노드 옆 두개의 가중치 점
    public Vector3 _tanNext;
    public Vector3 _tanBefore;

    //방향과 사이즈
    public Vector3 _direction;
    public Vector2 _sclae;

    public float roll;

    //위치 얻어오기
    public Vector3 GetSplinePoint(float t)
    {
        //다음 노드 없으면 0넘김
        if(_nextNode == null)
            return Vector3.zero;


        Vector3 P0 = this.transform.position;
        Vector3 P1 = _tanNext;
        Vector3 P2 = _nextNode._tanBefore;
        Vector3 P3 = _nextNode.transform.position; 

        float omt = 1.0f - t;


        //P0(1 − t)^3 + 3 * P1* t(1 - t)^2 + 3 * P2 * t^2 * (1 - t) + P3*t^3
        return
            P0 * Mathf.Pow(omt, 3)
            + P1 * 3.0f * t * Mathf.Pow(omt, 2)
            + P2 * 3.0f * t * (1.0f - t)
            + P3 * Mathf.Pow(t, 3);

    }

    //방향 얻어오기
    public Vector3 GetTangent(float t)
    {
        if (_nextNode == null)
            return Vector3.zero;


        Vector3 P0 = this.transform.position;
        Vector3 P1 = _tanNext;
        Vector3 P2 = _nextNode._tanBefore; 
        Vector3 P3 = _nextNode.transform.position;

        float omt = 1.0f - t;


        //-P0(1 - t)^2 + P1(3(1 - t)^2 - 2(1 - t)) + P2(-3t^2 + 2t) + P3t^2
        return
            (P0 * Mathf.Pow(omt, 2) * -1
            + P1 * 3 * (Mathf.Pow(omt, 3) - (omt) * -2)
            + P2 * (-3 * Mathf.Pow(t, 2) + 2 * t)
            + P3 * (Mathf.Pow(t, 2))).normalized;

    }

    //생성
    public void Spawn(LineNode before = null)
    {
        //가중치 점 초기 위치 설정
        _tanBefore = Vector3.forward * 0.3f;
        _tanNext = -Vector3.forward * 0.3f;

        _beforeNode = before;
        
        
    }

    //B(t) = (1 - t)3 P0 + 3 (1 - t)2 t P1 + 3 (1 - t) t2 P2 + t3 P3 
    //B'(t) = 3 (1 - t)2 (P1 - P0) + 6 (1 - t) t (P2 - P1) + 3 t2 (P3 - P2).

    /*
    private Vector3 GetTangent(float t)
    {
        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent =
            n1.Position * (-omt2) +
            n1.Direction * (3 * omt2 - 2 * omt) +
            GetInverseDirection() * (-3 * t2 + 2 * t) +
            n2.Position * (t2);
        return tangent.normalized;
    }*/

    public void Spawn(GameObject getP, LineNode before, Vector3 pos, ref List<LineNode> list)
    {
        LineNode inst = Instantiate(Resources.Load("LineNode") as GameObject).GetComponent<LineNode>();

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
            before._nextNode = inst;
            inst.Spawn(before);
            Debug.Log("YES BEFORE");
        }

        //inst.m_parent = getP.GetComponent<LineNode>();

        list.Add(inst);
    }
}
