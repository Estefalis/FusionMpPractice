using Fusion;
using UnityEngine;

namespace PlayerManagement
{
    public class PlayerNetworkController : NetworkBehaviour
    {
        [SerializeField] private GameObject m_localInputParent;

        [SerializeField] internal PlayerNetworkInput m_playerNetworkInput;
        [SerializeField] internal PlayerNetworkMovement m_playerNetworkMovement;    //Equal to SimpleCarController.
        [SerializeField] internal PlayerNetworkInteractions m_playerNetworkInteractions;
        [SerializeField] internal PlayerNetworkHealth m_playerNetworkHealth;
        [SerializeField] internal CameraNetworkBehaviour m_cameraNetworkBehaviour;
        [SerializeField] internal EAvatarMoveState m_eAvatarMoveState;
        [SerializeField] internal ERigidbodyMoveMethod m_eRigidbodyMoveMethod;

        #region Runtime-Values
        #region Reset on falling off the area
        [Header("Area Fall Off Reset")]
        [SerializeField] internal Vector3 m_rePopPosition;
        [SerializeField] internal float m_fallLimit = -100f;
        #endregion
        internal bool m_isDead = false;
        #endregion

        #region Network
        [Networked] private PlayerNetworkInputData PlayerNetworkedData { get; set; }   //Remote _playerRef receive the correct input data to move their avatars.
        //internal NetworkId m_networkId;
        #endregion

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                //m_networkId = Object.Id;
                m_localInputParent.SetActive(true);
            }
            else
            {
                m_localInputParent.SetActive(false);
            }
        }

        //public override void FixedUpdateNetwork()
        //{
        //    if (GetInput(out PlayerNetworkInputData networkedInputData))
        //    {
        //        PlayerNetworkedData = networkedInputData;
        //    }

        //    m_playerNetworkMovement.SetInputData(PlayerNetworkedData);
        //}
    }
}