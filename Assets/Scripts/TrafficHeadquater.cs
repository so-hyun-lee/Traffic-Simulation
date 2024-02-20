using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficHeadquater : MonoBehaviour
{
    //세그먼트와 세그먼트 사이의 검출 간격
    public float segDetectThresh = 0.1f;
    //웨이포인트 크기
    public float waypointSize = 0.5f;
    //충돌 레이어들
    public string[] collisionLayers;

    public List<TrafficSegment> Segments = new List<TrafficSegment>();
    public TrafficSegment curSegment;

    public List<TrafficWaypoint> GetAllWaypoints()
    {
        List<TrafficWaypoint> waypoints = new List<TrafficWaypoint>();
        foreach(var segment in Segments)
        {
            waypoints.AddRange(segment.Waypoints);
        }
        return waypoints;
    }

}
