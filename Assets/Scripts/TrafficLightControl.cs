using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightControl : MonoBehaviour
{
    public int lightGroupID;
    public TrafficIntersection intersection;
    //����Ʈ
    private Light pointLight;
    //��� ��Ȳ�� ������ ����
    private float blink = 0f;

    //���� ��ȣ�� ���� ���� ����
    void SetTrafficLightColor()
    {
        //�������϶�
        if(intersection.currentRedLightGroup == lightGroupID)
        {
            pointLight.color = Color.red;
        }
        //��޻�Ȳ�� ��
        else if(intersection.currentRedLightGroup == 0)
        {
            blink = Mathf.Clamp01(blink +Time.deltaTime *2f);
            pointLight.color = new Color(blink, 0f, 0f);
            if(blink >= 1f)
            {
                blink = 0f;
            }
        }
        else
        {
            pointLight.color = Color.green;
        }
    }

    private void Start()
    {
        pointLight = GetComponentInChildren<Light>();
        SetTrafficLightColor();
    }

    private void Update()
    {
        SetTrafficLightColor();
    }

}
