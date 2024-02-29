using Cinemachine;
using EditorButton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_ballCamera;
    [SerializeField] private CinemachineVirtualCamera m_carCamera;

    [SerializeField] private bool m_isCarCameraActive;

    private void Start()
    {
        m_isCarCameraActive = true;
        UpdateCameraStates();
    }

    private void UpdateCameraStates()
    {
        if (m_isCarCameraActive)
        {
            m_carCamera.Priority = 1;
            m_ballCamera.Priority = 0;
        }
        else
        {
            m_carCamera.Priority = 0;
            m_ballCamera.Priority = 1;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            ToggleCamera();
        }
    }

    //[Button]
    private void ToggleCamera()
    {
        m_isCarCameraActive = !m_isCarCameraActive;
        UpdateCameraStates();
    }

    internal void SetGameBall(Transform _ballVisual)
    {
        m_ballCamera.LookAt = _ballVisual;
    }
}