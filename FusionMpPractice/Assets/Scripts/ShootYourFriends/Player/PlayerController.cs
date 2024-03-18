using UnityEngine;
using UnityEngine.InputSystem;

internal enum EOnFootTargetMoveModi
{
    Idle,
    Walking,
    Running,
    Crouching,
}

namespace PlayerInputManagement
{
    public class PlayerController : MonoBehaviour
    {
        internal PlayerInputActions m_playerInputActions;
        internal InputActionMap m_currentActionMap;

        [SerializeField] internal Rigidbody m_rigidbody;
        [SerializeField] internal CapsuleCollider m_capsuleCollider;

        [SerializeField] internal PlayerInput m_playerInput;
        [SerializeField] internal PlayerMovement m_playerMovement;
        [SerializeField] internal PlayerInteractions m_playerInteractions;
        [SerializeField] internal PlayerHealth m_playerHealth;
        [SerializeField] internal CameraBehaviour m_cameraBehaviour;
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

            InputManager.m_changeActiveActionMap += CurrentlyActiveActionMap;
        }

        private void OnDisable()
        {
            InputManager.m_changeActiveActionMap -= CurrentlyActiveActionMap;
        }

        private void CurrentlyActiveActionMap(InputActionMap _activeInputActionMap)
        {
            m_currentActionMap = _activeInputActionMap;
        }
    }
}