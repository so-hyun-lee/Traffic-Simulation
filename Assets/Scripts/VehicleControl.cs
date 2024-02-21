//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public TrafficHeadquarter trafficHeadquarter;
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
        //시작하면 내가 어디있는지, 어디로 가는지 찾고 시작
        SetWaypointVehicleIsOn();
    }

    // Update is called once per frame
    void Update()
    {
        //float accelation = 1f;
        //float brake = 0f;
        //float steering = Input.GetAxis("Horizontal"); //각도를 줄수있음
        //wheelDriveControl.maxSpeed = initMaxSpeed;
        //wheelDriveControl.Move(_aceeleration: accelation, steering, brake);

        if (trafficHeadquarter == null)
        {
            return;
        }

        WayPointChecker();
        MoveVehicle();
    }

    int GetNextSegmentID()
    {
        //hq가 들고 있는 구간 중에 현재 차량이 속해있는 세그먼트가 갖고 있는 다음 구간들을 얻어옵니다.
        List<TrafficSegment> nextSegments = trafficHeadquarter.segments[currentTarget.segment].nextSegments;
        if (nextSegments.Count == 0)
        {
            return 0;
        }

        int randomCount = Random.Range(0, nextSegments.Count - 1);
        return nextSegments[randomCount].ID;
    }

    void SetWaypointVehicleIsOn()
    {
        foreach (var segment in trafficHeadquarter.segments)
        {
            //현재차가 이 구간안에 있는지 확인.
            if (segment.IsOnSegment(transform.position))
            {
                currentTarget.segment = segment.ID;
                //구간 내에서 시작할 가장 가까운 웨이포인트 찾기.
                float minDist = float.MaxValue;
                List<TrafficWaypoint> waypoints = trafficHeadquarter.segments[currentTarget.segment].Waypoints;
                for (int j = 0; j < waypoints.Count; j++)
                {
                    float distance = Vector3.Distance(transform.position,
                        waypoints[j].transform.position);

                    Vector3 lSpace = transform.InverseTransformPoint(waypoints[j].transform.position);
                    if (distance < minDist && lSpace.z > 0f)
                    {
                        minDist = distance;
                        currentTarget.waypoint = j;
                    }
                }

                break;
            }
        }
        //다음  target 찾기.
        nextTarget.waypoint = currentTarget.waypoint + 1;
        nextTarget.segment = currentTarget.segment;
        //위에 지정한 다음 타겟의 waypoint 가 범위를 벗어났다면 다시 처음 0번째 웨이포인트.다음 세그먼트 아이디를 구합니다.
        if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
        {
            nextTarget.waypoint = 0;
            nextTarget.segment = GetNextSegmentID();
        }
    }

    void WayPointChecker()
    {
        GameObject waypoint = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].gameObject;
        //차량을 기준으로 한 다음 웨이포인트와의 위ㅣ를 찾기위해 거리계산
        Vector3 wpDist = transform.InverseTransformPoint(new Vector3(waypoint.transform.position.x,transform.position.y,waypoint.transform.position.z));

        //만약 현재 타겟으로 하고있는 웨이포인트와 일정 거리 이하로 가깝다면
        if (wpDist.magnitude < waypointThresh)
        {
            currentTarget.waypoint++;
            //현재 구간이 갖고 있는 웨이포인트를 다 돌았다면 다음 구간의 웨이포인트로 타겟을 변경
            if (currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
            {
                pastTargetSegment = currentTarget.segment;
                currentTarget.segment = nextTarget.segment;
                currentTarget.waypoint = 0;

                //다음 타겟의 웨이포인트도 찾기
                nextTarget.waypoint = currentTarget.waypoint + 1;
                if(nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
                {
                    nextTarget.waypoint = 0;
                    nextTarget.segment = GetNextSegmentID();
                }
            }
        }
    }

    //레이 캐스팅 함수 -> 충돌 레이어와 자동차 레이어만 레이캐스팅 합니다.
    void CastRay(Vector3 anchor, float angle, Vector3 dir, float length, out GameObject outObstacle, out float outHitDistance)
    {
        outObstacle = null;
        outHitDistance = -1;

        Debug.DrawRay(anchor, Quaternion.Euler(0f, angle, 0f) * dir * length, Color.red);

        //자동차 레이어 얻기
        int layer = 1 << LayerMask.NameToLayer(TrafficHeadquarter.VehicleTagLayer);
        int finalMask = layer;
        
        //추가 충돌체의 레이어가 있으면 추가
        foreach (var layerName in trafficHeadquarter.collisionLayers)
        {
            int id = 1 << LayerMask.NameToLayer(layerName);
            finalMask = finalMask | id;
        }

        RaycastHit hit;
        if (Physics.Raycast(anchor, Quaternion.Euler(0f,angle, 0f)*dir,out hit, length, finalMask))
        {
            outObstacle = hit.collider.gameObject;
            outHitDistance = hit.distance;
        }

    }

    //레이캐스팅해서 충돌체를 얻어오고 거리도 얻어오는 함수
    GameObject GetDetectObstacles(out float hitDist)
    {
        GameObject obstacleObject = null;
        float minDist = 10000f;
        float initRay = (raycastNumber / 2f) * raycastSpacing;
        float hitDistance = -1f;

        for (float a = -initRay; a <= initRay; a += raycastSpacing)
        {
            CastRay(raycastAnchor.transform.position,a,transform.forward, raycastLength,out obstacleObject, out hitDistance);

            if (obstacleObject == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, obstacleObject.transform.position);
            if(dist < minDist)
            {
                minDist = dist;
            }
        }
        hitDist = hitDistance;
        return obstacleObject;

    }

    //이 차략의 구간(segment)을 얻어오는 함수
    public int GetSegmentVehicleIsIn()
    {
        int vehicleSegment = currentTarget.segment;
        bool isOnSegment = trafficHeadquarter.segments[vehicleSegment].IsOnSegment(transform.position);
        if(isOnSegment == false)
        {
            bool isOnPastSegment = trafficHeadquarter.segments[pastTargetSegment].IsOnSegment(transform.position) ;
            if (isOnPastSegment)
            {
                vehicleSegment = pastTargetSegment;
            }
        }

        return vehicleSegment;
    }

    //가능한 한 자율주행을 하고싶음.
    void MoveVehicle()
    {
        //디폴트: 풀악셀, 노브레이크, 노핸들링
        float acc = 1f;
        float brake = 0f;
        float steering = 0f;
        wheelDriveControl.maxSpeed = initMaxSpeed;

        Transform targetTransform = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].transform;
        Transform nextTargetTransform = trafficHeadquarter.segments[nextTarget.segment].Waypoints[nextTarget.waypoint].transform;
        Vector3 nextVector3 = nextTargetTransform.position - targetTransform.position;

        //회전을 해야하는지 계산
        float nextSteering = Mathf.Clamp(transform.InverseTransformDirection(nextVector3.normalized).x,-1,1);
        //만약 차가 서야한다면
        if (vehicleStatus == Status.STOP)
        {
            acc = 0f;
            brake = 1f;
            wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed /2f, 5f);
        }
        else
        {
            //속도를 줄여야하는 경우
            if (vehicleStatus == Status.SLOW_DOWN)
            {
                acc = 0.3f;
                brake = 0f;
            }
            //회전을 해야한다면 속도 조절
            if (nextSteering > 0.3f || nextSteering < -0.3f)
            {
                wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed, wheelDriveControl.steeringSpeepMax);

            }
            //레이캐스트로 감지된 장애물이 있는지 확인
            float hitDist;
            GameObject obstacle = GetDetectObstacles(out hitDist);
            //무언가 충돌이 있을 경우 
            if(obstacle != null)
            {
                WheelDriveControl obstacleVehicle = null;
                obstacleVehicle = obstacle.GetComponent<WheelDriveControl>();

                //자동차라면,(장애물이 wheeldrivecontrol을 가진경우)
                if(obstacleVehicle != null)
                {
                    //앞차인지 확인하기
                    float dotFront = Vector3.Dot(transform.forward,obstacleVehicle.transform.forward);

                    //1. 감지된 앞 차량의 속도가 내 차의 속도보다 낮으면 속도를 줄인다.
                    if (dotFront > 0.8f && obstacleVehicle.maxSpeed < wheelDriveControl.maxSpeed)
                    {
                        float speed = Mathf.Max(wheelDriveControl.GetSpeedMS(obstacleVehicle.maxSpeed) - 0.5f, 0.1f);
                        wheelDriveControl.maxSpeed = wheelDriveControl.GetSpeedUnit(speed);
                    }
                    //2. 두 차량이 너무 가까우면서 같은 방향을 향하고 있으면 일단 멈춘다
                    if (dotFront > 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        //아무리 속도를 줄여도 minSpeed까지만 줄인다.
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed / 2f , wheelDriveControl.minSpeed);
                    }

                    //3. 두 차량이 가까우면서 같은방향이 아닐경우(마주하고 오는 경우)
                    else if (dotFront <= 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = -0.3f;
                        brake = 0f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed /2f, wheelDriveControl.minSpeed);

                        //가까운 차량이 오른쪽에 있을수도, 왼쪽일 수도 있기 떄문에 회전하기
                        float dotRight = Vector3.Dot(transform.forward, obstacleVehicle.transform.forward);
                        //오른쪽
                        if (dotRight > 0.1f)
                        {
                            steering = -0.3f;
                        }
                        //왼쪽
                        else if(dotRight < -0.1f)
                        {
                            steering = 0.3f;
                        }
                        //가운데
                        else
                        {
                            steering = -0.7f;
                        }
                    }

                    //4. 두 차량이 가까워지면 속도를 줄이자
                    else if(hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }

                }



                //장애물이라면,
                else
                {
                    //1. 너무 가까우면 긴급제동
                    if(hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed/2f, wheelDriveControl.minSpeed);
                    }                
                    //2.그렇지 않으면 일정거리 이하로 가까워질 때 천천히 이동
                    else if(hitDist< slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
            }

            //경로를 따르도록 방향을 조정해야하는지 확인
            if (acc > 0f)
            {
                Vector3 nextVector = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].transform.position - transform.position;
                steering = Mathf.Clamp(transform.InverseTransformDirection(nextVector.normalized).x, min:-1, max:1);
            }
        }
        //차량 이동
        wheelDriveControl.Move(acc, steering, brake);


    }

    



}
