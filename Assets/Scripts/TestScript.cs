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
        //타겟이 없으면 동작x
        if (target == null)
        {
            return;
        }
        //큐브 transform
        Vector3 lhs = transform.forward;
        //타겟으로 향하는 벡터, 크기를 노멀라이즈하여 방향을 얻어냄
        Vector3 rhs = (target.position - transform.position).normalized;
        //내적을 구함 최대 1, 최소 -1
        float dot = Mathf.Clamp(value:Vector3.Dot(lhs, rhs), min:-1, max:1);
        //타겟 포지션으로 부터의 역벡터
        Vector3 lineVector = transform.InverseTransformPoint(target.position);

        //레이를 그리기. 타겟으로 향하는 레이, 큐브의 forward를 나타내는 레이
        Debug.DrawRay(start: transform.position, dir: lineVector, Color.red);
        Debug.DrawRay(start: transform.position, dir: transform.forward, Color.cyan);
        //텍스트로 내적의 값을 출력
        textLabel.text = dot.ToString(format :"F1");


    }
}
