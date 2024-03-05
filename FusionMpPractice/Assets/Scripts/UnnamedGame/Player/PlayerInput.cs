using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerInput : MonoBehaviour
    {
        //private PlayerInputActions m_playerInputActions;
        //private InputControlScheme m_inputControlScheme;
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private CursorLockMode m_cursorLockMode;
        private Vector3 m_mousePosition;

        private void Awake()
        {
            Cursor.lockState = m_cursorLockMode;
        }

        private void OnDisable()
        {
            m_playerController.m_playerInputActions.PlayerOnFootRH.Disable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.performed -= MoveCharacter;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.canceled -= StopMovement;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed -= OnRightMouseButtonDown;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled -= OnRightMouseButtonUp;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBreaking.performed -= ActiveBreaking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBreaking.canceled -= CancelActiveBreaking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.performed -= CharacterDuck;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.canceled -= StopDucking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed -= AccelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled -= DecelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed -= SwitchCursorLockMode;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed -= ZoomCamera;

            InputUser.onChange -= OnInputDeviceChange;

            m_playerController.m_playerMovement.m_permitCrouchLerp = false;
        }

        private void Start()
        {
            m_playerController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Enable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.performed += MoveCharacter;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.canceled += StopMovement;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.performed += OnRightMouseButtonDown;
            m_playerController.m_playerInputActions.PlayerOnFootRH.SwitchMoveMode.canceled += OnRightMouseButtonUp;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBreaking.performed += ActiveBreaking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.ActiveBreaking.canceled += CancelActiveBreaking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.performed += CharacterDuck;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Duck.canceled += StopDucking;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.performed += AccelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Acceleration.canceled += DecelerateMovespeed;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CursorLockMode.performed += SwitchCursorLockMode;
            m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.performed += ZoomCamera;

            InputUser.onChange += OnInputDeviceChange;
        }

        private void Update()
        {
            GetMousePosition();
            SubmitCameraRotation();
        }

        private void GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            m_mousePosition = m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>();
            //or Mouse.current.position.ReadValue()
#else
            m_mousePosition = Input.mousePosition;
#endif
        }

        private void SubmitCameraRotation()
        {
            switch (m_playerController.m_currentActionMap.name) //switch (m_playerController.m_currentActionMap.id) //How to use this Map-IDs?
            {
                case "PlayerOnFootRH":
                {
                    //ReadValue.x = Rotation around YAxis.
                    m_playerController.m_cameraBehaviour.m_rotVectorYXZero =
                new Vector3(-m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().y, m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>().x, 0.0f);
                    break;
                }
                case "PlayerOnFootLH":
                {
                    //    m_playerController.m_cameraBehaviour.m_rotVectorYXZero =
                    //new Vector3(-m_playerController.m_playerInputActions.PlayerOnFootLH.Rotation.ReadValue<Vector2>().y, m_playerController.m_playerInputActions.PlayerOnFootLH.Rotation.ReadValue<Vector2>().x, 0.0f);
                    break;
                }
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
            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode, EOnFootMoveModi.Running);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EOnFootMoveModi.Running);
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
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_permitCrouchLerp = _callbackContext.ReadValueAsButton();
            m_playerController.m_playerMovement.m_kneelToCrouch = m_playerController.m_playerMovement.m_permitCrouchLerp;
            m_playerController.m_playerMovement.m_crouchTimer = 0;

            m_playerController.m_playerMovement.m_groundCheckHeightAdjustment = (m_playerController.m_playerMovement.m_colliderWalkHeight - m_playerController.m_playerMovement.m_colliderCrouchHeight) / 2;
            m_playerController.m_playerMovement.m_groundCheckTransform.position = new Vector3(m_playerController.m_playerMovement.m_groundCheckTransform.position.x, transform.position.y + m_playerController.m_playerMovement.m_groundCheckHeightAdjustment, m_playerController.m_playerMovement.m_groundCheckTransform.position.z);

            m_playerController.m_playerMovement.SetTargetSpeedMode(m_playerController.m_playerMovement.m_currentVelocity, m_playerController.m_eCurrentMoveMode, EOnFootMoveModi.Crouching);
            //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EOnFootMoveModi.Crouching);
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
        #region InputDeviceChange
        private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            //TODO: Possible Notifications on changing the inpunt device.
        }
        #endregion
        private void ZoomCamera(InputAction.CallbackContext _callbackContext)
        {
            if (!(m_mousePosition.x < (m_playerController.m_cameraBehaviour.m_camera.rect.width - m_playerController.m_cameraBehaviour.m_camera.rect.width)) &&
                !(m_mousePosition.x > m_playerController.m_cameraBehaviour.m_camera.rect.width) &&
                !(m_mousePosition.y < (m_playerController.m_cameraBehaviour.m_camera.rect.height - m_playerController.m_cameraBehaviour.m_camera.rect.height)) &&
                !(m_mousePosition.y > m_playerController.m_cameraBehaviour.m_camera.rect.height))
            {
                float readValueY = -_callbackContext.ReadValue<Vector2>().y * m_playerController.m_cameraBehaviour.m_zoomStep;

                if (Mathf.Abs(readValueY) > 0.1f)
                {
                    m_playerController.m_cameraBehaviour.m_runtimeZoomHeight = m_playerController.m_cameraBehaviour.m_camera.transform.localRotation.y + readValueY * m_playerController.m_cameraBehaviour.m_zoomStep;

                    if (m_playerController.m_cameraBehaviour.m_runtimeZoomHeight < m_playerController.m_cameraBehaviour.m_minCamHeight)
                        m_playerController.m_cameraBehaviour.m_runtimeZoomHeight = m_playerController.m_cameraBehaviour.m_minCamHeight;
                    else if (m_playerController.m_cameraBehaviour.m_runtimeZoomHeight > m_playerController.m_cameraBehaviour.m_maxCamHeight)
                        m_playerController.m_cameraBehaviour.m_runtimeZoomHeight = m_playerController.m_cameraBehaviour.m_maxCamHeight;
                }
            }
        }
        #endregion
    }
}