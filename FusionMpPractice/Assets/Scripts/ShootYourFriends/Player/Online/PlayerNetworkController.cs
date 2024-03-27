using CameraManagement;
using Fusion;
using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerNetworkController : NetworkBehaviour
    {
        internal PlayerInputActions m_playerInputActions;

        [SerializeField] private GameObject m_localInputParent;
        [SerializeField] internal Rigidbody m_rigidbody;
        [SerializeField] internal CapsuleCollider m_capsuleCollider;

        [SerializeField] internal PlayerNetworkInput m_playerNetworkInput;
        [SerializeField] internal PlayerNetworkDataInput m_playerNetworkDataInput;
        [SerializeField] internal PlayerNetworkMovement m_playerNetworkMovement;
        [SerializeField] internal PlayerNetworkInteractions m_playerNetworkInteractions;
        [SerializeField] internal PlayerNetworkHealth m_playerNetworkHealth;
        [SerializeField] internal CameraNetworkBehaviour m_cameraNetworkBehaviour;
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

        #region Network
        internal PlayerNetworkData m_playerNetworkData;
        #endregion

        private void Awake()
        {
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();

            m_startPosition = transform.position;
            m_eCurrentMoveMode = EOnFootTargetMoveModi.Walking;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                m_localInputParent.SetActive(true);
            }
            else
            {
                m_localInputParent.SetActive(false);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerNetworkData inputData))
            {
                ProvideMovementData(inputData);
            }
        }

        /// <summary>
        /// IMPORTANT: Use Runner.DeltaTime instead of common DeltaTime!!!
        /// </summary>
        /// <param name="inputData"></param>
        internal void ProvideMovementData(PlayerNetworkData inputData)
        {
            m_playerNetworkData = inputData;
            //TODO: Update PlayerNetworkMovement.
        }
    }
}