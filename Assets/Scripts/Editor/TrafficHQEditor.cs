using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TrafficHeadquarter))]


public class TrafficHQEditor : Editor
{
    private TrafficHeadquarter headquarter;
    //웨이포인트 설치 시에 필요한 임시 저장소들
    private Vector3 startPosition;
    private Vector3 lastPoint;
    private TrafficWaypoint lastWaypoint;

    [MenuItem("Component/TrafficTool/Create Traffic System")]           
    private static void CreateTrafficSystem()
    {
        EditorHelper.SetUndoGroup("Create Traffic System");

        GameObject headquarterObject = EditorHelper.CreateGameObject("Traffic Headquarter");
        EditorHelper.AddComponent<TrafficHeadquarter>(headquarterObject);

        GameObject segmentsObject = EditorHelper.CreateGameObject("Segments", headquarterObject.transform);
        GameObject intersectionsObject = EditorHelper.CreateGameObject("Intersections", headquarterObject.transform);

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }

    private void OnEnable()
    {
        headquarter = target as TrafficHeadquarter;
    }
    //웨이 포인트 추가
    private void AddWaypoint(Vector3 position)
    {
        GameObject go = EditorHelper.CreateGameObject("Waypoint-" + headquarter.curSegment.Waypoints.Count, headquarter.curSegment.transform);
       //위치는 내가 클릭한 곳
        go.transform.position = position;
        TrafficWaypoint waypoint= EditorHelper.AddComponent<TrafficWaypoint>(go);
        waypoint.Refresh(headquarter.curSegment.Waypoints.Count, headquarter.curSegment);
        Undo.RecordObject(headquarter.curSegment, "");
        //HQ에 생성한 웨이포인트를 현재 작업중인 세그먼트에 추가함
        headquarter.curSegment.Waypoints.Add(waypoint);
    }

    //세그먼트 추가
    private void AddSegment(Vector3 position)
    {
        int segID = headquarter.segments.Count;
        //Segments라고 만든 빈 게임 오브젝트의 차일드로 세그먼트 게임오브젝트를 생성
        GameObject segGameObject = EditorHelper.CreateGameObject("Segment-" + segID, headquarter.transform.GetChild(0).transform);
        //내가 지금 클릭한 위치에 세그먼트를 이동시킴
        segGameObject.transform.position = position;
        //HQ에 지금 작업중인 세그먼트에 새로만든 세그먼트 스크립트를 연결해줌
        //이후에 추가되는 웨이포인트는 현재 작업중인 세그먼트에 추가됨
        headquarter.curSegment = EditorHelper.AddComponent<TrafficSegment>(segGameObject);
        headquarter.curSegment.ID = segID;
        headquarter.curSegment.Waypoints = new List<TrafficWaypoint>();
        headquarter.curSegment.nextSegments = new List<TrafficSegment>();

        Undo.RecordObject(headquarter, "");
        headquarter.segments.Add(headquarter.curSegment);
    }

    //인터섹션 추가
    private void AddIntersection(Vector3 position)
    {
        int intID = headquarter.intersections.Count;
        GameObject intersection = EditorHelper.CreateGameObject("Intersection-" + intID, headquarter.transform.GetChild(1).transform);
        intersection.transform.position = position;

        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(intersection);
        boxCollider.isTrigger = true;
        TrafficIntersection trafficIntersection = EditorHelper.AddComponent<TrafficIntersection>(intersection);
        trafficIntersection.ID = intID;

        Undo.RecordObject(headquarter, "");
        headquarter.intersections.Add(trafficIntersection);
    }
    //씬에서 직접 설치
    //shift
    //control
    //alt

