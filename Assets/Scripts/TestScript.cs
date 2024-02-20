using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestScript : MonoBehaviour
{
    public TextMeshProUGUI textLabel;
    //sphere
    public Transform target;


    // Update is called once per frame
    void Update()
    {
        //Ÿ���� ������ ����x
        if (target == null)
        {
            return;
        }
        //ť�� transform
        Vector3 lhs = transform.forward;
        //Ÿ������ ���ϴ� ����, ũ�⸦ ��ֶ������Ͽ� ������ ��
        Vector3 rhs = (target.position - transform.position).normalized;
        //������ ���� �ִ� 1, �ּ� -1
        float dot = Mathf.Clamp(value:Vector3.Dot(lhs, rhs), min:-1, max:1);
        //Ÿ�� ���������� ������ ������
        Vector3 lineVector = transform.InverseTransformPoint(target.position);

        //���̸� �׸���. Ÿ������ ���ϴ� ����, ť���� forward�� ��Ÿ���� ����
        Debug.DrawRay(start: transform.position, dir: lineVector, Color.red);
        Debug.DrawRay(start: transform.position, dir: transform.forward, Color.cyan);
        //�ؽ�Ʈ�� ������ ���� ���
        textLabel.text = dot.ToString(format :"F1");


    }
}
