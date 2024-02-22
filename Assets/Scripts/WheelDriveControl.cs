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
        RearWheelDrive, //�ķ�����.
        FrontWheelDrive, //��������.
        AllWheelDrive, //4������.
    }

    public enum SpeedUnitType
    {
        KMH,
        MPH,
    }
    [Tooltip("������ ����Ǵ� �ٿ�����.")]
    public float downForce = 100f;
    [Tooltip("������ �ִ� ���� ����.")]
    public float maxAngle = 60f;
    [Tooltip("���� �������� �����ϴ� �ӵ�(��������)")]
    public float steeringLerp = 5f;
    [Tooltip("������ ������ �ٲٷ��� �� ���� �ִ� �ӵ�.")]
    public float steeringSpeedMax = 8f;
    [Tooltip("���������� ����Ǵ� �ִ� ��ũ(��).")]
    public float maxTorque = 100f;
    [Tooltip("���������� ����Ǵ� �ִ� �극��ũ ��ũ.")]
    public float breakTorque = 100000f;
    [Tooltip("�ӵ� ����.")]
    public SpeedUnitType unityType = SpeedUnitType.KMH;
    [Tooltip("�ּ� �ӵ� - ���� �� (����/ �극��ũ ����) ������ ������ ������ �ӵ� ����,�ݵ�� 0���� Ŀ���մϴ�.")]
    public float minSpeed = 2f;
    [Tooltip("������ ������ ������ �ִ� �ӵ�.")]
    public float maxSpeed = 10f;
    [Tooltip("������ ���ݶ��̴��� �ڽ����� ���� �ٿ��ݴϴ�. ��ũ �ʿ�.")]
    public GameObject leftWheelShape;
    public GameObject rightWheelShape;
    [Tooltip("������ �ִϸ��̼� ȿ���� ���������� ����.")]
    public bool animateWheels = true;
    [Tooltip("������ ���� ���� : �ķ�, ����, 4��.")]
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
            //�ʿ��� ���� ���� ����� ������.
            if (leftWheelShape != null && wheel.transform.localPosition.x < 0)
            {
                var wheelshape = Instantiate(leftWheelShape);
                wheelshape.transform.parent = wheel.transform;
                wheelshape.transform.localPosition = Vector3.zero;//�̰�.
            }
            else if (rightWheelShape != null && wheel.transform.localPosition.x > 0)
            {
                var wheelshape = Instantiate(rightWheelShape);
                wheelshape.transform.parent = wheel.transform;
                wheelshape.transform.localPosition = Vector3.zero;//�̰�.
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
    //���� �ӵ� ������ ���߾� ���� �ӵ��� ���ɴϴ�.
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
    //�̵��ϸ鼭 ������ ���� ��ɵ� �ְ� ������ٵ� ���� �����ִ� ������� �̵��Ѵ�.
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
            //�չ��� ����
            if (wheel.transform.localPosition.z > 0)
            {
                wheel.steerAngle = angle;
            }
            //�޹��� ����.
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
            //�� Ʈ������ ������ ���� ������ ���� ���� ���������ν� �ִϸ��̼� ȿ��. 
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
            //������ �ݴϴ�.
            float speedUnit = GetSpeedUnit(rb.velocity.magnitude);
            if (speedUnit > maxSpeed)
            {
                rb.velocity = GetSpeedMS(maxSpeed) * rb.velocity.normalized;
            }
            //downforce�� �ݴϴ�.
            rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
        }
    }
}