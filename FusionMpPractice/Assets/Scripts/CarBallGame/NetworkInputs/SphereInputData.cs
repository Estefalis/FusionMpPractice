using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SphereInputData: INetworkInput
{
    public float HorizontalValue;
    public float VerticalValue;

    //or public Vector2 Inputs;
}