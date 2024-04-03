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

        private Vector3 m_horizontalMovement, m_characterRotation;

        #region Network
        internal Vector3 SidewardMovement;
        internal Vector3 ForwardMovement;
        internal Vector3 RotationMovement;
        internal bool JumpButtonGotPressed;
        internal bool JumpButtonGotReleased;
        internal bool KneelButtonGotPressed;
        #endregion

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
            SubmitCameraRotation();
        }

        #region Custom Methods
        private void SubmitCameraRotation()
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
                    Vector3 forwardVector = new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    Vector3 rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0.0f, 0.0f);
                    SetNetworkVectors(rightVector, Vector3.zero, forwardVector);
                    break;
                }
                case EmoveMethod.ADRotateY:
                {
                    m_horizontalMovement =
                            new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);    //W & S
                    m_characterRotation =
                        new Vector3(0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0.0f);
                    //A & D
                    SetNetworkVectors(Vector3.zero, m_characterRotation, m_horizontalMovement);
                    break;
                }
                case EmoveMethod.MouseRotateY:
                {
                    m_horizontalMovement =
                            new(0.0f, 0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);     //W & S
                    m_characterRotation =
                        new Vector3(0.0f, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, 0.0f);
                    //MouseX Rot Y
                    SetNetworkVectors(Vector3.zero, m_characterRotation, m_horizontalMovement);
                    break;
                }
                case EmoveMethod.Relative:
                {
                    #region Use of custom RelativeHelperPositioning(){} HelperConstruct in CameraBehaviour.cs
                    //Vector3 fakecameraForward = m_playerNetworkController.m_cameraOfflineBehaviour.m_relativeHelperTransform.forward;
                    //Vector3 cameraRight = m_playerNetworkController.m_cameraOfflineBehaviour.m_camera.transform.right;
                    ////cameraForward = cameraForward.normalized;
                    //cameraRight.y = 0;    //prevents characterJumps.
                    //cameraRight = cameraRight.normalized;
                    //Vector3 relativeForward = m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y * fakecameraForward;
                    #endregion

                    Vector3 cameraForward = m_playerNetworkController.m_cameraNetworkController.m_camera.transform.forward;
                    Vector3 cameraRight = m_playerNetworkController.m_cameraNetworkController.m_camera.transform.right;
                    cameraForward.y = 0.0f;   //prevents characterJumps.
                    cameraRight.y = 0.0f;    //prevents characterJumps.
                    cameraForward = cameraForward.normalized;   //Rotating the camera up or down does not influence the movementSpeed anymore.
                    cameraRight = cameraRight.normalized;   //Rotating the camera up or down does not influence the movementSpeed anymore.
                    Vector3 relativeForward = m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y * cameraForward;

                    Vector3 relativeRight = m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x * cameraRight;
                    SetNetworkVectors(relativeRight, Vector3.zero, relativeForward);
                    break;
                }
                case EmoveMethod.Locked:
                {
                    m_horizontalMovement = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);

                    Vector3 forwardVector = new(0, 0, m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    Vector3 rightVector = new(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, 0);
                    SetNetworkVectors(rightVector, Vector3.zero, forwardVector);
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
            NetworkJumpButtonState(_callbackContext.ReadValueAsButton());
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            NetworkJumpButtonState(_callbackContext.ReadValueAsButton());
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            NetworkKneelButtonState(/*m_kneelToCrouch = */_callbackContext.ReadValueAsButton());
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            NetworkKneelButtonState(/*m_kneelToCrouch = */_callbackContext.ReadValueAsButton());
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_eMoveMethod = EmoveMethod.Locked;
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            switch (m_playerNetworkController.m_cameraNetworkController.m_playerPerspective)
            {
                case PlayerPersPective.ThirdPerson:
                {
                    m_playerNetworkController.m_eMoveMethod = EmoveMethod.Relative;
                    break;
                }
                case PlayerPersPective.FirstPerson:
                {
                    m_playerNetworkController.m_eMoveMethod = EmoveMethod.ADRotateY;
                    break;
                }
            }
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

        #region Sets OnInput Variables in PlayerNetworkDataInput for Fusion's NetworkRunner.
        private void SetNetworkVectors(Vector3 _rightVector, Vector3 _rotationVector, Vector3 _forwardVector)
        {
            SidewardMovement = _rightVector;
            RotationMovement = _rotationVector;
            ForwardMovement = _forwardVector;
        }

        private void NetworkJumpButtonState(bool _jumpButtonIsPressed)
        {
            JumpButtonGotPressed = _jumpButtonIsPressed;
        }

        private void NetworkKneelButtonState(bool _kneelButtonIsPressed)
        {
            KneelButtonGotPressed = _kneelButtonIsPressed;
        }
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
                ForwardVector = ForwardMovement,
                RightVector = SidewardMovement,
                RotationVector = RotationMovement,
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