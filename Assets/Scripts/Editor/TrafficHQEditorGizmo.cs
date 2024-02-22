using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;


public static class TrafficHQEditorGizmo 
{
    //ȭ��ǥ�� �׸�. �밢�� 45������ ����2��
    private static void DrawArrow(Vector3 point, Vector3 forward, float size)
    {
        forward = forward.normalized * size;
        Vector3 left = Quaternion.Euler(0f, 45f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0f, -45f, 0f) * forward;

        Gizmos.DrawLine(point, point + left);
        Gizmos.DrawLine(point, point + right);
    }
    //ȭ��ǥ �׸� Ÿ�Կ� ���� ȭ��ǥ ������ ����
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
    //���õǾ��� ���� ���õ��� �ʾ��� ���� Ȱ��ȭ ���¶�� ����� �׸���
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawGizmo(TrafficHeadquarter headquarter, GizmoType gizmoType)
    {
        //���� ����� �ȱ׷����Ѵٸ� ����
        if(headquarter.hideGizmos)
        {
            return;
        }
        foreach(TrafficSegment segment in headquarter.segments)
        {
            //���׸�Ʈ �̸� ���(ex. Segment-0)
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = new Color(1,0,0)
                },
                fontSize = 16,
            };
            Handles.Label(segment.transform.position, segment.name, style);
            //��������Ʈ �׸���
            for (int j = 0; j < segment.Waypoints.Count; j++)
            {
                //���� ��������Ʈ�� ��ġ�� ã��
                Vector3 pos = segment.Waypoints[j].GetVisualPos();

                //���� �׸��� ������ ǥ���Ϸ��� ������ ����
                Gizmos.color = new Color(0,0,((j+1)/(float)segment.Waypoints.Count),1f);
                Gizmos.DrawSphere(pos, headquarter.waypointSize);

                //���� ��������Ʈ ��ġ ã��
                Vector3 pNext = Vector3.zero;
                if(j < segment.Waypoints.Count-1 && segment.Waypoints[j+1] != null)
                {
                    pNext = segment.Waypoints[j+1].GetVisualPos() ;
                }

                //���� ��������Ʈ�� �ִٸ�
                if(pNext != Vector3.zero)
                {
                    //���� ��������Ʈ�� �߰� ���� ���׸�Ʈ��� ��Ȳ
                    if(segment == headquarter.curSegment)
                    {
                        Gizmos.color = new Color(1f, 0.3f, 0.1f);
                    }
                    //�׳� ��������Ʈ��� ����
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f);
                    }
                    //������ ������ ���
                    if(Selection.activeGameObject == segment.gameObject)
                    {
                        Gizmos.color = Color.green;
                    }
                    //�� ���� ����Ʈ�� ���ἱ �׸���
                    Gizmos.DrawLine(pos, pNext);
                    //arrowDrawType�� ������� ȭ��ǥ �׸� ������ ���ͼ�
                    int arrowDrawCount = GetArrowCount(pos, pNext, headquarter);
                    //ȭ��ǥ�� �׸���
                    for(int i = 1; i < arrowDrawCount+1; i++)
                    {
                        Vector3 point = Vector3.Lerp(pos, pNext, (float)i/(arrowDrawCount+1));
                        DrawArrow(point, pos - pNext, headquarter.arrowSizeWaypoint);

                    }
                }
            }

            //���׸�Ʈ�� �����ϴ� �� �׸���
            foreach(TrafficSegment nextSegment in segment.nextSegments)
            {
                if(nextSegment != null)
                {
                    Vector3 p1 = segment.Waypoints.Last().GetVisualPos();
                    Vector3 p2 = nextSegment.Waypoints.First().GetVisualPos();

                    //����� ������ �׷���
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
