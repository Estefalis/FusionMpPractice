using UnityEngine;

namespace PlayerManagement
{
    public class PlayerOfflineController : MonoBehaviour
    {
        [SerializeField] private GameObject m_inputCameraComponents;

        [SerializeField] internal PlayerOfflineInput m_playerOfflineInput;
        [SerializeField] internal PlayerOfflineMovement m_playerOfflineMovement;
        [SerializeField] internal PlayerOfflineInteractions m_playerOfflineInteractions;
        [SerializeField] internal PlayerOfflineHealth m_playerOfflineHealth;
        [SerializeField] internal CameraOfflineBehaviour m_cameraOfflineBehaviour;
        [SerializeField] internal EAvatarMoveState m_eEAvatarMoveState;
        [SerializeField] internal ERigidbodyMoveMethod m_eRigidbodyMoveMethod;

        #region Runtime-Values
        #region Reset on falling off the area
        [Header("Area Fall Off Reset")]
        [SerializeField] internal Vector3 m_rePopPosition;
        [SerializeField] internal float m_fallLimit = -100f;
        #endregion
        internal bool m_isDead = false;
        #endregion
    }
}