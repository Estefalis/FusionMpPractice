using UnityEngine;
using UnityEngine.InputSystem;

internal enum EOnFootMoveModi
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
        [SerializeField] internal EOnFootMoveModi m_eCurrentMoveMode;

        #region Fall-Damage
        [Header("Fall Damage")]
        [SerializeField] internal int m_minFallDistance = 5;             //MinimumDistance to take damage.
        [SerializeField] internal float m_fallDamageMultiplier = 1;      //Adjustment-variable.
        [SerializeField] internal float m_finalFallDistance;             //Calculated fallDamage.
        [SerializeField] internal bool m_fallDamageEnabled = true;

        internal bool m_allowApplyingDamageOnce = false;
        internal bool m_isGroundContactLost = false;
        internal Vector3 m_lostGroundContact;
        internal Vector3 m_regainedGroundContact;
        #endregion

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
            m_eCurrentMoveMode = EOnFootMoveModi.Walking;

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