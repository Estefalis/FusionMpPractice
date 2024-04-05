using CameraManagement;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;        //For InputDeviceChange from the new InputSystem.

namespace PlayerInputManagement
{
    public class PlayerNetworkInput : NetworkBehaviour, INetworkRunnerCallbacks     //Equals PlayerInputController in Guide.
    {
        [SerializeField] private PlayerNetworkController m_playerNetworkController;

        private Vector3 m_rightVector, m_rotationVector, m_forwardVector;   //Building new MoveVector(s) in combination.

        #region Network
        internal bool JumpButtonGotPressed;
        internal bool KneelButtonGotPressed;
        #endregion

        private EmoveMethod m_ePreviousMoveMethod;

        private void OnDisable()
        {
            if (transform.gameObject.activeInHierarchy)
            {
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Disable();
                #region InputAction-UnSubscriptions
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.performed -= MoveCharacter;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.canceled -= StopMovement;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Jump.performed -= CharacterJump;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Jump.canceled -= OnJumpButtonRelease;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Duck.performed -= CharacterDuck;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Duck.canceled -= StopDucking;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed -= OnRightMouseButtonDown;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled -= OnRightMouseButtonUp;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed -= AccelerateMovespeed;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled -= DecelerateMovespeed;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed -= SwitchCursorLockMode;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed -= ZoomCamera;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled -= StopCameraZoom;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed -= OpenMenu;
                #endregion

                InputUser.onChange -= OnInputDeviceChange;
            }
        }

        private void Start()
        {
            if (transform.gameObject.activeInHierarchy)
            {

                m_playerNetworkController.m_playerInputActions = InputManager.m_InputManagerActions;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Enable();
                #region InputAction-Subscriptions
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.performed += MoveCharacter;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.canceled += StopMovement;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Jump.canceled += OnJumpButtonRelease;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Duck.performed += CharacterDuck;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Duck.canceled += StopDucking;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed += OnRightMouseButtonDown;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled += OnRightMouseButtonUp;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed += AccelerateMovespeed;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled += DecelerateMovespeed;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed += SwitchCursorLockMode;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed += ZoomCamera;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled += StopCameraZoom;
                m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed += OpenMenu;
                #endregion

                InputUser.onChange += OnInputDeviceChange;
            }
        }

        private void Update()
        {
            SubmitInputToPhoton();  //Sends Input to Photon via the 'SetNetworkVectors()' method.
            CameraRotation();
        }

        #region Custom Methods
        private void CameraRotation()
        {
            m_playerNetworkController.m_cameraNetworkController.m_playerInputRotationVector =
                new Vector3(-m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().y, 0.0f);
        }

        private void SubmitInputToPhoton()
        {
            switch (m_playerNetworkController.m_eMoveMethod)
            {
                case EmoveMethod.Basic:
                {
                    m_rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0.0f, 0.0f);
                    m_rotationVector = Vector3.zero;
                    m_forwardVector = new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    break;
                }
                case EmoveMethod.KbRotateY:
                {
                    m_rightVector = Vector3.zero;
                    m_rotationVector =
                        new Vector3(0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0.0f); //A & D
                    m_forwardVector =
                            new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);    //W & S
                    break;
                }
                case EmoveMethod.MouseRotateY:
                {
                    m_rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0.0f, 0.0f); //A & D
                    m_rotationVector =
                        new Vector3(0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, 0.0f);//MouseX Rot Y
                    m_forwardVector =
                            new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y); //W & S
                    break;
                }
                case EmoveMethod.Locked:
                {
                    m_rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, 0);
                    m_rotationVector = Vector3.zero;
                    m_forwardVector = new(0, 0, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    break;
                }
                case EmoveMethod.Relative:
                {
                    m_rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, 0);
                    m_rotationVector = Vector3.zero;
                    m_forwardVector = new(0, 0, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    break;
                }
                default:
                    break;
            }
        }
        #endregion

        #region CallbackContexts        
        #region Normal Acceleration
        //Set normal moveSpeed by pressing WASD and controller relatives.
        private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_playerNetworkMovement.m_moveButtonIsPressed = true;
        }

        private void StopMovement(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_playerNetworkMovement.m_moveButtonIsPressed = false;
        }
        #endregion
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
            JumpButtonGotPressed = _callbackContext.ReadValueAsButton();
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            JumpButtonGotPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            KneelButtonGotPressed = _callbackContext.ReadValueAsButton();
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            KneelButtonGotPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_ePreviousMoveMethod = m_playerNetworkController.m_eMoveMethod;
            m_playerNetworkController.m_eMoveMethod = EmoveMethod.Locked;
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_eMoveMethod = m_ePreviousMoveMethod;
        }
        #endregion
        #region Increasing Acceleration
        //Set fast moveSpeed by pressing shift and controller relatives.
        private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_playerNetworkMovement.m_shiftIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_playerNetworkMovement.m_shiftIsPressed = false;
        }
        #endregion
        #region CursorLockMode
        private void SwitchCursorLockMode(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion
        #region InputDeviceChange
        private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            //TODO: Possible Notifications on changing the inpunt device.
        }
        #endregion
        #region Camera Zoom
        private void ZoomCamera(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_cameraNetworkController.m_zoomScrollValue = _callbackContext.ReadValue<Vector2>().y * m_playerNetworkController.m_cameraNetworkController.m_zoomSpeed;
            //float readValueY = -_callbackContext.ReadValue<Vector2>().y * m_playerNetworkController.m_cameraOfflineBehaviour.m_zoomSpeed;
        }

        private void StopCameraZoom(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_cameraNetworkController.m_zoomScrollValue = 0.0f;
        }
        #endregion
        #region Menu
        private void OpenMenu(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion        
        #endregion

        #region INetworkRunnerCallbacks
        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Runner.AddCallbacks(this);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var inputData = new PlayerNetworkData()
            {
                MoveDirection = new Vector3(m_rightVector.x, m_rotationVector.y, m_forwardVector.z),
                JumpButtonIsPressed = JumpButtonGotPressed,
                KneelButtonIsPressed = KneelButtonGotPressed,
            };

            input.Set(inputData);
        }

        #region Currently unused INetworkRunnerCallbacks
        public void OnConnectedToServer(NetworkRunner runner)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }
        #endregion
        #endregion
    }
}