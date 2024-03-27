using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerOfflineInput : MonoBehaviour
    {
        [SerializeField] private PlayerOfflineController m_playerOfflineController;

        private void Start()
        {
            m_playerOfflineController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Enable();
            #region InputAction-Subscriptions
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Movement.performed += MoveCharacter;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Movement.canceled += StopMovement;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed += OnRightMouseButtonDown;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled += OnRightMouseButtonUp;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.performed += ActiveBraking;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.canceled += CancelActiveBraking;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed += AccelerateMovespeed;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled += DecelerateMovespeed;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed += SwitchCursorLockMode;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed += ZoomCamera;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled += StopCameraZoom;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed += OpenMenu;
            #endregion

            InputUser.onChange += OnInputDeviceChange;
        }

        private void OnDisable()
        {
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Disable();
            #region InputAction-UnSubscriptions
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Movement.performed -= MoveCharacter;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Movement.canceled -= StopMovement;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed -= OnRightMouseButtonDown;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled -= OnRightMouseButtonUp;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.performed -= ActiveBraking;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.canceled -= CancelActiveBraking;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed -= AccelerateMovespeed;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled -= DecelerateMovespeed;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed -= SwitchCursorLockMode;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed -= ZoomCamera;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled -= StopCameraZoom;
            m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed -= OpenMenu;
            #endregion

            InputUser.onChange -= OnInputDeviceChange;

            m_playerOfflineController.m_playerOfflineMovement.m_permitCrouchLerp = false;
        }

        private void Update()
        {
            SubmitCameraRotation();
        }

        private void SubmitCameraRotation()
        {
            //if(m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.inProgress)
            m_playerOfflineController.m_cameraOfflineBehaviour.m_playerInputRotationVector =
                new Vector3(-m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, m_playerOfflineController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().y, 0.0f);
        }

        #region CallbackContexts        
        #region Normal Acceleration
        //Set normal moveSpeed by pressing WASD and controller relatives.
        private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_moveButtonIsPressed = true;
        }

        private void StopMovement(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_moveButtonIsPressed = false;
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_eMoveMethod = EmoveMethod.Locked;
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            switch (m_playerOfflineController.m_cameraOfflineBehaviour.m_playerPerspective)
            {
                case PlayerPersPective.ThirdPerson:
                {
                    m_playerOfflineController.m_playerOfflineMovement.m_eMoveMethod = EmoveMethod.Relative;
                    break;
                }
                case PlayerPersPective.FirstPerson:
                {
                    m_playerOfflineController.m_playerOfflineMovement.m_eMoveMethod = EmoveMethod.ADRotateY;   //TODO: Change to 'MouseRotateY'
                    break;
                }
            }
        }
        #endregion
        #region Active Breaking
        private void ActiveBraking(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_activeBraking = true;
        }
        private void CancelActiveBraking(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_activeBraking = false;
        }
        #endregion
        #region Increasing Acceleration
        //Set fast moveSpeed by pressing shift and controller relatives.
        private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_shiftIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_shiftIsPressed = false;
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
            m_playerOfflineController.m_cameraOfflineBehaviour.m_zoomScrollValue = _callbackContext.ReadValue<Vector2>().y * m_playerOfflineController.m_cameraOfflineBehaviour.m_zoomSpeed;
            //float readValueY = -_callbackContext.ReadValue<Vector2>().y * m_playerNetworkController.m_cameraOfflineBehaviour.m_zoomSpeed;
        }

        private void StopCameraZoom(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_cameraOfflineBehaviour.m_zoomScrollValue = 0.0f;
        }
        #endregion
        #region Menu
        private void OpenMenu(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion
        #endregion
    }
}