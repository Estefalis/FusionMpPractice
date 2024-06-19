using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkInputData : INetworkInput
{
    public NetworkButtons InputButtons;
    public NetworkId NetworkId;
    public Vector3 MoveDirection;
    public NetworkBool JumpButtonGotPressed;    //bool or NetworkBool
    public NetworkBool DuckButtonGotPressed;    //bool or NetworkBool
}