using CameraManagement;
using Fusion;
using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerNetworkController : NetworkBehaviour                             //Equals Player.cs in Guide.
    {
        internal PlayerInputActions m_playerInputActions;

        [SerializeField] private GameObject m_localInputParent;
        
        [SerializeField] internal PlayerNetworkInput m_playerNetworkInput;
        [SerializeField] internal PlayerNetworkMovement m_playerNetworkMovement;        //Equals SimpleCarController in Guide.
        [SerializeField] internal PlayerNetworkInteractions m_playerNetworkInteractions;
        [SerializeField] internal PlayerNetworkHealth m_playerNetworkHealth;
        [SerializeField] internal CameraNetworkBehaviour m_cameraNetworkController;
        [SerializeField] internal EOnFootTargetMoveModi m_eCurrentMoveMode;
        [SerializeField] internal EmoveMethod m_eMoveMethod;

        #region Runtime-Values
        #region Reset on falling off the area
        [Header("Area Fall Off Reset")]
        [SerializeField] internal Vector3 m_repopPosition;
        [SerializeField] internal float m_fallLimit = -100f;
        #endregion
        internal bool m_isDead = false;
        #endregion

        #region Network
        [Networked] private PlayerNetworkData PlayerNetworkData { get; set; }
        #endregion

        private void Awake()
        {
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
                m_cameraNetworkController.m_camera.gameObject.SetActive(false);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerNetworkData inputData))      //Equals Player.cs CarInputData in Guide.
            {
                PlayerNetworkData = inputData;
            }

            m_playerNetworkMovement.SetInputData(PlayerNetworkData);
        }
    }
}