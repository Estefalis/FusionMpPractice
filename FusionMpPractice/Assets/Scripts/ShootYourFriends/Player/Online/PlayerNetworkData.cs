using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkData : INetworkInput
{
    public Vector3 MoveDirection;
    public bool JumpButtonIsPressed;
    public bool KneelButtonIsPressed;
}