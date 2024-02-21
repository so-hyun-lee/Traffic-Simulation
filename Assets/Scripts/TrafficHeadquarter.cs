using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficHeadquarter : MonoBehaviour
{
    //���׸�Ʈ�� ���׸�Ʈ ������ ���� ����
    public float segDetectThresh = 0.1f;
    //��������Ʈ ũ��
    public float waypointSize = 0.5f;
    //�浹 ���̾��
    public string[] collisionLayers;

    public List<TrafficSegment> segments = new List<TrafficSegment>();
    public TrafficSegment curSegment;

    public const string VehicleTagLayer = "AutonomouseVehicle"; //����������
    //�����ε�
    public List<TrafficIntersection> intersections = new List<TrafficIntersection>();

    //�����Ϳ�, ����� �Ӽ���
    public enum ArrowDraw
    {
        FixedCount,
        ByLength,
        Off
    }

    public bool hideGizmos = false;
    public ArrowDraw arrowDrawType = ArrowDraw.ByLength;
    public int arrowCount = 1;
    public float arrowDistance = 5f;
    public float arrowSizeWaypoint = 1;
    public float arrowSizeIntersection = 0.5f;



    public List<TrafficWaypoint> GetAllWaypoints()
    {
        List<TrafficWaypoint> waypoints = new List<TrafficWaypoint>();
        foreach(var segment in segments)
        {
            waypoints.AddRange(segment.Waypoints);
        }
        return waypoints;
    }

}
