using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IntersectionType
{
    NONE = 0,
    STOP,
    TRAFFIC_LIGHT,
    TRAFFIC_SLOW,
    EMERGENCY,
}

public class TrafficIntersection : MonoBehaviour
{
    public IntersectionType IntersectionType = IntersectionType.NONE;
    public int ID = -1;
    //�켱 ���� ����
    public List<TrafficSegment> prioritySegments = new List<TrafficSegment>();
    //��ȣ�� ������ �ʿ��� �Ӽ���
    public float lightDuration = 8f;
    private float lastChangeLightTime = 0f;
    private Coroutine lightRoutine;
    public float lightRepeatRate = 8f;
    public float orangeLightDuration = 2f;
    //������ ����
    public List<TrafficSegment> lightGroup1 =new List<TrafficSegment>();
    public List<TrafficSegment> lightGroup2 =new List<TrafficSegment>();
    //������ ������ �ִ� �ڵ�����
    private List<GameObject> vehiclesQueue = new List<GameObject>();
    private List<GameObject> vehiclesInIntersection = new List<GameObject>();
    private TrafficHeadquarter trafficHeadquarter;
    //���� ������ �׷�
    public int currentRedLightGroup = -1;
    //������ �����ΰ�?
    bool IsRedLightSegment(int vehicleSegment)
    {
        if(currentRedLightGroup == 1)
        {
            foreach(var segment in lightGroup1)
            {
                if(segment.ID == vehicleSegment)
                {
                    return true;
                }               
            }
        }
        else if(currentRedLightGroup == 2)
        {
            foreach(var segment in lightGroup2)
            {
                if (segment.ID == vehicleSegment)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void MoveVehicleQueue()
    {
        //ť�� �ִ� ������ ��ȣ ������ �ƴ� �ڵ������� �̵���Ų��
        List<GameObject>newVehicleQueue = new List<GameObject>(vehiclesQueue);
        foreach(var vehicle in vehiclesQueue)
        {
            VehicleControl vehicleControl = vehicle.GetComponent<VehicleControl>();
            int vehicleSegment = vehicleControl.GetSegmentVehicleIsIn();
            //���� ��ȣ�� ���� ���� �����̶��
            if(IsRedLightSegment(vehicleSegment)== false)
            {
                vehicleControl.vehicleStatus = VehicleControl.Status.GO;
                newVehicleQueue.Remove(vehicle);
            }
        }
        vehiclesQueue = newVehicleQueue;
    }

    void SwitchLights()
    {
        if(currentRedLightGroup == 1)
        {
            currentRedLightGroup = 2;
        }
        else if (currentRedLightGroup == 2)
        {
            currentRedLightGroup = 1;
        }
        else
        {
            currentRedLightGroup = 1;
        }

        //�ٸ� ������ �����̰� �ϱ����� ��ȣ��ȯ �� ���ʵ��� ��ٸ��� ����(��Ȳ��)
        Invoke("MoveVehicleQueue", orangeLightDuration);

    }


    private void Start()
    {
        vehiclesQueue = new List<GameObject>();
        vehiclesInIntersection = new List<GameObject>();
        lastChangeLightTime = Time.time;
    }
    //�ڷ�ƾ���� ��ȣ ���� ȣ��. ��������(lightRepeatRate, lightDuration)
    private IEnumerator OnTrafficLight()
    {
        SwitchLights();
        yield return new WaitForSeconds(lightRepeatRate);
    }


    private void Update()
    {
        switch (IntersectionType)
        {
            //��ȣ�� �����ζ�� ���� �ð� ���� ��ȣ ��ü�� ����
            case IntersectionType.TRAFFIC_LIGHT:
                if(Time.time>lastChangeLightTime + lightDuration)
                {
                    lastChangeLightTime = Time.time;
                    lightRoutine = StartCoroutine("OnTrafficLight");
                }
                break;

            //��� ��Ȳ�̶�� ��ȣ��ü�� ���߰� 0�� �׷����� ������
            case IntersectionType.EMERGENCY:
                if(lightRoutine != null)
                {
                    StopCoroutine(lightRoutine);
                    currentRedLightGroup = 0;
                }
                break;
            case IntersectionType.STOP:
                break;
        }
    }

    bool IsAlreadyInIntersection(GameObject target)
    {
        foreach(var vehicle in vehiclesInIntersection)
        {
            if(vehicle.GetInstanceID() == target.GetInstanceID())
            {
                return true;
            }
        }

        foreach (var vehicle in vehiclesQueue)
        {
            if (vehicle.GetInstanceID() == target.GetInstanceID())
            { 
                return true; 
            }

        }

        return false;

    }
    //�켱 �����ΰ�?
    bool IsPrioritySegment(int vehicleSegment)
    {
        foreach (var segment in prioritySegments)
        {
            if(vehicleSegment == segment.ID)
            {
                return true;
            }
        }
        return false;
    }

    //�켱 ���� ���� Ʈ����
    void TriggerStop(GameObject vehicle)
    {
        VehicleControl vehicleControl = vehicle.GetComponent<VehicleControl>();
        //��������Ʈ�Ӱ谪�� ���� �ڵ����� ��� ���� �Ǵ� �ٷ� ���� ������ ���� �� ����
        int vehicleSegment = vehicleControl.GetSegmentVehicleIsIn();

        if (IsPrioritySegment(vehicleSegment) == false)
        {
            if (vehiclesQueue.Count > 0 || vehiclesInIntersection.Count > 0)
            {
                vehicleControl.vehicleStatus = VehicleControl.Status.STOP;
            }
            //�����ο� ���� ���ٸ�
            else
            {
                vehiclesInIntersection.Add(vehicle);
                vehicleControl.vehicleStatus = VehicleControl.Status.SLOW_DOWN;
            }
        }
        else
        {
            vehicleControl.vehicleStatus = VehicleControl.Status.SLOW_DOWN;
            vehiclesInIntersection.Add(vehicle);
        }

    }

    void ExitStop(GameObject vehicle)
    {
        vehicle.GetComponent<VehicleControl>().vehicleStatus = VehicleControl.Status.GO;
        vehiclesInIntersection.Remove(vehicle);
        vehiclesQueue.Remove(vehicle);

        if (vehiclesQueue.Count > 0 && vehiclesInIntersection.Count == 0)
        {
            vehiclesQueue[0].GetComponent<VehicleControl>().vehicleStatus = VehicleControl.Status.GO;
        }
    }

    void TriggerLight(GameObject vehicle)
    {
        VehicleControl vehicleControl = vehicle.GetComponent<VehicleControl>();
        int vehicleSegment = vehicleControl.GetSegmentVehicleIsIn();

        if (IsRedLightSegment(vehicleSegment))
        {
            vehicleControl.vehicleStatus = VehicleControl.Status.STOP;
            vehiclesQueue.Add(vehicle);
        }
        else
        {
            vehicleControl.vehicleStatus |= VehicleControl.Status.GO;
        }
    }
    //������ ���� �������� ��� �״�� �̵�
    void ExitLight(GameObject vehicle)
    {
        vehicle.GetComponent<VehicleControl>().vehicleStatus = VehicleControl.Status.GO;
    }
    //��� ��Ȳ �߻� Ʈ����
    void TriggerEmergency(GameObject vehicle)
    {
        VehicleControl vehicleControl = vehicle.GetComponent<VehicleControl> ();
        int vehicleSegment = vehicleControl.GetSegmentVehicleIsIn();

        vehicleControl.vehicleStatus = VehicleControl.Status.STOP;
        vehiclesQueue.Add(vehicle);
    }
    //���������ٸ� = ��޻�Ȳ ����
    private void ExitEmergency(GameObject vehicle)
    {
        vehicle.GetComponent<VehicleControl>().vehicleStatus = VehicleControl.Status.GO;
    }
    //Ʈ���� �߻��� 
    private void OnTriggerEnter(Collider other)
    {
        //������ �̹� ��Ͽ� �ִ��� Ȯ���ϰ� �׷��ٸ� ó��x
        //��� ������ ���̶�� ó�� x(������ �� �����ο� ������ �ִ� ���)
       if(IsAlreadyInIntersection(other.gameObject) || Time.timeSinceLevelLoad < 0.5f)
        {
            return;
        }
       //������ �ƴϸ� ����
       if(other.tag.Equals(TrafficHeadquarter.VehicleTagLayer)==false )
        {
            return;
        }
        //�������� Ÿ�Կ� ���� ó���� �и���
        switch (IntersectionType)
        {
            case IntersectionType.STOP:
                TriggerStop(other.gameObject);
                break;
            case IntersectionType.TRAFFIC_LIGHT:
                TriggerLight(other.gameObject);
                break;
            case IntersectionType.EMERGENCY:
                TriggerEmergency(other.gameObject);
                break;
        }
    }
    //Ʈ���ſ��� �������� ���
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals(TrafficHeadquarter.VehicleTagLayer) == false)
        {
            return;
        }

        switch (IntersectionType)
        {
            case IntersectionType.STOP:
                ExitStop(other.gameObject);
                break;
            case IntersectionType.TRAFFIC_LIGHT:
                ExitLight(other.gameObject);
                break;
            case IntersectionType.EMERGENCY:
                ExitEmergency(other.gameObject);
                break;
        }
    }

}
