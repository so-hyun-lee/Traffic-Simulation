using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSegment : MonoBehaviour
{
    //'������ �̵��� ����(���׸�Ʈ)��
    public List<TrafficSegment> nextSegments;
    //�� ���׸�Ʈ�� id��
    public int ID = -1;
    //������ �����ִ� ��������Ʈ�� ����->��, 2-3���� ���� ������ �ִ�.
    public List<TrafficWaypoint> Waypoints = new List<TrafficWaypoint>();

    public bool IsOnSegment(Vector3 pos)
    {
        TrafficHeadquarter headquater = GetComponentInParent<TrafficHeadquarter>();

        for(int i = 0; i<Waypoints.Count-1; i++)
        {
            Vector3 pos1 = Waypoints[i].transform.position;
            Vector3 pos2 = Waypoints[i+1].transform.position;
            //ù��° ��������Ʈ�� ���� �Ÿ�
            float d1 = Vector3.Distance(pos1, pos);
            //2��° ��������Ʈ�� ���� �Ÿ�
            float d2 = Vector3.Distance(pos2, pos);
            //1-2 ��������Ʈ ���� �Ÿ�
            float d3 = Vector3.Distance(pos1, pos2);

            float diff = (d1 + d2) - d3;
            if (diff <headquater.segDetectThresh && diff > headquater.segDetectThresh)
            {
                //�ڵ����� �� ��������Ʈ ���̿� ����������
                return true;
            }
        }

        //�ڵ����� �� ��������Ʈ ���̿��� �ָ��ִ�. for�� �ۿ� �־����.
        return false;
    }
}
