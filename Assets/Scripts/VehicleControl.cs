using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VehicleControl : MonoBehaviour
{
    private WheelDriveControl wheelDriveControl;
    private float initMaxSpeed = 0f;

    //�ڵ����� �̵��� Ÿ�� ������ ����ü
    public struct Target
    {
        public int segment;
        public int waypoint;
    }

    //�ڵ����� ����
    public enum Status
    {
        GO,
        STOP,
        SLOW_DOWN,

    }

    [Header("���� ���� �ý���")]
    [Tooltip("���� Ȱ��ȭ �� ���� �ý���")]
    public TrafficHeadquater trafficHeadquater;
    [Tooltip("������ ��ǥ�� ������ �ñ⸦ Ȯ���մϴ�. ���� ��������Ʈ�� �� ���� ������ �� ����� �� ����. �� ���ڰ� �������� �� ���� �����")]
    public float waypointThresh = 2.5f;

    [Header("���� ���̴�")]
    [Tooltip("���̸� �� ��Ŀ")]
    public Transform raycastAnchor;
    [Tooltip("������ ����")]
    public float raycastLength = 3f;
    [Tooltip("���� ������ ����")]
    public float raycastSpacing = 3f;
    [Tooltip("������ ������ ��")]
    public int raycastNumber = 8;
    [Tooltip("������ ������ �� �Ÿ� �̸��̸� ���� ������")]
    public float emergencyBrakeThresh = 1.5f;
    [Tooltip("������ ������ �� �Ÿ����� �۰ų� ������� �ڵ����� �ӵ��� ������")]
    public float slowDownThresh = 5f;

    public Status vehicleStatus = Status.GO;
    private int pastTargetSegment = -1;
    private Target currentTarget;
    private Target nextTarget;


    // Start is called before the first frame update
    void Start()
    {
        wheelDriveControl = GetComponent<WheelDriveControl>();
        initMaxSpeed = wheelDriveControl.maxSpeed;

        if (raycastAnchor == null && transform.Find("RaycastAnchor") != null )
        {
            raycastAnchor = transform.Find("RaycastAnchor");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float accelation = 1f;
        float brake = 0f;
        float steering = Input.GetAxis("Horizontal"); //������ �ټ�����
        wheelDriveControl.maxSpeed = initMaxSpeed;
        wheelDriveControl.Move(_aceeleration: accelation, steering, brake);
    }
}
