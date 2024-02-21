using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class InspectorHelper
{
    public static void Label(string label)
    {
        EditorGUILayout.LabelField(label);
    }

    public static void Header(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    public static void Toggle(string label, ref bool isToggle)
    {
        isToggle = EditorGUILayout.Toggle(label, isToggle);
    }

    public static void IntField(string label, ref int value)
    {
        value = EditorGUILayout.IntField(label, value);
    }
    public static void IntField(string label, ref int value, int min, int max)
    {
        value = Mathf.Clamp(EditorGUILayout.IntField(label, value), min, max);
    }

    public static void FloatField(string label, ref float value)
    {
        value = EditorGUILayout.FloatField(label, value);
    }

    public static void FloatField(string label, ref float value, float min, float max)
    {
        value = Mathf.Clamp(EditorGUILayout.FloatField(label, value), min, max);
    }

    public static void PropertyField(string label, string value, SerializedObject serializedObject)
    {
        SerializedProperty extra = serializedObject.FindProperty(value);
        EditorGUILayout.PropertyField(extra, new GUIContent(label), true);
    }

    public static void HelpBox(string content)
    {
        EditorGUILayout.HelpBox(content, MessageType.Info);
    }

    public static bool Button (string label)
    {
        return GUILayout.Button(label);
    }

    public static void DrawArrowTypeSelection(TrafficHeadquarter trafficHeadquarter)
    {
        trafficHeadquarter.arrowDrawType = (TrafficHeadquarter.ArrowDraw)
            EditorGUILayout.EnumPopup("화살표 타입", trafficHeadquarter.arrowDrawType);
        EditorGUI.indentLevel ++;

        switch(trafficHeadquarter.arrowDrawType)
        {
            case TrafficHeadquarter.ArrowDraw.FixedCount:
                IntField("Count", ref trafficHeadquarter.arrowCount, 1, int.MaxValue);
                break;
            case TrafficHeadquarter.ArrowDraw.ByLength:
                FloatField("화살표 사이의 거리",ref trafficHeadquarter.arrowDistance);
                break;
            case TrafficHeadquarter.ArrowDraw.Off:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if(trafficHeadquarter.arrowDrawType != TrafficHeadquarter.ArrowDraw.Off )
        {
            FloatField("화살표 사이즈 : 웨이 포인트", ref trafficHeadquarter.arrowSizeWaypoint);
            FloatField("화살표 사이즈 : 교차로", ref trafficHeadquarter.arrowSizeIntersection);
        }

        EditorGUI.indentLevel --;
    }

}
