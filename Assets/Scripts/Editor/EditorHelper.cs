using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.YamlDotNet.Serialization.TypeResolvers;
using System;

public static class EditorHelper 
{
    public static void SetUndoGroup(string label)
    {
        //�� �ڷ� ������ ��� ��ȭ�� �ϳ��� �׷����� ���´ٴ� ��
        //ctrl+z�� ��� �׷� ������ ��ҵ�
        Undo.SetCurrentGroupName(label);
    } 

    public static void BeginUndoGroup(string undoName, TrafficHeadquarter trafficHeadquarter)
    {
        //undo�׷� ����
        Undo.SetCurrentGroupName(undoName);
        //headquarter���� �߻��ϴ� ��� ��ȭ�� ����ϰ� ��
        Undo.RegisterFullObjectHierarchyUndo(trafficHeadquarter.gameObject, undoName);
    }

    public static GameObject CreateGameObject(string name, Transform parent = null)
    {
        GameObject newGameObject = new GameObject(name);
        newGameObject.transform.position = Vector3.zero;
        newGameObject.transform.localScale = Vector3.one;
        newGameObject.transform.localRotation = Quaternion.identity;

        Undo.RegisterFullObjectHierarchyUndo(newGameObject, "Spawn Create GameObject");
        Undo.SetTransformParent(newGameObject.transform, parent, "Set Parent");
        return newGameObject;
    }
    //������Ʈ ���̴� �۾��� undo�� �����ϵ��� ����
    public static T AddComponent<T>(GameObject target) where T : Component
    {
        return Undo.AddComponent<T>(target);
    }
    //���̿� ���� �浹 �Ǻ���. ���� true��� �� �ݰ濡 ���̰� hit�Ǿ��ٴ� ��
    public static bool SphereHit(Vector3 center, float radius, Ray ray)
    {
        Vector3 originToCenter = ray.origin - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(originToCenter, ray.direction);
        float c = Vector3.Dot(originToCenter, originToCenter) - (radius * radius);
        float discriminant = b * b  - 4f * a * c;
        //���� �浹���� ����
        if(discriminant < 0)
        {
            return false;
        }
        //�浹��
        float sqrt = Mathf.Sqrt(discriminant);
        return -b - sqrt > 0f || -b + sqrt > 0f;
    }
    //���� ���� ������Ʈ ���̾ �����ϴ� �Լ�
    public static void CreateLayer(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name", "���ο� ���̾ �߰��Ϸ��� �̸��� �Է��ϼ���.");
        }

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layerProps = tagManager.FindProperty("layers");
        var propCount = layerProps.arraySize;

        SerializedProperty firstEmptyProp = null;
        for(var i = 0; i < propCount; i++)
        {
            var layerProp = layerProps.GetArrayElementAtIndex(i);
            var stringValue = layerProp.stringValue;
            if (stringValue == name)
            {
                return;
            }
            //builtin, �̹� �ٸ� ���̾ �ڸ��� �����ϰ� �ִٸ�
            if(i<8 || stringValue != string.Empty)
            {
                continue;
            }
            if(firstEmptyProp == null)
            {
                firstEmptyProp = layerProp;
                break;
            }

        }
        if(firstEmptyProp == null)
        {
            Debug.Log($"���̾ �ִ� ������ �����Ͽ����ϴ� �׷��� {name}�� �������� ���Ͽ����ϴ�");
            return;
        }

        firstEmptyProp.stringValue = name;
        tagManager.ApplyModifiedProperties();


    }

    /// <summary>
    /// //GameObject�� ���̾ ������. ���ϸ� �ڽĵ鵵 ���δ� �����մϴ�
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="layer"></param>
    /// <param name="includeChildren"></param>
    /// 
    public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren = false)
    {
        if(!includeChildren)
        {
            gameObject.layer = layer;
            return;
        }
        foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layer;
        }
    }

}
