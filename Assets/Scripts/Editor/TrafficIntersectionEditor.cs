using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrafficIntersectionEditor : Editor
{
    private TrafficIntersection intersection;

    private void OnEnable()
    {
        intersection= target as TrafficIntersection;
    }


    public override void OnInspectorGUI()
    {
        intersection.IntersectionType = (IntersectionType)EditorGUILayout.EnumPopup("교차로 타입", intersection.IntersectionType);
        EditorGUI.BeginDisabledGroup(intersection.IntersectionType != IntersectionType.STOP);
        {
            InspectorHelper.Header("우선 멈춤 구간");
            InspectorHelper.PropertyField("우선구간", "PrioritySegments", serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(intersection.IntersectionType != IntersectionType.TRAFFIC_LIGHT);
        {
            InspectorHelper.Header("신호 교차로");
            InspectorHelper.FloatField("신호 시간(초)", ref intersection.lightDuration);
            InspectorHelper.FloatField("주황불 시간(초)", ref intersection.orangeLightDuration);
            InspectorHelper.PropertyField("첫번째 빨간불 그룹1", "lightGroup1", serializedObject);
            InspectorHelper.PropertyField("두번째 빨간불 그룹2", "lightGroup2", serializedObject);
            serializedObject.ApplyModifiedProperties();

        }
        EditorGUI.EndDisabledGroup() ;
    }
}
