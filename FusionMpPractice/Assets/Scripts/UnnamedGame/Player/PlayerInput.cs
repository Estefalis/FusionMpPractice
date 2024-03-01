using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerInput : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        //private InputControlScheme m_inputControlScheme;
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private CursorLockMode m_cursorLockMode;

        private void Awake()
        {
            Cursor.lockState = m_cursorLockMode;
        }

        private void OnDisable()
        {
            m_playerInputActions.PlayerOnFoot.Disable();
            m_playerInputActions.PlayerOnFoot.Movement.performed -= MoveCharacter;
            m_playerInputActions.PlayerOnFoot.Movement.canceled -= StopMovement;
            m_playerInputActions.PlayerOnFoot.ActiveBreaking.performed -= ActiveBreaking;
            m_playerInputActions.PlayerOnFoot.ActiveBreaking.canceled -= CancelActiveBreaking;
            m_playerInputActions.PlayerOnFoot.Rotation.performed -= RotateCharacter;
            m_playerInputActions.PlayerOnFoot.Rotation.canceled -= StopRotation;
            m_playerInputActions.PlayerOnFoot.Duck.performed -= CharacterDuck;
            m_playerInputActions.PlayerOnFoot.Duck.canceled -= StopDucking;
            m_playerInputActions.PlayerOnFoot.Acceleration.performed -= AccelerateMovespeed;
            m_playerInputActions.PlayerOnFoot.Acceleration.canceled -= DecelerateMovespeed;
            m_playerInputActions.PlayerOnFoot.Menu.performed -= OpenMenu;
            m_playerInputActions.PlayerOnFoot.CursorLockMode.performed -= SwitchCursorLockMode;

            InputUser.onChange -= OnInputDeviceChange;

            m_playerController.m_playerMovement.m_permitCrouchLerp = false;
        }

        private void Start()
        {
            m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerInputActions.PlayerOnFoot.Enable();
            m_playerInputActions.PlayerOnFoot.Movement.performed += MoveCharacter;
            m_playerInputActions.PlayerOnFoot.Movement.canceled += StopMovement;
            m_playerInputActions.PlayerOnFoot.ActiveBreaking.performed += ActiveBreaking;
            m_playerInputActions.PlayerOnFoot.ActiveBreaking.canceled += CancelActiveBreaking;
            m_playerInputActions.PlayerOnFoot.Rotation.performed += RotateCharacter;
            m_playerInputActions.PlayerOnFoot.Rotation.canceled += StopRotation;
            m_playerInputActions.PlayerOnFoot.Duck.performed += CharacterDuck;
            m_playerInputActions.PlayerOnFoot.Duck.canceled += StopDucking;
            m_playerInputActions.PlayerOnFoot.Acceleration.performed += AccelerateMovespeed;
            m_playerInputActions.PlayerOnFoot.Acceleration.canceled += DecelerateMovespeed;
            m_playerInputActions.PlayerOnFoot.Menu.performed += OpenMenu;
            m_playerInputActions.PlayerOnFoot.CursorLockMode.performed += SwitchCursorLockMode;

            InputUser.onChange += OnInputDeviceChange;
        }

        #region CallbackContexts        
        #region Normal Acceleration
        //Set normal moveSpeed by pressing WASD and controller relatives.
        private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_moveButtonIsPressed = true;
            //m_inputVector = _callbackContext.ReadValue<Vector2>();
        }

        private void StopMovement(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_moveButtonIsPressed = false;
            //m_inputVector = Vector2.zero;
        }
        #endregion
        #region Active Breaking
        private void ActiveBreaking(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_activeBreaking = true;
        }
        private void CancelActiveBreaking(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_activeBreaking = false;
        }
        #endregion
        #region Increased Acceleration
        //Set fast moveSpeed by pressing shift and controller relatives.
        private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode, EMoveModi.Running);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EMoveModi.Running);
            m_playerController.m_playerMovement.m_lerpTimeCounter = 0.0f;
            m_playerController.m_playerMovement.m_shiftIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode);
            m_playerController.m_playerMovement.m_shiftIsPressed = false;
        }
        #endregion
        #region Rotation
        private void RotateCharacter(InputAction.CallbackContext _callbackContext)
        {
            //_callbackContext.ReadValue<Vector2>();
        }

        private void StopRotation(InputAction.CallbackContext _callbackContext)
        {

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

            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode, EMoveModi.Crouching);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EMoveModi.Crouching);
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerController.m_playerMovement.m_permitCrouchLerp)
            {
                m_playerController.m_playerMovement.m_kneelToCrouch = false;
            }

            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode);
            m_playerController.m_playerMovement.m_groundCheckTransform.position = transform.position;
        }
        #endregion
        #region CursorLockMode
        private void SwitchCursorLockMode(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion
        #region Menu
        private void OpenMenu(InputAction.CallbackContext _callbackContext)
        {

        }
        #endregion
        #region InputDeviceChange
        private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            //TODO: Possible Notifications on changing the inpunt device.
        }
        #endregion
        #endregion
    }
}