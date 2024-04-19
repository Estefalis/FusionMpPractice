using UnityEngine;

namespace PlayerManagement
{
    public class CameraNetworkBehaviour : MonoBehaviour
    {
        [SerializeField] internal Camera m_camera;

        internal Vector3 m_playerInputRotationVector;       //Incoming Inputsystem Input.

        private void Awake()
        {
            if (m_camera == null)
                m_camera = GetComponentInChildren<Camera>();
        }
    }
}