using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerInput : MonoBehaviour
    {
        //private InputControlScheme m_inputControlScheme;
        [SerializeField] private PlayerController m_playerController;

        private void Start()
        {
            m_playerController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Enable();
            #region InputAction-Subscriptions
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.performed += MoveCharacter;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.canceled += StopMovement;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled += OnJumpButtonRelease;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed += OnRightMouseButtonDown;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled += OnRightMouseButtonUp;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.performed += ActiveBraking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.canceled += CancelActiveBraking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.performed += CharacterDuck;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.canceled += StopDucking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed += AccelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled += DecelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed += SwitchCursorLockMode;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed += ZoomCamera;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled += StopCameraZoom;
            m_playerController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed += OpenMenu;
            #endregion

            InputUser.onChange += OnInputDeviceChange;

            //Debug.Log($"CurrentActionMap Name {m_playerController.m_currentActionMap.name} - CurrentActionMap ID {m_playerController.m_currentActionMap.id}");
            //Debug.Log($"ControllerAction Name {m_playerController.m_playerInputActions.asset.actionMaps[0].name} - ControllerAction ID {m_playerController.m_playerInputActions.asset.actionMaps[0].id}");
            //Debug.Log($"ReferenceAction Name {m_mouseRotationActionMaps[0].action.name} - ReferenceAction ID {m_mouseRotationActionMaps[0].action.id}");
        }

        private void OnDisable()
        {
            m_playerController.m_playerInputActions.PlayerOnFootRH.Disable();
            #region InputAction-UnSubscriptions
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.performed -= MoveCharacter;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.canceled -= StopMovement;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled += OnJumpButtonRelease;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed -= OnRightMouseButtonDown;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled -= OnRightMouseButtonUp;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.performed -= ActiveBraking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBraking.canceled -= CancelActiveBraking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.performed -= CharacterDuck;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.canceled -= StopDucking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed -= AccelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled -= DecelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed -= SwitchCursorLockMode;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed -= ZoomCamera;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.canceled -= StopCameraZoom;
            m_playerController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed -= OpenMenu;
            #endregion

            InputUser.onChange -= OnInputDeviceChange;

            m_playerController.m_playerMovement.m_permitCrouchLerp = false;
        }

        private void Update()
        {
            SubmitCameraRotation();
        }

        private void SubmitCameraRotation()
        {
            switch (m_playerController.m_currentActionMap.name) //switch (m_playerController.m_currentActionMap.id) //How to use this Map-IDs?
            {
                case "PlayerOnFootRH":
                {
                    m_playerController.m_cameraBehaviour.m_playerInputRotationVector =
                new Vector3(-m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().y, 0.0f);
                    break;
                }
                //TODO: Add new ActionMap cases on demand.
                default:
                    break;
            }
        }

        #region CallbackContexts        
        #region Normal Acceleration
        //Set normal moveSpeed by pressing WASD and controller relatives.
        private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_moveButtonIsPressed = true;
        }

        private void StopMovement(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_moveButtonIsPressed = false;
        }
        #endregion
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_jumpButtonIsPressed = _callbackContext.ReadValueAsButton();
            m_playerController.m_playerMovement.m_jumpButtonIsReleased = false;
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_jumpButtonIsPressed = false;
            m_playerController.m_playerMovement.m_jumpButtonIsReleased = true;
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_blockRotation = true; //m_characterRotation.y will also get set to 0 in PlayerMovement.cs.
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_blockRotation = false;
        }
        #endregion
        #region Active Breaking
        private void ActiveBraking(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_activeBraking = true;
        }
        private void CancelActiveBraking(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_activeBraking = false;
        }
        #endregion
        #region Increasing Acceleration
        //Set fast moveSpeed by pressing shift and controller relatives.
        private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_shiftIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_shiftIsPressed = false;
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_permitCrouchLerp = _callbackContext.ReadValueAsButton();
            m_playerController.m_playerMovement.m_kneelToCrouch = m_playerController.m_playerMovement.m_permitCrouchLerp;
            m_playerController.m_playerMovement.m_crouchTimer = 0;

            m_playerController.m_playerMovement.m_groundCheckHeightAdjustment = (m_playerController.m_playerMovement.m_colliderWalkHeight - m_playerController.m_playerMovement.m_colliderCrouchHeight) / 2;
            m_playerController.m_playerMovement.m_groundCheckTransform.position = new Vector3(m_playerController.m_playerMovement.m_groundCheckTransform.position.x, transform.position.y + m_playerController.m_playerMovement.m_groundCheckHeightAdjustment, m_playerController.m_playerMovement.m_groundCheckTransform.position.z);
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerController.m_playerMovement.m_permitCrouchLerp)
            {
                m_playerController.m_playerMovement.m_kneelToCrouch = false;
            }

            m_playerController.m_playerMovement.m_groundCheckTransform.position = transform.position;
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
            m_playerController.m_cameraBehaviour.m_zoomValueY = _callbackContext.ReadValue<Vector2>().y * m_playerController.m_cameraBehaviour.m_zoomSpeed;
            //float readValueY = -_callbackContext.ReadValue<Vector2>().y * m_playerController.m_cameraBehaviour.m_zoomSpeed;
        }

        private void StopCameraZoom(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_cameraBehaviour.m_zoomValueY = 0.0f;
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