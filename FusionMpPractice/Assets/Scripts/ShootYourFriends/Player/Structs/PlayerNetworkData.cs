using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkData : INetworkInput
{
    public NetworkButtons InputButtons;
    public NetworkId NetworkId;
    public Vector3 MoveDirection;
    public NetworkBool JumpButtonGotPressed;    //NetworkBool
    public NetworkBool DuckButtonGotPressed;   //NetworkBool
}