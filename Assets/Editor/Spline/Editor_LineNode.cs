using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(LineNode))]


public class Editor_LineNode : Editor
{
    //target 받아올 변수들
    private LineNode _select = null;
    private List<LineNode> _selects = new List<LineNode>();
    private LineNode _selectionRoot = null;

    public GameObject _defaultLineMesh = null;

    enum MouseInput { node, tanNext, tanBefore };
    MouseInput mode = MouseInput.node;

    private void Awake()
    {
        _defaultLineMesh = Resources.Load<GameObject>("DefaultLineMesh");
    }

    //traget 받아와 저장
    private void OnEnable()
    {
        _select = target as LineNode;

        //배열로 받기
        _selects.Clear();
        foreach (var it in targets)
            _selects.Add(it as LineNode);

        //기본 입력모드 설정
        mode = MouseInput.node;


        _selectionRoot = _select._parent;
        //선택한 노드의 최고 root 찾기
        while (true)
        {
            //루트노드 선택시
            if (_selectionRoot == null)
            {
                _selectionRoot = _select;
                break;
            }

            //일반 노드 선택시
            if (_selectionRoot._parent != null)
                _selectionRoot = _selectionRoot._parent;
            else
                break;
            
        }
    }

    float testInterval = 0.1f;

    //inspector 편집
    public override void OnInspectorGUI()
    {

        //EditorGUILayout.ObjectField("Is Root", _select._isRoot, typeof(bool), true);

        //테스트용
        {
            EditorGUILayout.LabelField("------------------------");
            EditorGUILayout.LabelField("Debug Field");


            EditorGUILayout.PropertyField(serializedObject.FindProperty("_nextNode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_beforeNode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_parent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_children"));

            if (GUILayout.Button("DEBUG UPDATE MESH"))
            {
                _selectionRoot.MeshUpdateSelf();
                foreach (var it in _selectionRoot._children)
                {
                    it.MeshUpdateSelf();
                }
            }
        }


        //입력 구간
        {
            EditorGUILayout.LabelField("------------------------");
            EditorGUILayout.LabelField("Input Field");


            EditorGUI.BeginChangeCheck();
            bool check = EditorGUILayout.Toggle("IsRoot", _select._isRoot);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_select);
                _select._isRoot = check;
            }

