using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSegment : MonoBehaviour
{
    //'다음에 이동할 구간(세그먼트)들
    public List<TrafficSegment> nextSegments;
    //이 세그먼트의 id값
    public int ID = -1;
    //구간이 갖고있는 웨이포인트들 시작->끝, 2-3개를 보통 가지고 있다.
    public List<TrafficWaypoint> Waypoints = new List<TrafficWaypoint>();

    public bool IsOnSegment(Vector3 pos)
    {
        TrafficHeadquarter headquater = GetComponentInParent<TrafficHeadquarter>();

        for(int i = 0; i<Waypoints.Count-1; i++)
        {
            Vector3 pos1 = Waypoints[i].transform.position;
            Vector3 pos2 = Waypoints[i+1].transform.position;
            //첫번째 웨이포인트와 차의 거리
            float d1 = Vector3.Distance(pos1, pos);
            //2번째 웨이포인트와 차의 거리
            float d2 = Vector3.Distance(pos2, pos);
            //1-2 웨이포인트 간의 거리
            float d3 = Vector3.Distance(pos1, pos2);

            float diff = (d1 + d2) - d3;
            if (diff <headquater.segDetectThresh && diff > headquater.segDetectThresh)
            {
                //자동차가 두 웨이포인트 사이에 가까이있음
                return true;
            }
        }

        //자동차가 두 웨이포인트 사이에서 멀리있다. for문 밖에 있어야함.
        return false;
    }
}
