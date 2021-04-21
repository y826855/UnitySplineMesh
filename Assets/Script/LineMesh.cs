using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineMesh : MonoBehaviour
{
    //메쉬
    public MeshFilter _meshFilter = null;
    public Mesh _currMesh = null;
    public Mesh _originMesh = null;

    public GameObject testLoc = null;

    //노드로 부터 받아올 사이즈와 거리
    [HideInInspector] public float _frontSize = 1.0f;
    [HideInInspector] public float _BackSize = 0.2f;
    [HideInInspector] public float _dist = 1.5f;

    //메쉬의 길이
    [HideInInspector] public float _meshLength = 0;
    public float _meshMinP = 0;
    public float _meshMaxP = 0;

    //노드
    public LineNode _node = null;

    public Vector3 _forwardXYZ = new Vector3(0, 0, 1);


    private void Awake()
    {
        Debug.Log("AWAKE MESH");
        _node = this.GetComponentInParent<LineNode>();

        //메쉬필터 받아옴
        _meshFilter = this.GetComponent<MeshFilter>();

        //원본 있다면 원본 Inst생성
        if (_originMesh != null)
            _meshFilter.sharedMesh = Mesh.Instantiate(_originMesh);
        else
        {
            _originMesh = _meshFilter.sharedMesh;
            _meshFilter.sharedMesh = Mesh.Instantiate(_originMesh);
        }

        ChangeMesh();
    }

    //메쉬 설정
    public void SetMesh(Mesh mesh)
    {
        //오리지널 메쉬 변경
        _originMesh = mesh;

        if (_meshFilter == null) _meshFilter = this.GetComponent<MeshFilter>();

        //원본 수정하지 않기 위해 복사
        _meshFilter.sharedMesh = Mesh.Instantiate(_originMesh);

        _meshMinP = 0;
        _meshMaxP = 0;
    }

    //메쉬 변경사항
    public void ChangeMesh()
    {
        //현재 메쉬 있다면 받아오기
        SetMesh(_originMesh);
        _currMesh = _meshFilter.sharedMesh;
        //_currMesh = Mesh.Instantiate(_originMesh);
        //_currMesh = Mesh.Instantiate(_originMesh);
        //_currMesh = _originMesh;

        //_meshFilter.mesh = _originMesh;



        //축에 따른 최소, 최대치 찾기
        for (int i = 0; i < _currMesh.vertexCount; i++)
        {
            if (_forwardXYZ.x == 1)//x축
            {
                _meshMinP = Mathf.Min(_currMesh.vertices[i].x, _meshMinP);
                _meshMaxP = Mathf.Max(_currMesh.vertices[i].x, _meshMaxP);
            }
            if (_forwardXYZ.y == 1)//y축
            {
                _meshMinP = Mathf.Min(_currMesh.vertices[i].y, _meshMinP);
                _meshMaxP = Mathf.Max(_currMesh.vertices[i].y, _meshMaxP);
            }
            if (_forwardXYZ.z == 1)//z축
            {
                _meshMinP = Mathf.Min(_currMesh.vertices[i].z, _meshMinP);
                _meshMaxP = Mathf.Max(_currMesh.vertices[i].z, _meshMaxP);

                Debug.Log(_currMesh.name);
            }
        }

        //메쉬 구부림
        BendMesh();
    }


    //스플라인에 따라 메쉬 변환
    public void BendMesh()
    {
        //버택스 임시 저장할 리스트
        List<Vector3> verts = new List<Vector3>();

        //노드 받아오기
        if (_node == null)
            _node = this.GetComponentInParent<LineNode>();

        //스플라인 길이 받아옴
        _node.CalcLength();
        _dist = _node._splineLength;

        //늘어날 방향에 따른 버택스 연산
        for (int i = 0; i < _currMesh.vertexCount; i++)
        {
            Vector3 calc = _originMesh.vertices[i];
            //if (_forwardXYZ.z == 1)
            //    CalcVerts(ref calc.x, ref calc.y, ref calc.z);
            //else if (_forwardXYZ.y == 1)
            //    CalcVerts(ref calc.x, ref calc.z, ref calc.y);
            //else if (_forwardXYZ.x == 1)
            //    CalcVerts(ref calc.y, ref calc.z, ref calc.x);

            CalcVerts(ref calc.x, ref calc.y, ref calc.z);
            verts.Add(calc);
        }

        //버택스 값 넣기
        _currMesh.vertices = verts.ToArray();

        //메쉬 다시 연산
        _currMesh.RecalculateBounds();
        _currMesh.RecalculateTangents();

        
    }
    Transform temp;
    //정점 계산
    /*
    public void CalcVerts(ref float X, ref float Y, ref float forward)
    {
        //forward /= 2f;
        //버텍스 위치에 따른 가중치
        float Weight = GetFloatNormal(forward, _meshMinP, _meshMaxP);

        //가중치에 따른 스플라인 위치
        Vector3 splinePoint = _node.GetSplinePoint(Weight) - this.transform.position;

        Vector3 vert = new Vector3(X, Y, splinePoint.z);
        Vector3 OriginVert = new Vector3(X, Y, forward);

        Quaternion dir = Quaternion.LookRotation(_node.GetTangent(Weight));


        //Debug.Log("norm : " + _node.GetTangent(Weight));
        //Debug.Log("forw : " + dir * Vector3.forward);

        //사이즈
        Vector2 vertSize = Vector2.Lerp(_node._scale, _node._nextNode._scale, Weight);

        //TODO : 회전, 탄젠트 등도 넣어보자


        //float vertLength = Mathf.Sqrt((X * X) + (Y * Y));

        float vertLength = Vector3.Distance(Vector3.zero, vert);
        //Vector3 vert = new Vector3(X, Y, splinePoint.z).normalized;

        
        
        Quaternion rot = dir;
        Vector3 res = vert.normalized * vertLength + splinePoint;
        
        X = res.x;
        Y = res.y;
        forward = splinePoint.z;

        ///////////////////////////////
        Vector3 afterVert = new Vector3(X, Y, forward);

        GameObject test = Resources.Load<GameObject>("Cube");
        Vector3 testn = _node.GetTangent(Weight);
        Quaternion testdir = Quaternion.LookRotation(testn);

        vert = dir * vert;
        vert.z = splinePoint.z;

        Vector3 tttt = vert.normalized * (vertLength) + splinePoint;
        Instantiate(test, this.transform.position + tttt, dir);
        //TODO : Z축 줄이야함.. 그냥 방법이 잘못된듯.. 새방법 찾자

        //X = (tttt).x;
        //Y = (tttt).y;
        //forward = (tttt).z;
    }*/

    //TODO : 처음과 끝을 어떻게 깔끔하게?
    public void CalcVerts(ref float X, ref float Y, ref float forward)
    {
        //버텍스 조정
        Vector3 vert = new Vector3(0, Y, -X);

        float Weight = GetFloatNormal(forward, _meshMinP, _meshMaxP);
        //가중치에 따른 스플라인 위치
        Vector3 splinePoint = _node.GetSplinePoint(Weight) - this.transform.position;

        //
        vert = Quaternion.AngleAxis(0.0f, Vector3.right) * vert;

        X = 0;

        //
        Quaternion rot = Quaternion.LookRotation(_node.GetTangent(Weight)) ;
        Quaternion q = rot * Quaternion.Euler(0, -90, 0);

        //최종 버택스 위치
        Vector3 movedVert = q * vert + splinePoint;
        
        X = movedVert.x;
        Y = movedVert.y;
        forward = movedVert.z;
    }


    //숫자 사이 값을 0~1로 반환
    float GetFloatNormal(float get, float min, float max)
    {
        return (get - min) / (max - min);
    }


    //삭제될때
    private void OnDestroy()
    {
        if (_node == null) return;
        _node._mesh = null;

        //Destroy(_node._mesh.gameObject);
    }
}