            testInterval = EditorGUILayout.FloatField("interval", testInterval);
        }


        //버튼 구간
        {
            EditorGUILayout.LabelField("------------------------");
            EditorGUILayout.LabelField("Button Field");

            //탄젠트 리셋
            if (GUILayout.Button("RESET TANGENT"))
            {
                foreach (var it in _selects)
                    it.ResetTangent();
                
            }

                //라인 노드 추가
             if (GUILayout.Button("ADD LINE NODE"))
            {
                //last node 불러오자
                //TODO : 수정필요

                LineNode beforeNode = null;
                LineNode parent = null;

                //선택된 노드가 부모가 있다
                if (_select._parent != null)
                {
                    //부모의 마지막 노드 불러오기
                    beforeNode = _select._parent._children
                        [_select._parent._children.Count - 1];

                    parent = _select._parent;
                }

                //선택된 노드가 부모 없다
                else
                {
                    //루트에 자식이 없으면 자기자신
                    if (_select._children.Count == 0)
                        beforeNode = _select;
                    else//있으면 자식 보내줌
                        beforeNode = _select._children[_select._children.Count - 1];
                    parent = _select;
                }

                //노드 생성될 위치
                Vector3 spawnPos = beforeNode.transform.position + Vector3.forward;

                //노드 생성
                EditorUtility.SetDirty(parent);
                SpawnNode(parent.gameObject, beforeNode, spawnPos, ref parent._children);
            }

            //메쉬 입력
            {
                EditorGUI.BeginChangeCheck();
                //_selectedLine.mesh = EditorGUILayout.ObjectField("Mesh", _selectedLine.mesh, typeof(GameObject), true) as GameObject;
                Mesh mesh = null;
                bool isChange = true;
                //메쉬 입력받기
                if (_select._mesh != null)
                {
                    mesh = EditorGUILayout.ObjectField("Mesh", _select._mesh.GetComponent<LineMesh>()._originMesh, typeof(Mesh), true) as Mesh;
                }

                //라인 메쉬 생성하기
                else
                {
                    //라인 메쉬 없으면 추가시키는 버튼 등장
                    if (_select._nextNode != null)
                        if (GUILayout.Button("Add LineMesh"))
                        {
                            foreach (var it in _selects)
                            {//혹시 메쉬 있는게 있다면 패스
                                if (it._mesh == null)
                                {
                                    //라인 메쉬 생성
                                    it._mesh = Instantiate(_defaultLineMesh).GetComponent<LineMesh>();
                                    EditorUtility.SetDirty(it._mesh);

                                    it._mesh._node = it;
                                    EditorUtility.SetDirty(it._mesh);
                                    Debug.Log(it._mesh.name);
                                    it._mesh.transform.SetParent(it.transform);
                                    it.MeshUpdate();
                                }
                            }
                        }
                    isChange = false;
                }

                //라인메쉬 입력 종료시
                if (EditorGUI.EndChangeCheck() && isChange == true)
                {
                    if (_select.GetComponentInChildren<MeshFilter>() != null)
                    {//메쉬 찾기

                        foreach (var it in _selects)
                        {
                            Undo.RecordObject(it, "Set Mesh");
                            it._mesh.GetComponent<LineMesh>().SetMesh(mesh);
                            it.MeshUpdate();
                        }
                    }
                }
            }//Check LineMesh

        }

    }



    //노드 생성
    private void SpawnNode(GameObject getP, LineNode before, Vector3 pos, ref List<LineNode> list)
    { 
        //인스턴스 생성
        LineNode inst = Instantiate(Resources.Load("LineNode") as GameObject).GetComponent<LineNode>();

        //위치 지정
        inst.transform.SetParent(getP.transform);
        inst.transform.position = pos;

        //에디터에서도 정보 저장하기
        EditorUtility.SetDirty(before);
        EditorUtility.SetDirty(inst);

        //생성
        if (before == null)
        {
            inst.Spawn();
            Debug.Log("NO BEFORE");
        }
        else
        {//이전 노드에 생성된 노드 정보 줌
            before._nextNode = inst;
            inst.Spawn(before);
            Debug.Log("YES BEFORE");
        }

        inst._parent = getP.GetComponent<LineNode>();

        //자식으로 추가
        list.Add(inst);
    }



    //
    private void OnSceneGUI()
    {
        //핸들 가시화  


        //if (Handles.Button(parent.transform.position, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap))
        //{ }





        //모든 노드 훑기
        if (_selectionRoot != null)
        {
            ShowLine(_selectionRoot);
            VisitAllNode(_selectionRoot);
        }


        //Vector3 move = Vector3.zero;

        switch (mode)
        {
            case MouseInput.node:
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 move = Handles.PositionHandle(_select.transform.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_select.transform, "move vert");
                        EditorUtility.SetDirty(_select);
                        _select.transform.position = move;
                    }
                    Undo.RecordObject(_select, "move vert");
                }
                break;

            case MouseInput.tanBefore:
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 move = Handles.PositionHandle(_select._tanBefore, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_select, "move vert");
                        //_select._tanNext = move * 2 - _select._tanNext;
                        _select._tanNext = (_select.transform.position - move) + _select.transform.position;
                        _select._tanBefore = move;
                        
                    }
                    Undo.RecordObject(_select, "move vert");
                }
                break;

            case MouseInput.tanNext:
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 move = Handles.PositionHandle(_select._tanNext, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_select, "move vert");
                        //_select._tanBefore = (move - _select.transform.position) *-2 - (_select._tanBefore - _select.transform.position);
                        //_select._tanBefore = (move - _select.transform.position) *-2 - (_select._tanBefore - _select.transform.position);
                        //_select._tanNext = move * -2 - _select._tanNext;
                        //_select._tanBefore = (move - _select.transform.position) + _select.transform.position;
                        _select._tanBefore = (_select.transform.position - move) + _select.transform.position;
                        _select._tanNext = move;
                    }
                    Undo.RecordObject(_select, "move vert");
                }
                break;
        }


    }

    //핸들 움직임 받기
    private void MoveByHandle(ref Vector3 move, GameObject record)
    {
        //EditorGUI.BeginChangeCheck();
        //Vector3 pos = Handles.PositionHandle(move, Quaternion.identity);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    Undo.RecordObject(record, "move vert");
        //    //EditorUtility.SetDirty(record);
        //    move = pos;
        //}

        Undo.RecordObject(record, "move vert");
        move = Handles.PositionHandle(move, Quaternion.identity);
    }

    //노드 아래 자식이 노드가 있다면 재귀
    private void VisitAllNode(LineNode node)
    {
        //자식 노드 있나?
        if (node._children.Count != 0)
        {
            foreach (var it in _selectionRoot._children)
            {
                ShowLine(it);
                
                //자식의 자식 노드 있으면 재귀
                if (it._children.Count > 0)
                    VisitAllNode(it);
            }
        }
    }

    //라인 그리기
    private void ShowLine(LineNode node)
    {
        //노드 위치 보여주기
        Handles.color = Color.red;
        Handles.Button(node.transform.position, Quaternion.identity, 0.02f, 0.02f, Handles.DotHandleCap);

        if (node._nextNode == null) {
            return;
        }
        
        Vector3 before = node.GetSplinePoint(0);
        int verts = 10;
        for (int i = 0; i < verts; i++)
        {//Show Spline Weight

            float t = (float)i * 1.0f/(float)verts;

            Vector3 pos = node.GetSplinePoint(t);
            Handles.DotHandleCap(0, pos, Quaternion.identity, 0.01f, EventType.Repaint);

            Handles.color = Color.blue;
            Handles.DrawLine(before, pos);
            before = pos;

            Vector3 rot = node.GetTangent(t);
            //rot.z = 0;
            Vector3 tt = Quaternion.LookRotation(rot) * Vector3.up * testInterval;
            //tt.z = 0;
            //if (rot != Vector3.zero)
            //    Handles.DoPositionHandle(pos + tt, Quaternion.LookRotation(rot) * Quaternion.Euler(0, -90, 0));
                //Handles.DoPositionHandle(Quaternion.LookRotation(rot) * pos, Quaternion.identity);
                //Handles.DoPositionHandle(Quaternion.LookRotation(rot) * pos, Quaternion.identity);
        }

        //Show Tanget, 클릭에 따른 입력모드 전환
        Handles.color = Color.green;
        if (Handles.Button(_select._tanBefore, Quaternion.identity, 0.01f, 0.01f, Handles.DotHandleCap))
        {
            mode = MouseInput.tanBefore;
        }
        Handles.color = Color.yellow;
        if (Handles.Button(_select._tanNext, Quaternion.identity, 0.01f, 0.01f, Handles.DotHandleCap))
        {
            mode = MouseInput.tanNext;
        }
    }
}
