using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VehicleControl : MonoBehaviour
{
    private WheelDriveControl wheelDriveControl;
    private float initMaxSpeed = 0f;

    //자동차가 이동할 타겟 데이터 구조체
    public struct Target
    {
        public int segment;
        public int waypoint;
    }

    //자동차의 상태
    public enum Status
    {
        GO,
        STOP,
        SLOW_DOWN,

    }

    [Header("교통 관제 시스템")]
    [Tooltip("현재 활성화 된 교통 시스템")]
    public TrafficHeadquater trafficHeadquater;
    [Tooltip("차량이 목표에 도달한 시기를 확인합니다. 다음 웨이포인트를 더 일찍 예상할 때 사용할 수 있음. 이 숫자가 높을수록 더 빨리 예상됨")]
    public float waypointThresh = 2.5f;

    [Header("감지 레이더")]
    [Tooltip("레이를 쏠 앵커")]
    public Transform raycastAnchor;
    [Tooltip("레이의 길이")]
    public float raycastLength = 3f;
    [Tooltip("레이 사이의 간격")]
    public float raycastSpacing = 3f;
    [Tooltip("생성될 레이의 수")]
    public int raycastNumber = 8;
    [Tooltip("감지된 차량이 이 거리 미만이면 차가 정지함")]
    public float emergencyBrakeThresh = 1.5f;
    [Tooltip("감지된 차량이 이 거리보다 작거나 높을경우 자동차의 속도가 느려짐")]
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
        float steering = Input.GetAxis("Horizontal"); //각도를 줄수있음
        wheelDriveControl.maxSpeed = initMaxSpeed;
        wheelDriveControl.Move(_aceeleration: accelation, steering, brake);
    }
}
