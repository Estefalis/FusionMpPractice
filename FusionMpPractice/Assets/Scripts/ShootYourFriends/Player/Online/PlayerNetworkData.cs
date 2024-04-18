using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkData : INetworkInput
{
    //public NetworkButtons InputButtons;
    public Vector3 MoveDirection;
    public bool JumpButtonGotPressed;   //NetworkBool?
    public bool KneelButtonGotPressed;  //NetworkBool?
}