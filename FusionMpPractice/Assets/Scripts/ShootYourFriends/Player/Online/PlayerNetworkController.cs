using Fusion;
using UnityEngine;

namespace PlayerManagement
{
    public class PlayerNetworkController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [SerializeField] private GameObject m_localInputParent;

        [SerializeField] internal PlayerNetworkInput m_playerNetworkInput;
        [SerializeField] internal PlayerNetworkMovement m_playerNetworkMovement;
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
        //[Networked] private PlayerNetworkData PlayerNetworkedData { get; set; }   //Remote _playerRef receive the correct input data to move their avatars.
        internal PlayerRef m_playerRef;
        internal int m_playerId;
        #endregion

        public void PlayerJoined(PlayerRef _playerRef)
        {
            m_playerRef = _playerRef;
            m_playerId = m_playerRef.PlayerId;
        }

        public void PlayerLeft(PlayerRef _playerRef)
        {
            m_playerRef = -1;
            m_playerId = -1;
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

        //public override void FixedUpdateNetwork()
        //{
        //    ////if (GetInput<CombinedPlayerInputs>(out var input))
        //    ////{
        //    ////    var dir = input[0].MoveDirection;
        //    ////    if (dir != Vector3.zero)
        //    ////        Debug.Log($"ID 0: {input[0].MoveDirection}");
        //    ////}

        //    if (GetInput(out PlayerNetworkData networkedInputData))
        //    {
        //        PlayerNetworkedData = networkedInputData;
        //    }

        //    //m_playerNetworkMovement.SetInputData(PlayerNetworkedData);
        //}
    }
}
//var PlayerRefStruct = Runner.LocalPlayer.PlayerId;
//var PlayerRefId = PlayerRefStruct.PlayerId;
//var isPlayerIndexValid = PlayerRefStruct.IsValid; //Runner.LocalPlayer.IsValid.