using Fusion;
using System;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private SimpleCarController m_carController;
    [SerializeField] private GameObject m_localComponents;
    [SerializeField] private CarCameraController m_carCameraController;

    [Networked] public CarInputData m_CarInputData { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            m_localComponents.SetActive(true);
        }
        else
            m_localComponents.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        //if (Object.HasInputAuthority)
        //{
        if (GetInput(out CarInputData _inputData))
        {
            m_CarInputData = _inputData;
        }
        //}

        m_carController.SetInputData(m_CarInputData);
    }

    internal void SetGameBall(Transform _ballVisual)
    {
        m_carCameraController.SetGameBall(_ballVisual);
    }
}