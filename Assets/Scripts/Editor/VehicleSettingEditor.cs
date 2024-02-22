using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VehicleSettingEditor :Editor
{
    private static void SetupWheelCollider(WheelCollider collider)
    {
        collider.mass = 20f;
        collider.radius = 0.175f;
        collider.wheelDampingRate = 0.25f;
        collider.suspensionDistance = 0.05f;
        collider.forceAppPointDistance = 0f;

        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = 70000f;
        jointSpring.spring = 3500f;
        jointSpring.targetPosition = 1f;
        collider.suspensionSpring = jointSpring;

        WheelFrictionCurve frictionCurve = new WheelFrictionCurve();
        frictionCurve.extremumSlip = 1f;
        frictionCurve.extremumValue = 1f;
        frictionCurve.asymptoteSlip = 1f;
        frictionCurve.asymptoteValue = 1f;
        frictionCurve.stiffness = 1f;
        collider.forwardFriction = frictionCurve;
        collider.sidewaysFriction = frictionCurve;
    }

    [MenuItem("Component/TrafficTool/Setup Vehicle")]
    private static void SetupVehicle()
    {
        EditorHelper.SetUndoGroup("Setup Vehicle");
        //���� ���� �����ߴٸ�(���� ���� x)
        GameObject selected = Selection.activeGameObject;
        //�������� �����հ� ������ ���ܼ� ������� ������ ����
        PrefabUtility.UnpackPrefabInstance(selected, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

     //1.����ĳ��Ʈ ��Ŀ�����
        GameObject anchor = EditorHelper.CreateGameObject("RaycastAnchor", selected.transform);
        anchor.transform.localPosition = Vector3.zero;
        anchor.transform.localRotation = Quaternion.identity;
     //2.��ũ��Ʈ�� ����
        VehicleControl vehicleControl = EditorHelper.AddComponent<VehicleControl>(selected);
        vehicleControl.raycastAnchor = anchor.transform;
     //3.���� �޽� ���̱�
        Transform tireBackLeft = selected.transform.Find("Tire BackLeft");
        Transform tireBackRight = selected.transform.Find("Tire BackRight");
        Transform tireFrontLeft = selected.transform.Find("Tire FrontLeft");
        Transform tireFrontRight= selected.transform.Find("Tire FrontRight");
        
     //4.�� �ݶ��̴� ����
        GameObject backLeftWheel = EditorHelper.CreateGameObject("TireBackLeft Wheel", selected.transform);
        backLeftWheel.transform.position = tireBackLeft.position;
        GameObject backRightWheel = EditorHelper.CreateGameObject("TireBackRight Wheel", selected.transform);
        backRightWheel.transform.position = tireBackRight.position;
        GameObject frontLeftWheel = EditorHelper.CreateGameObject("TireFrontLeft Wheel", selected.transform);
        frontLeftWheel.transform.position = tireFrontLeft.position;
        GameObject frontRightWheel = EditorHelper.CreateGameObject("TireFrontRight Wheel", selected.transform);
        frontRightWheel.transform.position = tireFrontRight.position;

        WheelCollider wheelCollider1 = EditorHelper.AddComponent<WheelCollider>(backLeftWheel);
        WheelCollider wheelCollider2 = EditorHelper.AddComponent<WheelCollider>(backRightWheel);
        WheelCollider wheelCollider3 = EditorHelper.AddComponent<WheelCollider>(frontLeftWheel);
        WheelCollider wheelCollider4 = EditorHelper.AddComponent<WheelCollider>(frontRightWheel);

        SetupWheelCollider(wheelCollider1);
        SetupWheelCollider(wheelCollider2);
        SetupWheelCollider(wheelCollider3);
        SetupWheelCollider(wheelCollider4);

        tireBackLeft.parent = backLeftWheel.transform;
        tireBackLeft.localPosition = Vector3.zero;
        tireBackRight.parent = backRightWheel.transform;
        tireBackRight.localPosition = Vector3.zero;
        tireFrontLeft.parent = frontLeftWheel.transform;
        tireFrontLeft.localPosition = Vector3.zero;
        tireFrontRight.parent = frontRightWheel.transform;
        tireFrontRight.localPosition = Vector3.zero;

     //4.5 WheelDriveControl ��ũ��Ʈ �ٿ��ֱ�
        WheelDriveControl wheelDriveControl = EditorHelper.AddComponent<WheelDriveControl>(selected);
        wheelDriveControl.Init();


     //5.������ٵ� ����
        Rigidbody rb = selected.GetComponent<Rigidbody>();
        rb.mass = 900f;
        rb.drag = 0.1f;
        rb.angularDrag = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;


     //6.������� ����
        TrafficHeadquarter headquarter = FindObjectOfType<TrafficHeadquarter>();
        if(headquarter != null)
        {
            vehicleControl.trafficHeadquarter = headquarter;
        }

        //7.�ٵ� �ݶ��̴� ���̱�
        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(selected);
        boxCollider.isTrigger = true;

        GameObject colliders = EditorHelper.CreateGameObject("Colliders", selected.transform);
        colliders.transform.localPosition = Vector3.zero;
        colliders.transform.localRotation = Quaternion.identity;
        colliders.transform.localScale = Vector3.one;

        GameObject Body = EditorHelper.CreateGameObject("Body", colliders.transform);
        Body.transform.localPosition = Vector3.zero;
        Body.transform.localRotation = Quaternion.identity;
        Body.transform.localScale = Vector3.one;

        BoxCollider bodyCollider = EditorHelper.AddComponent<BoxCollider>(Body);
        bodyCollider.center = new Vector3(0f, 0.4f, 0f);
        bodyCollider.size = new Vector3(0.95f, 0.54f, 2.0f);
        //���� ���̾ ���ٸ� ������ �߰�
        EditorHelper.CreateLayer(TrafficHeadquarter.VehicleTagLayer);
        selected.tag = TrafficHeadquarter.VehicleTagLayer;
        EditorHelper.SetLayer(selected, LayerMask.NameToLayer(TrafficHeadquarter.VehicleTagLayer),true);
        //undo �׷� ������ ����
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());    


     //8.���̾���� �ڵ����� ���̾�.AutonomousVehicle set

    }
}

