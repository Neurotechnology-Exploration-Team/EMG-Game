using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float motorForce;
    public float breakForce;
    public float maxSteerAngle;

    public Transform fr_Transform;
    public Transform fl_Transform;
    public Transform br_Transform;
    public Transform bl_Transform;

    public WheelCollider fr_Collider;
    public WheelCollider fl_Collider;
    public WheelCollider br_Collider;
    public WheelCollider bl_Collider;

    private float horizontalInput;
    private float verticleInput;
    private float curBreakForce;
    private float curSteerAngle;
    private bool isBreaking;

    private void FixedUpdate() 
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    // Input Controls
    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticleInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    // Motor
    private void HandleMotor()
    {
        fr_Collider.motorTorque = verticleInput * motorForce;
        fl_Collider.motorTorque = verticleInput * motorForce;

        // Brakes
        if(isBreaking)
        {
            curBreakForce = breakForce;
        }else
        {
            curBreakForce = 0f;
        }

        ApplyBreaks();
    }

    // Applys Breaks to Wheels
    private void ApplyBreaks()
    {
        fr_Collider.brakeTorque = curBreakForce;
        fl_Collider.brakeTorque = curBreakForce;
        br_Collider.brakeTorque = curBreakForce;
        bl_Collider.brakeTorque = curBreakForce;
    }
    
    private void HandleSteering()
    {
        curSteerAngle = maxSteerAngle * horizontalInput;
        fr_Collider.steerAngle = curSteerAngle;
        fl_Collider.steerAngle = curSteerAngle;
    }
    private void UpdateWheels()
    {
        UpdateSingleWheel(fr_Collider, fr_Transform);
        UpdateSingleWheel(fl_Collider, fl_Transform);
        UpdateSingleWheel(br_Collider, br_Transform);
        UpdateSingleWheel(bl_Collider, bl_Transform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
