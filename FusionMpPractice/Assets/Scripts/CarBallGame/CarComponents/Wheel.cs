using System;
using UnityEngine;

[Serializable]
public class Wheel
{
    public GameObject wheelObj;
    public WheelCollider wheelCollider;
    public WheelType wheelType;

    public bool IsFrontWheel => wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
    public bool IsBackWheel => wheelType == WheelType.BackLeft || wheelType == WheelType.BackRight;
    public bool IsGrounded => wheelCollider.isGrounded;

    public void SynchronizeWheels()
    {
        wheelCollider.GetWorldPose(out var pos, out var rot);
        wheelObj.transform.rotation = rot;
        
    }
    
    public void Brake(float brakePower)
    {
        wheelCollider.brakeTorque = brakePower;
    }
}