using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (transform.gameObject.activeInHierarchy && Object.HasInputAuthority)
            {
                m_playerInputActions.PlayerOnFoot.Disable();

                if (Runner != null)
                    Runner.RemoveCallbacks(this);

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
                m_playerInputActions.PlayerOnFoot.CursorVisibility.performed -= SwitchCursorVisibility;
                m_playerInputActions.PlayerOnFoot.CameraZoom.performed -= ZoomCamera;
                m_playerInputActions.PlayerOnFoot.CameraZoom.canceled -= StopCameraZoom;
                m_playerInputActions.PlayerOnFoot.OpenMenu.performed -= OpenMenu;
                #endregion
            }
        }

        private void Start()
        {
            if (Object.HasInputAuthority)
            {
                m_playerInputActions = InputManager.m_InputManagerActions;
                m_playerInputActions.PlayerOnFoot.Enable();

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
                m_playerInputActions.PlayerOnFoot.CursorVisibility.performed += SwitchCursorVisibility;
                m_playerInputActions.PlayerOnFoot.CameraZoom.performed += ZoomCamera;
                m_playerInputActions.PlayerOnFoot.CameraZoom.canceled += StopCameraZoom;
                m_playerInputActions.PlayerOnFoot.OpenMenu.performed += OpenMenu;
                #endregion
            }
        }

        private void Update()
        {
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
        #region CursorVisibility
        private void SwitchCursorVisibility(InputAction.CallbackContext _callbackContext)
        {
            m_playerNetworkController.m_cameraNetworkBehaviour.SwitchCursorVisibility();
        }
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

        public override void Spawned()
        {
            if (Runner != null && Object.HasInputAuthority)
            {
                Runner.AddCallbacks(this);
            }
        }

        public void BeforeUpdate()
        {
            RetrieveUserInput();  //Modular Setup of Vectors for individual Movement.
        }

        #region INetworkRunnerCallbacks
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            #region Version 1
            //PlayerNetworkData playerInput = new PlayerNetworkData();
            ////var InputActions = m_playerInputActions.PlayerOnFoot;

            //playerInput.MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal);
            //playerInput.MoveDirection = m_localMoveVector;
            //playerInput.JumpButtonGotPressed = JumpButtonIsPressed;
            //playerInput.KneelButtonGotPressed = KneelButtonIsPressed;

            #region OnInput InputButtons.Set-Tests
            //playerInput.InputButtons.Set(EInputButtons.Forward, m_forwardInputLocal > 0);
            //playerInput.InputButtons.Set(EInputButtons.Backward, m_forwardInputLocal < 0);
            //playerInput.InputButtons.Set(EInputButtons.Left, m_rightInputLocal < 0);
            //playerInput.InputButtons.Set(EInputButtons.Right, m_rightInputLocal > 0);
#if UNITY_EDITOR
            //Debug.Log($"Forward: {m_forwardInputLocal > 0} - Backward: {m_forwardInputLocal < 0} - Left: {m_rightInputLocal < 0} - Right: {m_rightInputLocal > 0} - ");
#endif
            //playerInput.InputButtons.Set(EInputButtons.Jump, InputActions.PlayerOnFoot.Jump.IsPressed());
            //playerInput.InputButtons.Set(EInputButtons.Jump, InputActions.PlayerOnFoot.Duck.IsPressed());
            #endregion
            #endregion

            #region Version 2
            var playerInput = new PlayerNetworkData()   //or PlayerNetworkData playerInput = new();
            {
                MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
                //MoveDirection = m_localMoveVector,
                JumpButtonGotPressed = JumpButtonIsPressed,
                KneelButtonGotPressed = KneelButtonIsPressed,
            };
            #endregion

            #region Combined Player Inputs
            //var playerInput = new CombinedPlayerInputs();

            //playerInput[0] = new PlayerNetworkData()
            //{
            //    MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
            //    //MoveDirection = m_localMoveVector,
            //    JumpButtonGotPressed = JumpButtonIsPressed,
            //    KneelButtonGotPressed = KneelButtonIsPressed,
            //};
            //playerInput[1] = new PlayerNetworkData()
            //{
            //    MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
            //    //MoveDirection = m_localMoveVector,
            //    JumpButtonGotPressed = JumpButtonIsPressed,
            //    KneelButtonGotPressed = KneelButtonIsPressed,
            //};
            //playerInput[2] = new PlayerNetworkData()
            //{
            //    MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
            //    //MoveDirection = m_localMoveVector,
            //    JumpButtonGotPressed = JumpButtonIsPressed,
            //    KneelButtonGotPressed = KneelButtonIsPressed,
            //};
            //playerInput[3] = new PlayerNetworkData()
            //{
            //    MoveDirection = new Vector3(m_rightInputLocal, m_rotationInputLocal, m_forwardInputLocal),
            //    //MoveDirection = m_localMoveVector,
            //    JumpButtonGotPressed = JumpButtonIsPressed,
            //    KneelButtonGotPressed = KneelButtonIsPressed,
            //};
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