using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEditor.Rendering;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

public class VehicleControl : MonoBehaviour
{
    private WheelDriveControl wheelDriveControl;
    private float initMaxSpeed = 0f;
    //�ڵ����� �̵��� Ÿ�� ������ ����ü.
    public struct Target
    {
        public int segment;
        public int waypoint;
        public override string ToString()
        {
            string ret = $"Target : {segment} / {waypoint}";
            return ret;
        }
    }
    //�ڵ����� ����.
    public enum Status
    {
        GO,
        STOP,
        SLOW_DOWN,
    }

    [Header("���� ���� �ý���.")]
    [Tooltip("���� Ȱ��ȭ �� ���� �ý���.")]
    public TrafficHeadquarter trafficHeadquarter;
    [Tooltip("������ ��ǥ�� ������ �ñ⸦ Ȯ���մϴ�. ���� ��������Ʈ�� �� ���� �����ϴµ� ����� �� �ֽ��ϴ�.(�� ���ڰ� ���� ����" +
             "�� ���� ����˴ϴ�.")]
    public float waypointThresh = 2.5f;

    [Header("���� ���̴�")]
    [Tooltip("���̸� �� ��Ŀ.")]
    public Transform raycastAnchor;
    [Tooltip("������ ����")]
    public float raycastLength = 3f;
    [Tooltip("���� ������ ����.")]
    public float raycasySpacing = 3f;
    [Tooltip("������ ������ ��")]
    public int raycastNumber = 8;
    [Tooltip("������ ������ �� �Ÿ� �̸��̸� ���� �����մϴ�.")]
    public float emergencyBrakeThresh = 1.5f;
    [Tooltip("������ ������ �� �Ÿ����� ���ų� �Ÿ����� ���� ��� �ڵ����� �ӵ��� �������ϴ�.")]
    public float slowDownThresh = 5f;

    public Status vehicleStatus = Status.GO;
    private int pastTargetSegment = -1;
    private Target currentTarget;
    private Target nextTarget;


    void Start()
    {
        wheelDriveControl = GetComponent<WheelDriveControl>();
        initMaxSpeed = wheelDriveControl.maxSpeed;

        if (raycastAnchor == null && transform.Find("RaycastAnchor") != null)
        {
            raycastAnchor = transform.Find("RaycastAnchor");
        }
        //�����ϸ� ���� ��� �ִ���, ���� �����ϴ� �� �ѹ� ã�ƺ��ϴ�.
        SetWaypointVehicleIsOn();
    }