    private void OnSceneGUI()
    {
        //마우스 클릭 조작이 있었는지 얻어옴
        Event @event = Event.current;
        if(@event == null)
        {
            return;
        }
        //마우스 포지션 위치로 레이를 만들어줌
        Ray ray  = HandleUtility.GUIPointToWorldRay(@event.mousePosition);
        RaycastHit hit;
        //충돌체 검출이 되었고, 마우스 왼쪽클릭으로 발생
        //0왼쪽 1 오른쪽 2 휠
        if (Physics.Raycast(ray, out hit) && @event.type == EventType.MouseDown && @event.button == 0)
        {
            if (@event.shift)
            {


                //마우스 왼쪽 + shift ->웨이포인트 추가
                if (headquarter.curSegment == null)
                {
                    Debug.LogWarning("세그먼트를 먼저 만드세요");
                    return;
                }
                EditorHelper.BeginUndoGroup("Add WayPoint", headquarter);
                AddWaypoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            //마우스 왼쪽 + control -> 세그먼트 추가
            else if (@event.control)
            {
                EditorHelper.BeginUndoGroup("Add Segment", headquarter);
                AddSegment(hit.point);
                AddWaypoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            //마우스 왼쪽 + alt ->인터섹션 추가
            else if (@event.alt)
            {
                EditorHelper.BeginUndoGroup("Add Intersection", headquarter);
                AddIntersection(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }

        //웨이포인트 시스템을 하이어라키뷰에서 선택한 게임 객체로 설정
        Selection.activeGameObject = headquarter.gameObject;
        //선택한 웨이포인트를 처리
        if(lastWaypoint != null)
        {
            //레이가 충돌할 수 있도록 plane을 사용
            Plane plane = new Plane(Vector3.up, lastWaypoint.GetVisualPos());
            plane.Raycast(ray, out float dst);
            Vector3 hitPoint = ray.GetPoint(dst);
            //마우스 버튼을 처음 눌렀을 때 lastpoint 재설정
            if(@event.type == EventType.MouseDown && @event.button == 0)
            {
                lastPoint = hitPoint;
                startPosition = lastWaypoint.transform.position;
            }
            //선택한 웨이 포인트를 이동
            if(@event.type == EventType.MouseDrag && @event.button == 0)
            {
                Vector3 realPos = new Vector3(hitPoint.x - lastPoint.x, 0, hitPoint.z - lastPoint.z);
                lastWaypoint.transform.position += realPos;
                lastPoint = hitPoint;
            }
            //선택한 웨이포인트를 해제
            if(@event.type == EventType.MouseUp && @event.button == 0)
            {
                Vector3 curPos = lastWaypoint.transform.position;
                lastWaypoint.transform.position = startPosition;
                Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move Waypoint");
                lastWaypoint.transform.position = curPos;
            }
            //구 하나 그리기

            Handles.SphereHandleCap(0,lastWaypoint.GetVisualPos(), Quaternion.identity, headquarter.waypointSize*2f, EventType.Repaint);
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            SceneView.RepaintAll();

        }
        //모든 웨이포인트로부터 판별식을 통해 충돌되는 웨이포인트를 lastWaypoint로 세팅
        if(lastWaypoint == null)
        {
            lastWaypoint = headquarter.GetAllWaypoints().FirstOrDefault(i => EditorHelper.SphereHit(i.GetVisualPos(), headquarter.waypointSize, ray)); ;

        }
        //hq의 현재 수정중인 세그먼트를 현재 선택한 세그먼트로 대체
        if(lastWaypoint != null && @event.type == EventType.MouseDown)
        {
            headquarter.curSegment = lastWaypoint.segment;
        }
        //현재 웨이포인트를 재설정. 마우스를 이동하면 선택이 풀림
        else if (lastWaypoint != null && @event.type == EventType.MouseMove)
        {
            lastWaypoint = null;
        }




    }
    //시뮬레이터 세팅 마무리에 재설정을 하는 기능
    void RestructureSystem()
    {
        //구간과 웨이포인트의 이름 바꾸기, 구조를 조정
        List<TrafficSegment> segmentsList = new List<TrafficSegment>();
        int itSeg = 0;
        foreach(Transform trans in headquarter.transform.GetChild(0).transform)
        {
            TrafficSegment segment = trans.GetComponent<TrafficSegment>();
            if(segment != null)
            {
                List<TrafficWaypoint>waypointList = new List<TrafficWaypoint>();
                segment.ID = itSeg;
                segment.gameObject.name = "Segment-" + itSeg;

                int itWay = 0;
                foreach(Transform trans2 in segment.transform)
                {
                   TrafficWaypoint waypoint = trans2.GetComponent<TrafficWaypoint>();
                    if(waypoint != null)
                    {
                        waypoint.Refresh(itWay, segment);
                        waypointList.Add(waypoint);
                        itWay++;
                    }
                }
                segment.Waypoints = waypointList;
                segmentsList.Add(segment);
                itSeg++;


            }
        }
        //세그먼트도 마찬가지로 리스트에서 삭제된 건 빼주고 재정렬
        foreach(TrafficSegment segment in segmentsList)
        {
            List<TrafficSegment> nextSegmentsList = new List<TrafficSegment>();
            foreach(TrafficSegment nextSegment in segment.nextSegments)
            {
                if(nextSegment != null)
                {
                    nextSegmentsList.Add(nextSegment);
                }
            }
            segment.nextSegments = nextSegmentsList;    
        }
        //새로만든 세그먼트 리스트를 다시 hq에 할당
        headquarter.segments = segmentsList;
        //교차로도
        List<TrafficIntersection> intersectionList = new List<TrafficIntersection>();
        int itInter = 0;
        foreach(Transform transInter in headquarter.transform.GetChild(1).transform)
        {
            TrafficIntersection intersection = transInter.GetComponent<TrafficIntersection>();
            if(intersection != null)
            {
                intersection.ID = itInter;
                intersection.gameObject.name = "Intersection-" + itInter;
                intersectionList.Add(intersection);
                itInter++;
            }
        }
        headquarter.intersections = intersectionList;

        //변경된 게 있으면 씬도 새로 그리고 저장할지 물어보는 식으로 
        if (!EditorUtility.IsDirty(target))
        {
            EditorUtility.SetDirty(target);
        }
        Debug.Log("[교통시뮬레이션] 성공적으로 재빌드하였습니다");

    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        {
            Undo.RecordObject(headquarter, "Traffic Inspector Edit");
            TrafficHQInspectorEditor.DrawInspector(headquarter, serializedObject, out bool restructureSystem);

            if (restructureSystem)
            {
                RestructureSystem();
            }
        }
        //값이 편집된 경우 씬을 아예 다시그리기
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        serializedObject.ApplyModifiedProperties();
        EditorGUI.EndChangeCheck();
    }


}
