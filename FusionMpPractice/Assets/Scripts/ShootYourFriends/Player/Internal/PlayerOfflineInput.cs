using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.Users;

namespace PlayerManagement
{
    public class PlayerOfflineInput : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] private PlayerOfflineController m_playerOfflineController;

        #region Network
        private float m_rightInput, m_rotationInput, m_forwardInput;   //Building new MoveVector(s) in combination.
        #endregion

        private ERigidbodyMoveMethod m_ePreviousMoveMethod;

        private void OnDisable()
        {
            if (transform.gameObject.activeInHierarchy)
            {
                m_playerInputActions.PlayerOnFoot.Disable();

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
            //m_objectId = Object.Id;

            if (transform.gameObject.activeInHierarchy)
            {
                m_playerInputActions = InputManager.m_InputManagerActions;
                m_playerInputActions.PlayerOnFoot.Enable();

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
            RetrieveUserInput();  //Modular Setup of Vectors for individual Movement.
            CameraRotation();
        }

        #region Custom Methods
        private void CameraRotation()
        {
            m_playerOfflineController.m_cameraOfflineBehaviour.m_playerInputRotationVector =
                new Vector3(-m_playerInputActions.PlayerOnFoot.CameraMovement.ReadValue<Vector2>().x, m_playerInputActions.PlayerOnFoot.CameraMovement.ReadValue<Vector2>().y, 0.0f);
        }

        private void RetrieveUserInput()
        {
            switch (m_playerOfflineController.m_eRigidbodyMoveMethod)
            {
                case ERigidbodyMoveMethod.Basic:
                {
                    m_rightInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;
                    m_rotationInput = 0.0f;
                    m_forwardInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;
                    break;
                }
                case ERigidbodyMoveMethod.KbRotateY:
                {
                    m_rightInput = 0.0f;
                    m_rotationInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;           //A & D
                    m_forwardInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.MouseRotateY:
                {
                    m_rightInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInput = m_playerInputActions.PlayerOnFoot.Rotation.ReadValue<Vector2>().x;           //MouseX Rot Y
                    m_forwardInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.Locked:
                {
                    m_rightInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInput = 0.0f;
                    m_forwardInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                case ERigidbodyMoveMethod.Relative:
                {
                    m_rightInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().x;              //A & D
                    m_rotationInput = 0.0f;
                    m_forwardInput = m_playerInputActions.PlayerOnFoot.Movement.ReadValue<Vector2>().y;            //W & S
                    break;
                }
                default:
                    break;
            }

            m_playerOfflineController.m_playerOfflineMovement.m_moveVector = new Vector3(m_rightInput, m_rotationInput, m_forwardInput);
        }
        #endregion

        #region CallbackContexts        
        #region Normal Acceleration
        private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_moveButtonIsPressed = true;
        }

        private void StopMovement(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_moveButtonIsPressed = false;
        }
        #endregion
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_JumpButtonIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_JumpButtonIsPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_KneelButtonIsPressed = _callbackContext.ReadValueAsButton();
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_playerOfflineMovement.m_KneelButtonIsPressed = _callbackContext.ReadValueAsButton();
        }
        #endregion
        #region Rotation
        private void OnRightMouseButtonDown(InputAction.CallbackContext _callbackContext)
        {
            m_ePreviousMoveMethod = m_playerOfflineController.m_eRigidbodyMoveMethod;
            m_playerOfflineController.m_eRigidbodyMoveMethod = ERigidbodyMoveMethod.Locked;
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_eRigidbodyMoveMethod = m_ePreviousMoveMethod;
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
        //private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        //{
        //    //TODO: Possible Notifications on changing the input device.
        //}
        #endregion
        #region Camera Zoom
        private void ZoomCamera(InputAction.CallbackContext _callbackContext)
        {
            m_playerOfflineController.m_cameraOfflineBehaviour.m_zoomScrollValue = _callbackContext.ReadValue<Vector2>().y * m_playerOfflineController.m_cameraOfflineBehaviour.m_zoomSpeed;
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