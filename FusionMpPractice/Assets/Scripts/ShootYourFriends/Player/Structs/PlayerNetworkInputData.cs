using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct PlayerNetworkInputData : INetworkInput
{
    //public NetworkButtons InputButtons;
    //public NetworkId NetworkId;
    public float VariableMoveSpeed;
    public Vector3 MoveDirection;
    public Vector2 MouseVector;
    public NetworkBool JumpButtonGotPressed;    //bool or NetworkBool
    public NetworkBool DuckButtonGotPressed;    //bool or NetworkBool
}