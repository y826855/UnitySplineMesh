using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tangent
{
    public Vector3 normal = Vector3.zero;
    public bool upper = false;
}

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

    public Quaternion _rot = Quaternion.identity;

    //방향과 사이즈
    public Vector3 _direction;
    public Vector2 _scale = Vector2.one;

    public float _splineLength = 0;
    public float _roll = 0;

    public LineMesh _mesh = null;

    //스케일 얻어오기
    public Vector2 GetSplineScale(float t)
    {
        if (_nextNode == null)
            return Vector2.zero;


        return Vector2.Lerp(_scale, _nextNode._scale, t);
    }

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

        //B(t) = (1 - t)^3 * P0 + 3 * (1 - t)^2 * t *  P1 + 3 * (1 - t) * t^2 * P2 + t^3 * P3 

        //p0 * (1 - t)^3  +  
        //p1 * 3 * (1 - t)^2 * t  +  
        //P2 * 3 * (1 - t) * t^2  +  
        //P3 * t ^3

        return
            P0 * Mathf.Pow(omt, 3) +
            P1 * Mathf.Pow(omt, 2) * 3 * t +
            P2 * Mathf.Pow(t, 2) * 3 * omt +
            P3 * Mathf.Pow(t, 3);
    }



    //방향 얻어오기
    //public Vector3 GetTangent(float t)
    public Tangent GetTangent(float t)
    {
        Tangent result = new Tangent();

        if (_nextNode == null)
            //return Vector3.zero;
            return result;


        Vector3 P0 = this.transform.position;
        Vector3 P1 = _tanNext;
        Vector3 P2 = _nextNode._tanBefore;
        Vector3 P3 = _nextNode.transform.position;

        //회전 때문에 x축 맞춰줌
        P1.x = P0.x;
        P2.x = P0.x;
        P3.x = P0.x;


        //one minus t
        float omt = 1.0f - t;

        //B'(t) = 3 * (1 - t)^2 * (P1 - P0) + 6 * (1 - t) * t * (P2 - P1) + 3 * t^2 * (P3 - P2). //위 식의 미분?

        //3 * (1 - t) ^ 2 * (P1 - P0) + 
        //6 * (1 - t) * t * (P2 - P1)+
        //3 * t ^ 2 * (P3 - P2).

        Vector3 res = (
            (P1 - P0) * 3 * Mathf.Pow(omt, 2) +
            (P2 - P1) * 6 * omt * t +
            (P3 - P2) * 3 * Mathf.Pow(t, 2)
            );

        //TODO : z가 0 밑으로 내려가면 강제 회전이 일어남.. 어떻게 해결할까?

        Quaternion testRot = Quaternion.LookRotation(res);
        Vector3 euler = testRot.eulerAngles;

        if (res.z > 0)
        {
            euler = new Vector3(euler.x, euler.y, euler.z);
            res = Quaternion.Euler(euler) * Vector3.forward;
            result.upper = true;
        }


        //TODO : 비틀리는 부분 z축이 어떤지 한번 알아보자

        result.normal = res.normalized;
        //result.normal.z = -result.normal.z;

        //return res.normalized;
        return result;

        /*return
            (
            P0 * (-Mathf.Pow(omt, 2)) +
            P1 * (3 * Mathf.Pow(omt, 2) - 2 * omt) +
            P2 * (-3 * Mathf.Pow(t, 2) + 2 * t) +
            P3 * Mathf.Pow(t, 2)
            ).normalized;
            */
        /*
                Vector3 tangent =
                n1.Position * (-omt2) +
                n1.Direction * (3 * omt2 - 2 * omt) +
                GetInverseDirection() * (-3 * t2 + 2 * t) +
                n2.Position * (t2);
         */
    }

    public void SetRot()
    {
        if (_nextNode == null) return;
        _rot = Quaternion.LookRotation(GetTangent(0.0f).normal);
        Debug.Log("reset rot");
    }

    //생성
    public void Spawn(LineNode before = null)
    {
        //가중치 점 초기 위치 설정
        ResetTangent();

        _beforeNode = before;


        before._rot = Quaternion.LookRotation(before.GetTangent(0.0f).normal);
    }

    public void ResetTangent()
    {
        _tanBefore = this.transform.position - Vector3.forward * 0.3f;
        _tanNext   = this.transform.position + Vector3.forward * 0.3f;

        _rot = Quaternion.identity;
        //SetRot();
    }

    public void Update()
    {
        //_tanBefore = this.transform.position - Vector3.forward * 0.3f;
        //_tanNext   = this.transform.position + Vector3.forward * 0.3f;

        //MeshUpdate();
    }



    //메쉬 업데이트 시키기
    public void MeshUpdate()
    {
        MeshUpdateSelf();

        //다음노드도 업데이트 하자
        if (_nextNode != null)
            _nextNode.MeshUpdateSelf();

        //이전 노드도 업데이트 하자
        if (_beforeNode != null)
            _beforeNode.MeshUpdateSelf();

    }

    //자기자긴만 업데이트하기
    public void MeshUpdateSelf()
    {
        if (_mesh != null)
        {
            _mesh.ChangeMesh();
            //Debug.Log("MESH UPDATE");
        }
        //Debug.Log("CALL MESH UPDATE");

    }

    //라인 길이 계산
    public void CalcLength()
    {
        Vector3 beforeNode = this.transform.position;
        float calcLength = 0;

        for (int i = 1; i < 20; i++)
        {
            Vector3 nextNode = GetSplinePoint((float)i * 0.05f);
            calcLength += Vector3.Distance(beforeNode, nextNode);
            beforeNode = nextNode;
        }

        _splineLength = calcLength;
        //Debug.Log(_splineLength);
    }

    ///////////////
    //삭제 되었을때
    private void OnDestroy()
    {
        //부모 배열에서 제거
        if (_parent != null)
            _parent._children.Remove(this);
        else
        {
            //_children.Clear();
            //if (_editor._lineNodes.Count != 0)
            //    _editor._lineNodes.Remove(this);
        }
        _children.Clear();
        //_editor._points

        //Debug.Log("delete");
    }

    /*
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

    inst._parent = getP.GetComponent<LineNode>();

    list.Add(inst);
}*/
}
