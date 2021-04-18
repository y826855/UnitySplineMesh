using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LineNode))]
public class Editor_LineNode : Editor
{

    //target 받아올 변수들
    private LineNode _select = null;
    private List<LineNode> _selects = new List<LineNode>();

    //traget 받아와 저장
    private void OnEnable()
    {
        _select = target as LineNode;

        //배열로 받기
        _selects.Clear();
        foreach (var it in targets)
            _selects.Add(it as LineNode);

    }

    //inspector 편집
    public override void OnInspectorGUI()
    {

        //EditorGUILayout.ObjectField("Is Root", _select._isRoot, typeof(bool), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_nextNode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_beforeNode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_parent"));


        EditorGUI.BeginChangeCheck();
        bool check = EditorGUILayout.Toggle("IsRoot", _select._isRoot);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_select);
            _select._isRoot = check;
        }





        if (GUILayout.Button("ADD LINE NODE"))
        {
            //last node 불러오자
            //TODO : 수정필요

            LineNode beforeNode = null;
            LineNode parent = null;
            
            //선택된 노드가 부모가 있다
            if (_select._parent != null)
            {
                beforeNode = _select._parent._children
                    [_select._parent._children.Count - 1];

                parent = _select._parent;
            }
            //선택된 노드가 부모 없다
            else
            {
                beforeNode = _select;
                parent = _select;
            }

            Vector3 spawnPos = beforeNode.transform.position + Vector3.forward;

            //노드 생성
            //Debug.Log(parent._children.Count);
            _select.Spawn(parent.gameObject, beforeNode, spawnPos, ref parent._children);
            //Spline.Spawn(null, null, Vector3.zero, ref parent._children);
            //Spline.Spawn(parent.gameObject, beforeNode, spawnPos, ref parent._children);

            //if (_select._isRoot)
            //    Spline.Spawn(null, _select, _select.transform.position, ref _select._children);
        }
    }

    //
    private void OnSceneGUI()
    {
        //핸들 가시화  


        //if (Handles.Button(parent.transform.position, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap))
        //{ }

        for (int i = 0; i < 20; i++)
        {
            float t = (float)i * 0.05f;

            Vector3 pos = _select.GetSplinePoint(t);
            Handles.Button(pos, Quaternion.identity, 0.01f, 0.01f, Handles.DotCap);
            
            Vector3 rot = _select.GetTangent(t);
            Handles.DoPositionHandle(pos, Quaternion.LookRotation(rot));
        }
    }
}
