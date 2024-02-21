//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public TrafficHeadquarter trafficHeadquarter;
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
        //�����ϸ� ���� ����ִ���, ���� ������ ã�� ����
        SetWaypointVehicleIsOn();
    }

    // Update is called once per frame
    void Update()
    {
        //float accelation = 1f;
        //float brake = 0f;
        //float steering = Input.GetAxis("Horizontal"); //������ �ټ�����
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
        //hq�� ��� �ִ� ���� �߿� ���� ������ �����ִ� ���׸�Ʈ�� ���� �ִ� ���� �������� ���ɴϴ�.
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
            //�������� �� �����ȿ� �ִ��� Ȯ��.
            if (segment.IsOnSegment(transform.position))
            {
                currentTarget.segment = segment.ID;
                //���� ������ ������ ���� ����� ��������Ʈ ã��.
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
        //����  target ã��.
        nextTarget.waypoint = currentTarget.waypoint + 1;
        nextTarget.segment = currentTarget.segment;
        //���� ������ ���� Ÿ���� waypoint �� ������ ����ٸ� �ٽ� ó�� 0��° ��������Ʈ.���� ���׸�Ʈ ���̵� ���մϴ�.
        if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
        {
            nextTarget.waypoint = 0;
            nextTarget.segment = GetNextSegmentID();
        }
    }

    void WayPointChecker()
    {
        GameObject waypoint = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].gameObject;
        //������ �������� �� ���� ��������Ʈ���� ���Ӹ� ã������ �Ÿ����
        Vector3 wpDist = transform.InverseTransformPoint(new Vector3(waypoint.transform.position.x,transform.position.y,waypoint.transform.position.z));

        //���� ���� Ÿ������ �ϰ��ִ� ��������Ʈ�� ���� �Ÿ� ���Ϸ� �����ٸ�
        if (wpDist.magnitude < waypointThresh)
        {
            currentTarget.waypoint++;
            //���� ������ ���� �ִ� ��������Ʈ�� �� ���Ҵٸ� ���� ������ ��������Ʈ�� Ÿ���� ����
            if (currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
            {
                pastTargetSegment = currentTarget.segment;
                currentTarget.segment = nextTarget.segment;
                currentTarget.waypoint = 0;

                //���� Ÿ���� ��������Ʈ�� ã��
                nextTarget.waypoint = currentTarget.waypoint + 1;
                if(nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
                {
                    nextTarget.waypoint = 0;
                    nextTarget.segment = GetNextSegmentID();
                }
            }
        }
    }

    //���� ĳ���� �Լ� -> �浹 ���̾�� �ڵ��� ���̾ ����ĳ���� �մϴ�.
    void CastRay(Vector3 anchor, float angle, Vector3 dir, float length, out GameObject outObstacle, out float outHitDistance)
    {
        outObstacle = null;
        outHitDistance = -1;

        Debug.DrawRay(anchor, Quaternion.Euler(0f, angle, 0f) * dir * length, Color.red);

        //�ڵ��� ���̾� ���
        int layer = 1 << LayerMask.NameToLayer(TrafficHeadquarter.VehicleTagLayer);
        int finalMask = layer;
        
        //�߰� �浹ü�� ���̾ ������ �߰�
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

    //����ĳ�����ؼ� �浹ü�� ������ �Ÿ��� ������ �Լ�
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

    //�� ������ ����(segment)�� ������ �Լ�
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

    //������ �� ���������� �ϰ����.
    void MoveVehicle()
    {
        //����Ʈ: Ǯ�Ǽ�, ��극��ũ, ���ڵ鸵
        float acc = 1f;
        float brake = 0f;
        float steering = 0f;
        wheelDriveControl.maxSpeed = initMaxSpeed;

        Transform targetTransform = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].transform;
        Transform nextTargetTransform = trafficHeadquarter.segments[nextTarget.segment].Waypoints[nextTarget.waypoint].transform;
        Vector3 nextVector3 = nextTargetTransform.position - targetTransform.position;

        //ȸ���� �ؾ��ϴ��� ���
        float nextSteering = Mathf.Clamp(transform.InverseTransformDirection(nextVector3.normalized).x,-1,1);
        //���� ���� �����Ѵٸ�
        if (vehicleStatus == Status.STOP)
        {
            acc = 0f;
            brake = 1f;
            wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed /2f, 5f);
        }
        else
        {
            //�ӵ��� �ٿ����ϴ� ���
            if (vehicleStatus == Status.SLOW_DOWN)
            {
                acc = 0.3f;
                brake = 0f;
            }
            //ȸ���� �ؾ��Ѵٸ� �ӵ� ����
            if (nextSteering > 0.3f || nextSteering < -0.3f)
            {
                wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed, wheelDriveControl.steeringSpeepMax);

            }
            //����ĳ��Ʈ�� ������ ��ֹ��� �ִ��� Ȯ��
            float hitDist;
            GameObject obstacle = GetDetectObstacles(out hitDist);
            //���� �浹�� ���� ��� 
            if(obstacle != null)
            {
                WheelDriveControl obstacleVehicle = null;
                obstacleVehicle = obstacle.GetComponent<WheelDriveControl>();

                //�ڵ������,(��ֹ��� wheeldrivecontrol�� �������)
                if(obstacleVehicle != null)
                {
                    //�������� Ȯ���ϱ�
                    float dotFront = Vector3.Dot(transform.forward,obstacleVehicle.transform.forward);

                    //1. ������ �� ������ �ӵ��� �� ���� �ӵ����� ������ �ӵ��� ���δ�.
                    if (dotFront > 0.8f && obstacleVehicle.maxSpeed < wheelDriveControl.maxSpeed)
                    {
                        float speed = Mathf.Max(wheelDriveControl.GetSpeedMS(obstacleVehicle.maxSpeed) - 0.5f, 0.1f);
                        wheelDriveControl.maxSpeed = wheelDriveControl.GetSpeedUnit(speed);
                    }
                    //2. �� ������ �ʹ� �����鼭 ���� ������ ���ϰ� ������ �ϴ� �����
                    if (dotFront > 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        //�ƹ��� �ӵ��� �ٿ��� minSpeed������ ���δ�.
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed / 2f , wheelDriveControl.minSpeed);
                    }

                    //3. �� ������ �����鼭 ���������� �ƴҰ��(�����ϰ� ���� ���)
                    else if (dotFront <= 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = -0.3f;
                        brake = 0f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed /2f, wheelDriveControl.minSpeed);

                        //����� ������ �����ʿ� ��������, ������ ���� �ֱ� ������ ȸ���ϱ�
                        float dotRight = Vector3.Dot(transform.forward, obstacleVehicle.transform.forward);
                        //������
                        if (dotRight > 0.1f)
                        {
                            steering = -0.3f;
                        }
                        //����
                        else if(dotRight < -0.1f)
                        {
                            steering = 0.3f;
                        }
                        //���
                        else
                        {
                            steering = -0.7f;
                        }
                    }

                    //4. �� ������ ��������� �ӵ��� ������
                    else if(hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }

                }



                //��ֹ��̶��,
                else
                {
                    //1. �ʹ� ������ �������
                    if(hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed/2f, wheelDriveControl.minSpeed);
                    }                
                    //2.�׷��� ������ �����Ÿ� ���Ϸ� ������� �� õõ�� �̵�
                    else if(hitDist< slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
            }

            //��θ� �������� ������ �����ؾ��ϴ��� Ȯ��
            if (acc > 0f)
            {
                Vector3 nextVector = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].transform.position - transform.position;
                steering = Mathf.Clamp(transform.InverseTransformDirection(nextVector.normalized).x, min:-1, max:1);
            }
        }
        //���� �̵�
        wheelDriveControl.Move(acc, steering, brake);


    }

    



}