    void Update()
    {
        //�׽�Ʈ �ڵ�. �ּ�ó�� �մϴ�.
        //float accelation = 1f;
        //float brake = 0f;
        //float steering = Input.GetAxisRaw("Horizontal");// 0f;
        //wheelDriveControl.maxSpeed = initMaxSpeed;
        //wheelDriveControl.Move(accelation, steering, brake);

        if (trafficHeadquarter == null)
        {
            return;
        }
        //�̵��ؾ��� Ÿ�� ��������Ʈ�� �������, �����ٸ� ���� ��������Ʈ ��������.
        WayPointChecker();
        //���� ����.
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
    //���� ����Ǿ����� ���� ������ ��� ������ ��� ��������Ʈ�� ���� �����ϴ��� ������ �Ǵ�.
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
    //���� �̵��� ��������Ʈ�� üũ�Ͽ� Ÿ���� ����.
    void WayPointChecker()
    {
        GameObject waypoint = trafficHeadquarter.segments[currentTarget.segment].
            Waypoints[currentTarget.waypoint].gameObject;
        //������ �������� �� ���� ��������Ʈ���� ��ġ�� ã�� ���� ��������Ʈ���� �Ÿ� ���.
        Vector3 wpDist = transform.InverseTransformPoint(
            new Vector3(waypoint.transform.position.x,
                transform.position.y,
                waypoint.transform.position.z));
        //���� ���� Ÿ������ �ϰ� �ִ� ��������Ʈ�� ���� �Ÿ� ���Ϸ� �����ٸ�.
        if (wpDist.magnitude < waypointThresh)
        {
            currentTarget.waypoint++;
            //���� ������ ���� �ִ� ��������Ʈ�� �� ���ҵ��� ���� ������ ��������Ʈ�� Ÿ���� ����.
            if (currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
            {
                pastTargetSegment = currentTarget.segment;
                currentTarget.segment = nextTarget.segment;
                currentTarget.waypoint = 0;
            }
            //���� Ÿ���� ��������Ʈ�� ã��.
            nextTarget.waypoint = currentTarget.waypoint + 1; //<--��� �����ּ���. ����.
            if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
            {
                nextTarget.waypoint = 0;
                nextTarget.segment = GetNextSegmentID();
            }
        }

    }
    //���� ĳ���� �Լ�. -> �浹 ���̾�� �ڵ��� ���̾ ���� ĳ�����մϴ�.
    void CastRay(Vector3 anchor, float angle, Vector3 dir, float length, out GameObject outObstacle,
        out float outHitDistance)
    {
        outObstacle = null;
        outHitDistance = -1;

        Debug.DrawRay(anchor, Quaternion.Euler(0f, angle, 0f) * dir * length,
            Color.red);
        //�ϴ� �ڵ��� ���̾.
        int layer = 1 << LayerMask.NameToLayer(TrafficHeadquarter.VehicleTagLayer);
        int finalMask = layer;

        //�߰� �浹ü�� ���̾ ������ �߰�.
        foreach (var layerName in trafficHeadquarter.collisionLayers)
        {
            int id = 1 << LayerMask.NameToLayer(layerName);
            finalMask = finalMask | id;
        }

        RaycastHit hit;
        if (Physics.Raycast(anchor, Quaternion.Euler(0f, angle, 0f) * dir,
            out hit, length, finalMask))
        {
            outObstacle = hit.collider.gameObject;
            outHitDistance = hit.distance;
            //Vector3 hitPosition = hit.point;
        }

    }
    //����ĳ������ �ؼ� �浹ü�� ������ �Ÿ��� ������ �Լ�.
    GameObject GetDetectObstacles(out float hitDist)
    {
        GameObject obstacleObject = null;
        float minDist = 10000f;
        float initRay = (raycastNumber / 2f) * raycasySpacing;
        float hitDistance = -1f;

        for (float a = -initRay; a <= initRay; a += raycasySpacing)
        {
            CastRay(raycastAnchor.transform.position, a, transform.forward,
                raycastLength, out obstacleObject, out hitDistance);

            if (obstacleObject == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, obstacleObject.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        hitDist = hitDistance;
        return obstacleObject;
    }
    //�� ������ ����(segment)�� ������ �Լ�.
    public int GetSegmentVehicleIsIn()
    {
        int vehicleSegment = currentTarget.segment;
        bool isOnSegment = trafficHeadquarter.segments[vehicleSegment].IsOnSegment(transform.position);
        if (isOnSegment == false)
        {
            bool isOnPastSegment = trafficHeadquarter.segments[pastTargetSegment].IsOnSegment(transform.position);
            if (isOnPastSegment)
            {
                vehicleSegment = pastTargetSegment;
            }
        }

        return vehicleSegment;
    }
    //������ ���������� �ϰ� �ͽ��ϴ�.
    void MoveVehicle()
    {
        //�⺻������ Ǯ ����, �� �극��ũ, �� �ڵ鸵.
        float acc = 1f;
        float brake = 0f;
        float steering = 0f;
        wheelDriveControl.maxSpeed = initMaxSpeed;
        if (currentTarget.segment >= trafficHeadquarter.segments.Count ||
            currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].Waypoints.Count)
        {
            Debug.LogError(currentTarget.ToString());
        }
        Transform targetTransform = trafficHeadquarter.segments[currentTarget.segment].Waypoints[currentTarget.waypoint].transform;
        Transform nextTargetTransform = trafficHeadquarter.segments[nextTarget.segment].Waypoints[nextTarget.waypoint].transform;
        Vector3 nextVector3 = nextTargetTransform.position - targetTransform.position;
        //ȸ���� �ؾ��ϴ� �� ���.
        float nextSteering = Mathf.Clamp(transform.InverseTransformDirection(nextVector3.normalized).x,
            -1, 1);
        //���� ���� ���� �Ѵٸ�.
        if (vehicleStatus == Status.STOP)
        {
            acc = 0f;
            brake = 1f;
            wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed / 2f, 5f);
        }
        else
        {
            // - ----- ----------- ---  -----
            //�ӵ��� �ٿ��� �ϴ� ���.
            if (vehicleStatus == Status.SLOW_DOWN)
            {
                acc = 0.3f;
                brake = 0f;
            }
            //ȸ���� �ؾ� �Ѵٸ� �ӵ��� ����.
            if (nextSteering > 0.3f || nextSteering < -0.3f)
            {
                wheelDriveControl.maxSpeed = Mathf.Min(wheelDriveControl.maxSpeed,
                    wheelDriveControl.steeringSpeedMax);
            }
            //2. ����ĳ��Ʈ�� ������ ��ֹ��� �ִ� �� Ȯ��.
            float hitDist;
            GameObject obstacle = GetDetectObstacles(out hitDist);
            // ���� �浹�� �Ǿ��ٸ�.
            if (obstacle != null)
            {
                WheelDriveControl obstacleVehicle = null;
                obstacleVehicle = obstacle.GetComponent<WheelDriveControl>();
                //�ڵ������.
                if (obstacleVehicle != null)
                {
                    //���� ���� ���� Ȯ��.
                    float dotFront = Vector3.Dot(transform.forward,
                        obstacleVehicle.transform.forward);
                    //������ �� ������ �ӵ��� ������ �ӵ����� ������ �ӵ��� ���δ�.
                    if (dotFront > 0.8f && obstacleVehicle.maxSpeed < wheelDriveControl.maxSpeed)
                    {
                        //�ӵ��� ���϶� �ƹ��� �۾Ƶ� 0.1���ٴ� ũ�� ����.
                        float speed = Mathf.Max(
                            wheelDriveControl.GetSpeedMS(obstacleVehicle.maxSpeed) - 0.5f, 0.1f);
                        wheelDriveControl.maxSpeed = wheelDriveControl.GetSpeedUnit(speed);
                    }
                    //�� ������ �ʹ� �����鼭 ���� ������ ���ϰ� ������ �ϴ� �����.
                    if (dotFront > 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1;
                        //�ƹ��� �ӵ��� �ٿ��� �ּҼӵ������� ���Դϴ�.
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed / 2f,
                            wheelDriveControl.minSpeed);
                    }
                    //�� ������ �ʹ� �����鼭 ���� ������ ���ϰ� ���� ���� ��� , ���鿡�� �޷����� ���� �ε����� ������.
                    else if (dotFront <= 0.8f && hitDist < emergencyBrakeThresh)
                    {
                        acc = -0.3f;
                        brake = 0f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed / 2f,
                            wheelDriveControl.minSpeed);
                        //�����̿� �ִ� ������ �����ʿ� ������ �ְ� ���ʿ� ���� �� �ֱ� ������ �׿����� ȸ���� �ϰڽ��ϴ�.
                        float dotRight = Vector3.Dot(transform.forward, obstacleVehicle.transform.forward);
                        //������.
                        if (dotRight > 0.1f)
                        {
                            steering = -0.3f;
                        }
                        //����.
                        else if (dotRight < -0.1f)
                        {
                            steering = 0.3f;
                        }
                        //���.
                        else
                        {
                            steering = -0.7f;
                        }
                    }
                    //�� ������ ��������� �ӵ��� ������.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
                //��ֹ�....
                else
                {
                    //�ʹ� ������ ��� ����.
                    if (hitDist < emergencyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        wheelDriveControl.maxSpeed = Mathf.Max(wheelDriveControl.maxSpeed / 2f,
                            wheelDriveControl.minSpeed);
                    }
                    //�׷��� ������ ���� �Ÿ� ���Ϸ� ��������� õõ�� �̵�.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
            }

            //��θ� �������� ������ �����ؾ��ϴ� �� Ȯ��.
            if (acc > 0f)
            {
                Vector3 nextVector = trafficHeadquarter.segments[currentTarget.segment].Waypoints
                    [currentTarget.waypoint].transform.position - transform.position;
                steering = Mathf.Clamp(transform.InverseTransformDirection(nextVector.normalized).x,
                    -1, 1);
            }
        }
        //���� �̵�.
        wheelDriveControl.Move(acc, steering, brake);
    }












}