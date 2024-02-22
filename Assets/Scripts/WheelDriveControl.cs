using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
public class WheelDriveControl : MonoBehaviour
{
    public enum DriveType
    {
        RearWheelDrive, //후륜구동.
        FrontWheelDrive, //전륜구동.
        AllWheelDrive, //4륜구동.
    }

    public enum SpeedUnitType
    {
        KMH,
        MPH,
    }
    [Tooltip("차량에 적용되는 다운포스.")]
    public float downForce = 100f;
    [Tooltip("바퀴의 최대 조향 각도.")]
    public float maxAngle = 60f;
    [Tooltip("조향 각도각에 도달하는 속도(선형보간)")]
    public float steeringLerp = 5f;
    [Tooltip("차량이 방향을 바꾸려고 할 때의 최대 속도.")]
    public float steeringSpeedMax = 8f;
    [Tooltip("구동바퀴에 적용되는 최대 토크(힘).")]
    public float maxTorque = 100f;
    [Tooltip("구동바퀴에 적용되는 최대 브레이크 토크.")]
    public float breakTorque = 100000f;
    [Tooltip("속도 단위.")]
    public SpeedUnitType unityType = SpeedUnitType.KMH;
    [Tooltip("최소 속도 - 주행 시 (정지/ 브레이크 제외) 단위는 위에서 선택한 속도 단위,반드시 0보다 커야합니다.")]
    public float minSpeed = 2f;
    [Tooltip("위에서 선택한 단위의 최대 속도.")]
    public float maxSpeed = 10f;
    [Tooltip("바퀴는 휠콜라이더의 자식으로 따로 붙여줍니다. 링크 필요.")]
    public GameObject leftWheelShape;
    public GameObject rightWheelShape;
    [Tooltip("바퀴에 애니메이션 효과를 적용할지의 여부.")]
    public bool animateWheels = true;
    [Tooltip("차량의 구동 유형 : 후륜, 전륜, 4륜.")]
    public DriveType driveType = DriveType.RearWheelDrive;

    private WheelCollider[] wheels;
    private float currentSteering = 0f;
    private Rigidbody rb;

    public void Init()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();

        for (int i = 0; i < wheels.Length; i++)
        {
            var wheel = wheels[i];
            //필요할 때만 바퀴 모양을 만들자.
            if (leftWheelShape != null && wheel.transform.localPosition.x < 0)
            {
                var wheelshape = Instantiate(leftWheelShape);
                wheelshape.transform.parent = wheel.transform;
                wheelshape.transform.localPosition = Vector3.zero;//이거.
            }
            else if (rightWheelShape != null && wheel.transform.localPosition.x > 0)
            {
                var wheelshape = Instantiate(rightWheelShape);
                wheelshape.transform.parent = wheel.transform;
                wheelshape.transform.localPosition = Vector3.zero;//이거.
            }
            wheel.ConfigureVehicleSubsteps(10, 1, 1);
        }

    }
    private void Awake()
    {
        Init();
    }
    /*private void OnEnable()
    {
        Init();
    }*/
    //현재 속도 단위의 맞추어 현재 속도를 얻어옵니다.
    public float GetSpeedMS(float speed)
    {
        if (speed == 0f)
        {
            return 0f;
        }

        return unityType == SpeedUnitType.KMH ? speed / 3.6f : speed / 2.237f;
    }
    public float GetSpeedUnit(float speed)
    {
        return unityType == SpeedUnitType.KMH ? speed * 3.6f : speed * 2.237f;
    }
    //이동하면서 바퀴의 조향 기능도 있고 리지드바디에 힘을 가해주는 기능으로 이동한다.
    public void Move(float _acceleration, float _steering, float _brake)
    {
        float nSteering = Mathf.Lerp(currentSteering, _steering, Time.deltaTime * steeringLerp);
        currentSteering = nSteering;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        float angle = maxAngle * nSteering;
        float torque = maxTorque * _acceleration;
        float handBrake = _brake > 0f ? breakTorque : 0f;

        foreach (var wheel in wheels)
        {
            //앞바퀴 조향
            if (wheel.transform.localPosition.z > 0)
            {
                wheel.steerAngle = angle;
            }
            //뒷바퀴 조향.
            if (wheel.transform.localPosition.z < 0)
            {
                wheel.brakeTorque = handBrake;
            }

            if (wheel.transform.localPosition.z < 0 &&
                driveType != DriveType.FrontWheelDrive)
            {
                wheel.motorTorque = torque;
            }

            if (wheel.transform.localPosition.z > 0 &&
                driveType != DriveType.RearWheelDrive)
            {
                wheel.motorTorque = torque;
            }
            //휠 트랜스폼 정보를 위에 세팅한 값에 따라 변경함으로써 애니메이션 효과. 
            if (animateWheels)
            {
                Quaternion rotation;
                Vector3 pos;
                wheel.GetWorldPose(out pos, out rotation);

                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = pos;
                shapeTransform.rotation = rotation;
            }
        }

        if (rb != null)
        {
            //가속을 줍니다.
            float speedUnit = GetSpeedUnit(rb.velocity.magnitude);
            if (speedUnit > maxSpeed)
            {
                rb.velocity = GetSpeedMS(maxSpeed) * rb.velocity.normalized;
            }
            //downforce를 줍니다.
            rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
        }
    }
}