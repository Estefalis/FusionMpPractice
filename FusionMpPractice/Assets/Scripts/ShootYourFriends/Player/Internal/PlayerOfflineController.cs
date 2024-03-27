using CameraManagement;
using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerOfflineController : MonoBehaviour
    {
        internal PlayerInputActions m_playerInputActions;
        [SerializeField] internal Rigidbody m_rigidbody;
        [SerializeField] internal CapsuleCollider m_capsuleCollider;

        [SerializeField] internal PlayerOfflineInput m_playerOfflineInput;
        [SerializeField] internal PlayerOfflineMovement m_playerOfflineMovement;
        [SerializeField] internal PlayerOfflineInteractions m_playerOfflineInteractions;
        [SerializeField] internal PlayerOfflineHealth m_playerHealth;
        [SerializeField] internal CameraOfflineBehaviour m_cameraOfflineBehaviour;
        [SerializeField] internal EOnFootTargetMoveModi m_eCurrentMoveMode;

        #region Runtime-Values
        #region Reset on falling off the area
        [Header("Area Fall Off Reset")]
        [SerializeField] internal Vector3 m_repopPosition;
        [SerializeField] internal float m_fallLimit = -100f;
        #endregion
        internal bool m_isDead = false;
        internal Vector3 m_startPosition;
        #endregion

        private void Awake()
        {
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();

            m_startPosition = transform.position;
            m_eCurrentMoveMode = EOnFootTargetMoveModi.Walking;
        }
    }
}