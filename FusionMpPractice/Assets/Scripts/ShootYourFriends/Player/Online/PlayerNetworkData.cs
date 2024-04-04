using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkData : INetworkInput
{
    //public NetworkButtons NetworkButtons;
    public Vector3 ForwardVector;
    public Vector3 RightVector;
    public Vector3 RotationVector;
    public bool JumpButtonIsPressed;
    public bool KneelButtonIsPressed;
}