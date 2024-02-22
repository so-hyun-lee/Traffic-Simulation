using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TrafficHQInspectorEditor 
{
    public static void DrawInspector(TrafficHeadquarter trafficHeadquarter, SerializedObject serializedObject, out bool restructureSystem)
    {
        //기즈모 세팅
        InspectorHelper.Header("기즈모 설정");
        InspectorHelper.Toggle("기즈모를 숨길까요?", ref trafficHeadquarter.hideGizmos);

        //화살표 세팅
        InspectorHelper.DrawArrowTypeSelection(trafficHeadquarter);
        InspectorHelper.FloatField("웨이포인트 크기", ref trafficHeadquarter.waypointSize);
        EditorGUILayout.Space();

        //시스템 설정
        InspectorHelper.Header("시스템 설정");
        InspectorHelper.FloatField("구간 감지 최소 거리", ref trafficHeadquarter.segDetectThresh);
        InspectorHelper.PropertyField("충돌 레이어들", "collisionLayers", serializedObject);
        EditorGUILayout.Space();

        //도움말
        InspectorHelper.HelpBox("Ctrl + 마우스 왼쪽 버튼 : 세그먼트 생성 \n" + 
                                "Shift + 마우스 왼쪽 버튼 : 웨이포인트 생성 \n"+
                                "Alt + 마우스 왼쪽 버튼 : 교차로 생성");
        InspectorHelper.HelpBox("차량은 추가한대로 웨이포인트를 따라서 이동하게 됩니다");
        EditorGUILayout.Space();

        restructureSystem = InspectorHelper.Button("교통 시뮬레이션 시스템 재구성");
    }
}
