using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Transform myTransform = null;

    public float distance = 5f;
    public float height = 1.5f;
    //높이값 변경 속도
    public float heightDamping = 2.0f; 
    //회전값 변경 속도
    public float rotationDamping = 3.0f;
    //타겟
    public Transform target = null;


    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        //타겟이 없다면 Player라는 태그를 가진 게임 오브젝트가 타겟임.
        if(target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    private void LateUpdate()
    {
        if(target == null)
        {
            return;
        }

        //카메라가 목표로 하고 있는 회전 y축 값과 높이값.
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height; 

        //현재 카메라가 바라보고 있는 회전 y축 값과 높이값
        float currentRotationAngle = myTransform.eulerAngles.y;
        float currentHeight = myTransform.position.y;

        //현재 카메라가 바라보고 있는 회전값과 높이값을 보간해서 새로운 값으로 계산
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
        //위에서 계산한 회전값으로 쿼터니언 회전값을 생성
        Quaternion currentRotation = Quaternion.Euler(0.0f, currentRotationAngle, 0.0f);

        //카메라가 타겟의 위치에서 회전하고자하는 벡터만큼 뒤로 이동
        myTransform.position = target.position;
        myTransform.position -= currentRotation * Vector3.forward * distance;
        //이동한 위치에서 원하는 높이값으로 올라감
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        //타겟을 항상 바라보도록함.forward -> target
        myTransform.LookAt(target);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
