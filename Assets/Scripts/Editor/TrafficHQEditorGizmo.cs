using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;


public static class TrafficHQEditorGizmo 
{
    //화살표를 그림. 대각선 45도방향 양쪽2개
    private static void DrawArrow(Vector3 point, Vector3 forward, float size)
    {
        forward = forward.normalized * size;
        Vector3 left = Quaternion.Euler(0f, 45f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0f, -45f, 0f) * forward;

        Gizmos.DrawLine(point, point + left);
        Gizmos.DrawLine(point, point + right);
    }
    //화살표 그림 타입에 따라 화살표 개수를 얻어옴
    private static int GetArrowCount(Vector3 pointA, Vector3 pointB, TrafficHeadquarter headquarter)
    {
        switch(headquarter.arrowDrawType)
        {
            case TrafficHeadquarter.ArrowDraw.FixedCount:
                return headquarter.arrowCount;
                //break;
            case TrafficHeadquarter.ArrowDraw.ByLength:
                int count = (int)(Vector3.Distance(pointA, pointB) / headquarter.arrowDistance);
                return Mathf.Max(1, count);
               // break;
            case TrafficHeadquarter.ArrowDraw.Off:
                return 0;
            default:
                throw new ArgumentOutOfRangeException();

        }
    }
    //선택되었을 때나 선택되지 않았을 때도 활성화 상태라면 기즈모를 그린다
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawGizmo(TrafficHeadquarter headquarter, GizmoType gizmoType)
    {
        //만약 기즈모를 안그려야한다면 리턴
        if(headquarter.hideGizmos)
        {
            return;
        }
        foreach(TrafficSegment segment in headquarter.segments)
        {
            //세그먼트 이름 출력(ex. Segment-0)
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = new Color(1,0,0)
                },
                fontSize = 16,
            };
            Handles.Label(segment.transform.position, segment.name, style);
            //웨이포인트 그리기
            for (int j = 0; j < segment.Waypoints.Count; j++)
            {
                //현재 웨이포인트의 위치를 찾고
                Vector3 pos = segment.Waypoints[j].GetVisualPos();

                //구를 그리고 방향을 표시하려면 색상을 변경
                Gizmos.color = new Color(0,0,((j+1)/(float)segment.Waypoints.Count),1f);
                Gizmos.DrawSphere(pos, headquarter.waypointSize);

                //다음 웨이포인트 위치 찾고
                Vector3 pNext = Vector3.zero;
                if(j < segment.Waypoints.Count-1 && segment.Waypoints[j+1] != null)
                {
                    pNext = segment.Waypoints[j+1].GetVisualPos() ;
                }

                //다음 웨이포인트가 있다면
                if(pNext != Vector3.zero)
                {
                    //현재 웨이포인트를 추가 중인 세그먼트라면 주황
                    if(segment == headquarter.curSegment)
                    {
                        Gizmos.color = new Color(1f, 0.3f, 0.1f);
                    }
                    //그냥 웨이포인트라면 빨강
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f);
                    }
                    //선택한 구간은 녹색
                    if(Selection.activeGameObject == segment.gameObject)
                    {
                        Gizmos.color = Color.green;
                    }
                    //두 웨이 포인트의 연결선 그리기
                    Gizmos.DrawLine(pos, pNext);
                    //arrowDrawType을 기반으로 화살표 그릴 개수를 얻어와서
                    int arrowDrawCount = GetArrowCount(pos, pNext, headquarter);
                    //화살표를 그리기
                    for(int i = 1; i < arrowDrawCount+1; i++)
                    {
                        Vector3 point = Vector3.Lerp(pos, pNext, (float)i/(arrowDrawCount+1));
                        DrawArrow(point, pos - pNext, headquarter.arrowSizeWaypoint);

                    }
                }
            }

            //세그먼트를 연결하는 선 그리기
            foreach(TrafficSegment nextSegment in segment.nextSegments)
            {
                if(nextSegment != null)
                {
                    Vector3 p1 = segment.Waypoints.Last().GetVisualPos();
                    Vector3 p2 = nextSegment.Waypoints.First().GetVisualPos();

                    //노란색 선으로 그려줌
                    Gizmos.color = new Color(1f, 1f, 0f);
                    Gizmos.DrawLine(p1, p2);
                    if(headquarter.arrowDrawType != TrafficHeadquarter.ArrowDraw.Off)
                    {
                        DrawArrow(point:(p1 + p2) / 2f, forward: p1 - p2, size: headquarter.arrowSizeIntersection);
                    }
                }
            }
        }

    }


}
