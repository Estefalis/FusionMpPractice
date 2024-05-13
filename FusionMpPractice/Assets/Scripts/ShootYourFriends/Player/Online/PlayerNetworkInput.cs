using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.Users;        //For InputDeviceChange from the new InputSystem.

namespace PlayerManagement
{
    public enum EInputButtons
    {
        Forward = 0,
        Backward = 1,
        Left = 2,
        Right = 3,
        Jump = 4,
        Duck = 5,
    }

    public class PlayerNetworkInput : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] private PlayerNetworkController m_playerNetworkController;

        #region Network
        //[Networked] private NetworkButtons m_previousButtonState { get; set; }
        private float m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal;   //Building new MoveVector(s) in combination.
        private Vector3 m_localMoveVector;
        internal bool JumpButtonIsPressed;
        internal bool KneelButtonIsPressed;
        #endregion

        private ERigidbodyMoveMethod m_ePreviousMoveMethod;

        private void OnDisable()
        {
            if (transform.gameObject.activeInHierarchy)
            {
                if (Runner != null)
                {
                    m_playerInputActions.PlayerOnFoot.Disable();
                    Runner.RemoveCallbacks(this);
                }

                #region InputAction-UnSubscriptions
                m_playerInputActions.PlayerOnFoot.Movement.performed -= MoveCharacter;
                m_playerInputActions.PlayerOnFoot.Movement.canceled -= StopMovement;
                m_playerInputActions.PlayerOnFoot.Jump.performed -= CharacterJump;
                m_playerInputActions.PlayerOnFoot.Jump.canceled -= OnJumpButtonRelease;
                m_playerInputActions.PlayerOnFoot.Duck.performed -= CharacterDuck;
                m_playerInputActions.PlayerOnFoot.Duck.canceled -= StopDucking;
                m_playerInputActions.PlayerOnFoot.SwitchMoveMode.performed -= OnRightMouseButtonDown;
                m_playerInputActions.PlayerOnFoot.SwitchMoveMode.canceled -= OnRightMouseButtonUp;
                m_playerInputActions.PlayerOnFoot.Acceleration.performed -= AccelerateMovespeed;
                m_playerInputActions.PlayerOnFoot.Acceleration.canceled -= DecelerateMovespeed;
                m_playerInputActions.PlayerOnFoot.CursorLockMode.performed -= SwitchCursorLockMode;
                m_playerInputActions.PlayerOnFoot.CameraZoom.performed -= ZoomCamera;
                m_playerInputActions.PlayerOnFoot.CameraZoom.canceled -= StopCameraZoom;
                m_playerInputActions.PlayerOnFoot.OpenMenu.performed -= OpenMenu;
                #endregion

                //InputUser.onChange -= OnInputDeviceChange;
            }
        }

        private void Start()
        {
            if (transform.gameObject.activeInHierarchy)
            {
                if (Runner != null)
                {
                    m_playerInputActions = InputManager.m_InputManagerActions;
                    m_playerInputActions.PlayerOnFoot.Enable();
                    Runner.AddCallbacks(this);
                }

                #region List isComposite/isPartOfComposite from Actions in Console
                //for (int i = 0; i < m_playerInputActions.PlayerOnFoot.Movement.bindings.Count; i++)
                //{
                //    if (m_playerInputActions.PlayerOnFoot.Movement.bindings[i].isComposite)
                //    {
                //        var bindings = m_playerInputActions.PlayerOnFoot.Movement.bindings[i];
                //        Debug.Log($"isComposite: {bindings.effectivePath}");   //.path/.name/.effectivePath.
                //    }

                //    if (m_playerInputActions.PlayerOnFoot.Movement.bindings[i].isPartOfComposite)
                //    {
                //        var bindings = m_playerInputActions.PlayerOnFoot.Movement.bindings[i];
                //        Debug.Log($"isPartOfComposite: {bindings.effectivePath}");   //.path/.name/.effectivePath.
                //    }
                //}
                #endregion

                #region InputAction-Subscriptions
                m_playerInputActions.PlayerOnFoot.Movement.performed += MoveCharacter;
                m_playerInputActions.PlayerOnFoot.Movement.canceled += StopMovement;
                m_playerInputActions.PlayerOnFoot.Jump.performed += CharacterJump;
                m_playerInputActions.PlayerOnFoot.Jump.canceled += OnJumpButtonRelease;
                m_playerInputActions.PlayerOnFoot.Duck.performed += CharacterDuck;
                m_playerInputActions.PlayerOnFoot.Duck.canceled += StopDucking;
                m_playerInputActions.PlayerOnFoot.SwitchMoveMode.performed += OnRightMouseButtonDown;
                m_playerInputActions.PlayerOnFoot.SwitchMoveMode.canceled += OnRightMouseButtonUp;
                m_playerInputActions.PlayerOnFoot.Acceleration.performed += AccelerateMovespeed;
                m_playerInputActions.PlayerOnFoot.Acceleration.canceled += DecelerateMovespeed;
                m_playerInputActions.PlayerOnFoot.CursorLockMode.performed += SwitchCursorLockMode;
                m_playerInputActions.PlayerOnFoot.CameraZoom.performed += ZoomCamera;
                m_playerInputActions.PlayerOnFoot.CameraZoom.canceled += StopCameraZoom;
                m_playerInputActions.PlayerOnFoot.OpenMenu.performed += OpenMenu;
                #endregion

                //InputUser.onChange += OnInputDeviceChange; 
            }
        }

        private void Update()
        {
            //RetrieveUserInput();  //Modular Setup of Vectors for individual Movement.
            CameraRotation();
        }

        #region Custom Methods
        private void CameraRotation()
        {
            m_playerNetworkController.m_cameraNetworkBehaviour.m_playerInputRotationVector =
                new Vector3(-m_playerInputActions.PlayerOnFoot.CameraMovement.ReadValue<Vector2>().x, m_playerInputActions.PlayerOnFoot.CameraMovement.ReadValue<Vector2>().y, 0.0f);
        }

        private void RetrieveUserInput()
        {
            switch (m_playerNetworkController.m_eRigidbodyMoveMethod)
            {
                case ERigidbodyMoveMethod.Basic:
                {
                    m_rightInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;
                    m_rotationInputLocal = 0.0f;
                    m_forwardInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;
                    break;
                }
                case ERigidbodyMoveMethod.KbRotateY:
                {
                    m_rightInputLocal = 0.0f;
                    m_rotationInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;           //A & D
                    m_forwardInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.MouseRotateY:
                {
                    m_rightInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInputLocal = m_playerInputActions.PlayerOnFoot.Rotation.ReadValue<Vector2>().x;           //MouseX Rot Y
                    m_forwardInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.Locked:
                {
                    m_rightInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInputLocal = 0.0f;
                    m_forwardInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.Relative:
                {
                    m_rightInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInputLocal = 0.0f;
                    m_forwardInputLocal = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                default:
                    break;
            }

            //m_localMoveVector = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal);
        }
        #endregion

        #region CallbackContexts        
        #region Normal Acceleration
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
            JumpButtonIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            JumpButtonIsPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            KneelButtonIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            KneelButtonIsPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_ePreviousMoveMethod = m_playerNetworkController.m_eRigidbodyMoveMethod;
            m_playerNetworkController.m_eRigidbodyMoveMethod = ERigidbodyMoveMethod.Locked;
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_eRigidbodyMoveMethod = m_ePreviousMoveMethod;
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
        //private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        //{
        //    //TODO: Possible Notifications on changing the input device.
        //}
        #endregion
        #region Camera Zoom
        private void ZoomCamera(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_cameraNetworkBehaviour.m_zoomScrollValue = _callbackContext.ReadValue<Vector2>().y * m_playerNetworkController.m_cameraNetworkBehaviour.m_zoomSpeed;
        }

        private void StopCameraZoom(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_cameraNetworkBehaviour.m_zoomScrollValue = 0.0f;
        }
        #endregion

        #region Menu
        private void OpenMenu(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion
        #endregion

        public void BeforeUpdate()
        {
            RetrieveUserInput();  //Modular Setup of Vectors for individual Movement.
        }

        #region INetworkRunnerCallbacks
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            #region Version 1
            //PlayerNetworkData playerInput = new();
            //var InputActions = m_playerInputActions.PlayerOnFoot;

            #region OnInput Tests
            //playerInput.MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal);

            //playerInput.InputButtons.Set(EInputButtons.Forward, m_forwardInputLocal > 0);
            //playerInput.InputButtons.Set(EInputButtons.Backward, m_forwardInputLocal < 0);
            //playerInput.InputButtons.Set(EInputButtons.Left, m_rightInputLocal < 0);
            //playerInput.InputButtons.Set(EInputButtons.Right, m_rightInputLocal > 0);
            //Debug.Log($"Forward: {m_forwardInputLocal > 0} - Backward: {m_forwardInputLocal < 0} - Left: {m_rightInputLocal < 0} - Right: {m_rightInputLocal > 0} - ");

            //playerInput.InputButtons.Set(EInputButtons.Jump, InputActions.PlayerOnFoot.Jump.IsPressed()); //m_playerInputActions.PlayerOnFoot;
            //playerInput.InputButtons.Set(EInputButtons.Jump, InputActions.PlayerOnFoot.Duck.IsPressed());
            #endregion

            //playerInput.MoveDirection = m_localMoveVector;

            //playerInput.JumpButtonGotPressed = JumpButtonIsPressed;
            //playerInput.KneelButtonGotPressed = KneelButtonIsPressed;
            #endregion

            #region Version 2
            var playerInput = new PlayerNetworkData()   //or PlayerNetworkData playerInput = new(); playerInput.xyz = retrieved Input;
            {
                MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
                //MoveDirection = m_localMoveVector,
                JumpButtonGotPressed = JumpButtonIsPressed,
                KneelButtonGotPressed = KneelButtonIsPressed,
            };
            #endregion

            input.Set(playerInput);

            //playerInput = default;
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