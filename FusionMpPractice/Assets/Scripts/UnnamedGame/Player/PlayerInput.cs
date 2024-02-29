using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInput : MonoBehaviour
{
    //internal enum EMoveModi
    //{
    //    Idle,
    //    Walking,
    //    Running,
    //    Crouching
    //}
    //private EMoveModi m_eCurrentMoveMode;

    //[SerializeField] internal CharacterController m_characterController;
    //private PlayerInputActions m_playerInputActions;
    //private InputControlScheme m_inputControlScheme;

    //[SerializeField] private PlayerController m_playerController;

    //[SerializeField] private CursorLockMode m_cursorLockMode;

    #region Movement-Variables
    //[Header("Movement")]
    //[SerializeField] private float m_playerWalkSpeed = 5f;
    //[SerializeField] private float m_playerRunSpeed = 10f;
    //[SerializeField] private float m_playerCrouchSpeed = 2.5f;
    //[SerializeField] private float m_jumpForce = 11.5f;
    //[SerializeField] private float m_kneelTime = 0.1f;

    //private bool m_permitRunning = false;
    //private Vector2 m_inputVector;
    #endregion
    //private bool m_jumpIsPressed;

    #region Gravity-Variables
    //[Header("Gravity")]
    //[SerializeField] private LayerMask m_groundCheckLayerMask;
    //[SerializeField] private Transform m_groundCheckTransform;
    //[SerializeField] private float m_groundCheckDistance = 0.2f;
    //[SerializeField] private float m_gravityValue = -9.81f;
    //[SerializeField] private float m_coyoteTime = 0.2f;
    //[SerializeField, Range(0.0001f, 2f)] private float m_inversedGravityMultiplier = 1.0f;
    #endregion

    #region Crouch-Variables
    //[Header("Crouching")]
    //[SerializeField] private LayerMask m_crouchObstacles;
    //[SerializeField] private GameObject m_currentHitObject;
    //[SerializeField] private float m_maxDistanceAbove;
    //[SerializeField] private float m_sphereRadius;
    //[SerializeField] private float m_controllerWalkHeight;
    //[SerializeField] private float m_controllerCrouchHeight;
    //private bool m_obstacleIsAbove, m_permitCrouchLerp = false, m_kneelToCrouch = false;
    #endregion    

    #region Debug.Drawline & DrawWireSphere
    //[Header("Debug Drawings")]
    //private Vector3 m_lineOrigin;
    //private Vector3 m_sphereCastDirection;
    //private float m_hitCheckDistance;
    #endregion

    #region Fall-Damage
    //[Header("Fall Damage")]
    //[SerializeField] private int m_minFallDistance = 5;             //MinimumDistance to take damage.
    //[SerializeField] private float m_fallDamageMultiplier = 1;      //Adjustment-variable.
    //[SerializeField] private float m_finalFallDistance;             //Calculated fallDamage.
    //[SerializeField] private bool m_fallDamageEnabled = true;

    //private bool m_allowApplyingDamageOnce = false;
    //private bool m_allowFallDistanceRecord = false;
    //private Vector3 m_lostGroundContact;
    //private Vector3 m_regainedGroundContact;
    #endregion

    //#region Runtime-Values
    //private float m_crouchTimer;                        //Used to calculate the LerpTime to move up or down.
    //private float m_coyoteTimeCounter;                  //resets coyoteTimer on regained groundContact.
    //private Vector3 m_startPosition;
    //private Vector3 m_playerMoveVector;
    //private bool m_isDead = false;

    //#region Reset on falling off the area
    //[Header("Area Fall Off Reset")]
    //[SerializeField] private Vector3 m_repopPosition;
    //[SerializeField] private float m_fallLimit = -100f;
    //#endregion

    private void Awake()
    {
        //if (m_playerController.m_characterController != null)
        //    m_playerController.m_characterController = GetComponent<CharacterController>();

        //m_startPosition = transform.position;
        //Cursor.lockState = m_cursorLockMode;
        //m_eCurrentMoveMode = EMoveModi.Walking;
    }

    private void OnEnable()
    {
        //InputUser.onChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        //m_playerInputActions.PlayerControl.Disable();
        //m_playerInputActions.PlayerControl.Movement.performed -= MoveCharacter;
        //m_playerInputActions.PlayerControl.Movement.canceled -= StopCharacterMovement;
        //m_playerInputActions.PlayerControl.Rotation.performed -= RotateCharacter;
        //m_playerInputActions.PlayerControl.Rotation.canceled -= StopCharacterRotation;
        //m_playerInputActions.PlayerControl.Jump.performed -= CharacterJump;
        //m_playerInputActions.PlayerControl.Jump.canceled -= StopJumping;
        //m_playerInputActions.PlayerControl.Duck.performed -= CharacterDuck;
        //m_playerInputActions.PlayerControl.Duck.canceled -= StopDucking;
        //m_playerInputActions.PlayerControl.Acceleration.performed -= AccelerateMovespeed;
        //m_playerInputActions.PlayerControl.Acceleration.canceled -= DecelerateMovespeed;
        //m_playerInputActions.PlayerControl.SwitchMoveModi.performed -= SwitchMoveMode;
        //m_playerInputActions.PlayerControl.Menu.performed -= OpenMenu;
        //m_playerInputActions.PlayerControl.CursorLockMode.performed -= SwitchCursorLockMode;

        //InputUser.onChange -= OnInputDeviceChange;
    }

    private void Start()
    {
        //yield return new WaitUntil(InputManagerIsSet);
        //m_playerInputActions = InputManager.m_InputManagerActions;
        //m_playerInputActions.PlayerControl.Enable();
        //m_playerInputActions.PlayerControl.Movement.performed += MoveCharacter;
        //m_playerInputActions.PlayerControl.Movement.canceled += StopCharacterMovement;
        //m_playerInputActions.PlayerControl.Rotation.performed += RotateCharacter;
        //m_playerInputActions.PlayerControl.Rotation.canceled += StopCharacterRotation;
        //m_playerInputActions.PlayerControl.Jump.performed += CharacterJump;
        //m_playerInputActions.PlayerControl.Jump.canceled += StopJumping;
        //m_playerInputActions.PlayerControl.Duck.performed += CharacterDuck;
        //m_playerInputActions.PlayerControl.Duck.canceled += StopDucking;
        //m_playerInputActions.PlayerControl.Acceleration.performed += AccelerateMovespeed;
        //m_playerInputActions.PlayerControl.Acceleration.canceled += DecelerateMovespeed;
        //m_playerInputActions.PlayerControl.SwitchMoveModi.performed += SwitchMoveMode;
        //m_playerInputActions.PlayerControl.Menu.performed += OpenMenu;
        //m_playerInputActions.PlayerControl.CursorLockMode.performed += SwitchCursorLockMode;

        //InputUser.onChange += OnInputDeviceChange;
    }

    //private void Update()
    //{
    //    if (m_playerIsGrounded)
    //    {
    //        m_coyoteTimeCounter = m_coyoteTime;
    //    }
    //    else if (m_coyoteTimeCounter > 0)
    //    {
    //        m_coyoteTimeCounter -= Time.deltaTime;
    //    }

    //    if (!m_isDead)
    //    {
    //        //TODO: Set/Reset CoyoteTime(r). (VoxelWarfareProject)
    //        Crouching();
    //        MoveSpeedAcceleration();
    //    }

    //    if (transform.position.y < m_fallLimit)
    //    {
    //        AreaFallOffReset();
    //    }
    //}

    private void FixedUpdate()
    {
        //if (!m_playerController.m_isDead)
        //    ProcessPlayerInput();
    }

    #region Custom Methods
    private void ProcessPlayerInput()
    {
        //MoveCharacterController();
    }

    private void MoveCharacterController()
    {
        //Vector3 horizontalMovement = new Vector3(m_inputVector.x, 0, m_inputVector.y);
        //Vector3 horizontalMovement = new Vector3(m_playerInputActions.PlayerControl.Movement.ReadValue<Vector2>().x, 0, m_playerInputActions.PlayerControl.Movement.ReadValue<Vector2>().y);        
        //m_playerController.m_keyboardMovement = horizontalMovement;

        //switch (m_eCurrentMoveMode)
        //{
        //    case EMoveModi.Idle:
        //        break;
        //    case EMoveModi.Walking:
        //    {
        //        m_characterController.Move(m_playerWalkSpeed * Time.fixedDeltaTime * horizontalMovement);
        //        break;
        //    }
        //    case EMoveModi.Running:
        //    {
        //        m_characterController.Move(m_playerRunSpeed * Time.fixedDeltaTime * horizontalMovement);
        //        break;
        //    }
        //    case EMoveModi.Crouching:
        //    {
        //        m_characterController.Move(m_playerCrouchSpeed * Time.fixedDeltaTime * horizontalMovement);
        //        break;
        //    }
        //    default:
        //        break;
        //}

        //if (m_playerIsGrounded && m_playerMoveVector.y <= 0)
        //{
        //    m_playerMoveVector.y = -4.0f;

        //    ////Einmaliges ausloesen bei wiedererlangtem Bodenkontakt ueber den InputManager.
        //    //if (m_jumpEventTriggered)
        //    //    InputManager.m_RegainedGroundContact?.Invoke();
        //}

        //m_playerMoveVector.y += m_gravityValue * Time.fixedDeltaTime;
        ////Apply continous gravity.
        //m_characterController.Move(m_playerMoveVector * Time.fixedDeltaTime);
    }

    #region DelegateBools
    //private bool InputManagerIsSet()
    //{
    //    return InputManager.InputManagerIsSet;        
    //    //return m_playerController.m_playerControllerStarted;
    //}
    #endregion
    #endregion

    #region Custom Methods


    #region Fall-Damage
    //private void FallDamageCalculationStart()
    //{
    //    if (!m_allowFallDistanceRecord)
    //    {
    //        m_lostGroundContact.y = transform.position.y;
    //        m_allowFallDistanceRecord = true;
    //        m_allowApplyingDamageOnce = true;
    //    }
    //}

    //private void FallDamageCalculationEnd()
    //{
    //    if (m_allowFallDistanceRecord && m_allowApplyingDamageOnce)
    //    {
    //        m_regainedGroundContact.y = transform.position.y;
    //        m_allowFallDistanceRecord = false;
    //    }

    //    CalculateFallDamage();
    //}

    //private void CalculateFallDamage()
    //{
    //    //TODO: Calculate the final FallDamage.

    //    ////Minimale Anpassung der Falldistanz, je nach Absprung oder Vorwärtslaufen mit Sicht auf die GroundCheck-Sphäre.
    //    ////Bessere Anpassung gehen vielleicht mit mehr Wissen.
    //    //if ((m_lostGroundContact.y - m_regainedGroundContact.y) >= m_minFallDistance && m_fallDamageEnabled)
    //    //{
    //    //    if (m_rigidbody.velocity.y > 0)
    //    //    {
    //    //        m_finalFallDistance = m_lostGroundContact.y - m_regainedGroundContact.y - m_groundCheckDistance;
    //    //    }
    //    //    else
    //    //    {
    //    //        m_finalFallDistance = m_lostGroundContact.y - m_regainedGroundContact.y + m_groundCheckDistance;
    //    //    }

    //    //    //Die sofortige Blockierung der Schadensübertragung auf den Player, wird die Falldistanz nur einmal übergeben.
    //    //    //Damit die Health-Komponente den Schaden verarbeiten kann, wird er in einer Extra-Methode erst einmal berechnet. 
    //    //    if (m_allowApplyingDamageOnce)
    //    //    {
    //    //        m_allowApplyingDamageOnce = false;
    //    //        ApplyFallDamage(m_finalFallDistance);
    //    //    }
    //    //}
    //}

    //private void ApplyFallDamage(float _finalFallDistance)
    //{
    //    _finalFallDistance *= m_fallDamageMultiplier;
    //    m_playerController.m_playerHealth.TakeDamage(/*Mathf.Round(*/_finalFallDistance/*)*/);
    //}
    #endregion

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Debug.DrawLine(m_lineOrigin, m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance);
    //    Gizmos.DrawWireSphere(m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance, m_sphereRadius);
    //}
    #endregion

    //#region CallbackContexts
    //#region InputDeviceChange
    //private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
    //{
    //    //string controlSchemeName = _inputUser.controlScheme.Value.name;
    //    //var inputDeviceName = _inputDevice.name;
    //    //var schemeValue = _inputUser.controlScheme.Value;
    //    //switch (_inputUserChange)
    //    //{
    //    //    case InputUserChange.ControlSchemeChanged:
    //    //    {
    //    //        if(m_inputControlScheme == _inputUser.controlScheme.GetValueOrDefault(schemeValue))
    //    //        break;
    //    //    }
    //    //    break;
    //    //}
    //}
    //#endregion
    //#region Movement
    //private void MoveCharacter(InputAction.CallbackContext _callbackContext)
    //{
    //    //m_inputVector = _callbackContext.ReadValue<Vector2>();
    //}

    //private void StopCharacterMovement(InputAction.CallbackContext _callbackContext)
    //{
    //    //m_inputVector = Vector2.zero;
    //}
    //#endregion
    //#region Rotation
    //private void RotateCharacter(InputAction.CallbackContext _callbackContext)
    //{
    //    //_callbackContext.ReadValue<Vector2>();
    //}

    //private void StopCharacterRotation(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region Character Jump
    //private void CharacterJump(InputAction.CallbackContext _callbackContext)
    //{

    //}

    //private void StopJumping(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region Ducking
    //private void CharacterDuck(InputAction.CallbackContext _callbackContext)
    //{

    //}

    //private void StopDucking(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region Acceleration
    //private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
    //{

    //}

    //private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region MoveModeSwitch
    //private void SwitchMoveMode(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region CursorLockMode
    //private void SwitchCursorLockMode(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#region Menu
    //private void OpenMenu(InputAction.CallbackContext _callbackContext)
    //{

    //}
    //#endregion
    //#endregion    
}