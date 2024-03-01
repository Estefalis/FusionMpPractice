using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerMovementCC : MonoBehaviour
    {
        //private PlayerInputActions m_playerInputActions;
        //private InputControlScheme m_inputControlScheme;

        //[SerializeField] private PlayerController m_playerController;
        //[SerializeField] private CharacterController m_characterController;

        //[SerializeField] private CursorLockMode m_cursorLockMode;

        //#region MoveCharacter-Variables
        //[Header("Movement")]
        //[SerializeField] private float m_walkSpeed = 5f;
        //[SerializeField] private float m_runSpeed = 10f;
        //[SerializeField] private float m_crouchSpeed = 2.5f;
        //private float m_stopMovementValue = 0.0f;
        //[SerializeField] private float m_jumpForce = 11.5f;
        //[SerializeField] private float m_kneelTime = 0.1f;
        //[SerializeField] private float m_moveSpeedLerpTime = 0.5f;

        //private float m_startMoveLerp, m_lerpTimeCounter;    //Start / End / runtime
        //private bool m_playerIsGrounded, m_moveButtonIsPressed, m_jumpIsPressed, m_shiftIsPressed = false;

        //#region Acceleration
        //[Header("Acceleration")]
        //[SerializeField] private float m_durationToMaxSpeed;
        //[SerializeField] private float m_durationToZeroSpeed;
        //[SerializeField] private float m_breakToZeroSpeed;
        //private float /*m_lerpedMoveSpeed,*/ m_acceleRatePerSec, m_deceleRatePerSec, m_breakRatePerSec, m_currentVelocity, m_individualMaxSpeed;
        //private bool m_activeBreaking = false;
        //private EMoveModi m_lastMoveMode;
        //#endregion
        //#endregion

        //#region Gravity-Variables
        //[Header("Gravity")]
        //[SerializeField] private LayerMask m_groundCheckLayerMask;
        //[SerializeField] private Transform m_groundCheckTransform;
        //[SerializeField] private float m_groundCheckDistance = 0.2f;
        //[SerializeField] private float m_gravityValue = -9.81f;
        //[SerializeField] private float m_coyoteTime = 0.2f;
        //[SerializeField, Range(0.0001f, 2f)] private float m_inversedGravityMultiplier = 1.0f;
        //#endregion

        //#region Crouch-Variables
        //[Header("Crouching")]
        //[SerializeField] private LayerMask m_crouchObstacles;
        //[SerializeField] private GameObject m_currentHitObject;
        //[SerializeField] private float m_maxDistanceAbove;
        //[SerializeField] private float m_sphereRadius;
        //[SerializeField] private float m_colliderWalkHeight;
        //[SerializeField] private float m_colliderCrouchHeight;
        //private bool m_obstacleIsAbove, m_permitCrouchLerp = false, m_kneelToCrouch = false;
        //private float m_groundCheckHeightAdjustment;
        //#endregion

        //#region Debug.Drawline & DrawWireSphere
        //[Header("Debug Drawings")]
        //private Vector3 m_lineOrigin;
        //private Vector3 m_sphereCastDirection;
        //private float m_hitCheckDistance;
        //#endregion

        //#region Fall-Damage
        //[Header("Fall Damage")]
        //[SerializeField] private int m_minFallDistance = 5;             //MinimumDistance to take damage.
        //[SerializeField] private float m_fallDamageMultiplier = 1;      //Adjustment-variable.
        //[SerializeField] private float m_finalFallDistance;             //Calculated fallDamage.
        //[SerializeField] private bool m_fallDamageEnabled = true;

        //private bool m_allowApplyingDamageOnce = false;
        //private bool m_allowFallDistanceRecord = false;
        //private Vector3 m_lostGroundContact;
        //private Vector3 m_regainedGroundContact;
        //#endregion

        //#region Runtime-Values
        //private float m_crouchTimer;                        //Used to calculate the LerpTime to move up or down.
        //private float m_coyoteTimeCounter;                  //resets coyoteTimer on regained groundContact.
        //private Vector3 m_startPosition;
        //private Vector3 m_playerMoveVector;

        //#region Reset on falling off the area
        //[Header("Area Fall Off Reset")]
        //[SerializeField] private Vector3 m_repopPosition;
        //[SerializeField] private float m_fallLimit = -100f;
        //#endregion
        //#endregion

        #region Runtime-Values
        ////Waehrend des Springens soll ein Wert von 0.0f verhindern, dass der CharacterController automatisch Kanten hochgleitet.
        //private float m_controllerStepOffset = 1f;
        #endregion

        private void Awake()
        {
            //if (m_characterController != null)
            //    m_characterController = GetComponent<CharacterController>();

            //m_startPosition = transform.position;
            //Cursor.lockState = m_cursorLockMode;

            //m_lerpedMoveSpeed = m_walkSpeed;
            //m_currentVelocity = m_walkSpeed;
            //SetTargetSpeedMode(m_stopMovementValue, EMoveModi.Walking);
        }

        //private void OnEnable()
        //{
        //    InputUser.onChange += OnInputDeviceChange;
        //}

        //private void OnDisable()
        //{
        //    m_playerInputActions.PlayerControl.Disable();
        //    m_playerInputActions.PlayerControl.Movement.performed -= MoveCharacter;
        //    m_playerInputActions.PlayerControl.Movement.canceled -= StopMovement;
        //    m_playerInputActions.PlayerControl.ActiveBreaking.performed -= ActiveBreaking;
        //    m_playerInputActions.PlayerControl.ActiveBreaking.canceled -= CancelActiveBreaking;
        //    m_playerInputActions.PlayerControl.Rotation.performed -= RotateCharacter;
        //    m_playerInputActions.PlayerControl.Rotation.canceled -= StopRotation;
        //    m_playerInputActions.PlayerControl.Jump.performed -= CharacterJump;
        //    m_playerInputActions.PlayerControl.Jump.canceled -= StopJumping;
        //    m_playerInputActions.PlayerControl.Duck.performed -= CharacterDuck;
        //    m_playerInputActions.PlayerControl.Duck.canceled -= StopDucking;
        //    m_playerInputActions.PlayerControl.Acceleration.performed -= AccelerateMovespeed;
        //    m_playerInputActions.PlayerControl.Acceleration.canceled -= DecelerateMovespeed;
        //    m_playerInputActions.PlayerControl.SwitchMoveModi.performed -= SwitchMoveMode;          //May gets replaced or removed it.
        //    m_playerInputActions.PlayerControl.Menu.performed -= OpenMenu;
        //    m_playerInputActions.PlayerControl.CursorLockMode.performed -= SwitchCursorLockMode;

        //    InputUser.onChange -= OnInputDeviceChange;

        //    m_permitCrouchLerp = false;
        //}

        //private void Start()
        //{
        //    m_playerInputActions = InputManager.m_InputManagerActions;
        //    m_playerInputActions.PlayerControl.Enable();
        //    m_playerInputActions.PlayerControl.Movement.performed += MoveCharacter;
        //    m_playerInputActions.PlayerControl.Movement.canceled += StopMovement;
        //    m_playerInputActions.PlayerControl.ActiveBreaking.performed += ActiveBreaking;
        //    m_playerInputActions.PlayerControl.ActiveBreaking.canceled += CancelActiveBreaking;
        //    m_playerInputActions.PlayerControl.Rotation.performed += RotateCharacter;
        //    m_playerInputActions.PlayerControl.Rotation.canceled += StopRotation;
        //    m_playerInputActions.PlayerControl.Jump.performed += CharacterJump;
        //    m_playerInputActions.PlayerControl.Jump.canceled += StopJumping;
        //    m_playerInputActions.PlayerControl.Duck.performed += CharacterDuck;
        //    m_playerInputActions.PlayerControl.Duck.canceled += StopDucking;
        //    m_playerInputActions.PlayerControl.Acceleration.performed += AccelerateMovespeed;
        //    m_playerInputActions.PlayerControl.Acceleration.canceled += DecelerateMovespeed;
        //    m_playerInputActions.PlayerControl.SwitchMoveModi.performed += SwitchMoveMode;          //May gets replaced or removed it.
        //    m_playerInputActions.PlayerControl.Menu.performed += OpenMenu;
        //    m_playerInputActions.PlayerControl.CursorLockMode.performed += SwitchCursorLockMode;
        //}

        //private void Update()
        //{
        //    if (!m_playerController.m_isDead)
        //    {
        //        Crouching();
        //        MoveSpeedAcceleration();
        //    }

        //    if (transform.position.y < m_fallLimit)
        //    {
        //        AreaFallOffReset();
        //    }
        //}

        //private void FixedUpdate()
        //{
        //    if (!m_playerController.m_isDead)
        //    {
        //        //simple Groundcheck without Arrays of hitted objects or memory allocation.
        //        //m_playerIsGrounded = Physics.CheckSphere(m_groundCheckTransform.position, m_groundCheckDistance, m_groundCheckLayerMask);
        //        m_playerIsGrounded = Physics.Raycast(m_groundCheckTransform.position, Vector3.down, m_groundCheckDistance, m_groundCheckLayerMask);

        //        MoveCharacterController();

        //        switch (m_playerIsGrounded) //Calculate FallDamage.
        //        {
        //            case false:
        //            {
        //                FallDamageCalculationStart();
        //                break;
        //            }
        //            case true:
        //            {
        //                FallDamageCalculationEnd();
        //                break;
        //            }
        //        }
        //    }
        //}

        //#region Custom Methods
        //#region MoveCharacter
        //private void MoveCharacterController()
        //{
        //    //Implement mouseMovement switch here for a combinedDevice rotation?
        //    Vector3 horizontalMovement = new Vector3(m_playerInputActions.PlayerControl.Movement.ReadValue<Vector2>().x, 0, m_playerInputActions.PlayerControl.Movement.ReadValue<Vector2>().y);

        //    horizontalMovement = transform.TransformDirection(horizontalMovement);
        //    m_characterController.Move(m_currentVelocity * Time.fixedDeltaTime * horizontalMovement);
        //    //m_characterController.Move(m_lerpedMoveSpeed * Time.fixedDeltaTime * horizontalMovement);

        //    if (m_playerIsGrounded && m_playerMoveVector.y <= 0)
        //    {
        //        m_playerMoveVector.y = -4.0f;

        //        ////Einmaliges ausloesen bei wiedererlangtem Bodenkontakt ueber den InputManager.
        //        //if (m_jumpEventTriggered)
        //        //    InputManager.m_RegainedGroundContact?.Invoke();
        //    }

        //    m_playerMoveVector.y += m_gravityValue * Time.fixedDeltaTime;           //Apply continous gravity.
        //    m_characterController.Move(m_playerMoveVector * Time.fixedDeltaTime * m_currentVelocity);
        //}
        //#endregion

        //#region Crouching
        //private void SphereCastCheckAbove()
        //{
        //    m_lineOrigin = m_groundCheckTransform.position;
        //    m_sphereCastDirection = m_groundCheckTransform.up;

        //    m_obstacleIsAbove = Physics.SphereCast(m_lineOrigin, m_sphereRadius, m_sphereCastDirection, out RaycastHit hitObject, m_maxDistanceAbove, m_crouchObstacles, QueryTriggerInteraction.UseGlobal);

        //    switch (m_obstacleIsAbove)
        //    {
        //        case false:
        //        {
        //            m_currentHitObject = null;
        //            m_hitCheckDistance = m_maxDistanceAbove;
        //            break;
        //        }
        //        case true:
        //        {
        //            m_currentHitObject = hitObject.transform.gameObject;
        //            m_hitCheckDistance = hitObject.distance;
        //            break;
        //        }
        //    }
        //}

        //private void Crouching()
        //{
        //    //switch (m_playerIsGrounded)
        //    //{
        //    //    case false:
        //    //    {
        //    //        if (m_coyoteTimeCounter > 0)
        //    //            m_coyoteTimeCounter -= Time.deltaTime;
        //    //        break;
        //    //    }
        //    //    case true:
        //    //    {
        //    //        m_coyoteTimeCounter = m_coyoteTime;
        //    //        break;
        //    //    }
        //    //}

        //    if (m_permitCrouchLerp)
        //    {
        //        m_crouchTimer += Time.deltaTime;
        //        float countingUp = m_crouchTimer / m_kneelTime;
        //        m_crouchTimer *= m_crouchTimer;

        //        SphereCastCheckAbove(); //Locks Player in 'crouch-mode', if obstacles are detected above.

        //        #region Option 1
        //        //if (m_kneelToCrouch)
        //        //{
        //        //    //Passt 1. temporaeren Wert an 2. Zielwert an. Abwaertsbewegung (knien).
        //        //    m_characterController.height = Mathf.Lerp(m_characterController.height, m_colliderCrouchHeight, countingUp);
        //        //}
        //        //else
        //        //{
        //        //    if (!m_kneelToCrouch && m_currentHitObject == null)
        //        //    {
        //        //        //Passt 1. temporaeren Wert an 2. Zielwert an. Aufwaertsbewegung (aufstehen), wenn kein Objekt ueber Player ist.
        //        //        m_characterController.height = Mathf.Lerp(m_characterController.height, m_colliderWalkHeight, countingUp);
        //        //    }
        //        //}
        //        #endregion

        //        #region Option 2
        //        switch (m_kneelToCrouch)
        //        {
        //            case false:
        //            {
        //                if (m_currentHitObject == null)
        //                {
        //                    if (m_crouchTimer < m_kneelTime)
        //                    {
        //                        //Lerp getting up.
        //                        m_characterController.height = Mathf.Lerp(m_characterController.height, m_colliderWalkHeight, countingUp);
        //                        m_crouchTimer += Time.deltaTime;
        //                    }
        //                    else
        //                    {
        //                        m_characterController.height = m_colliderWalkHeight;
        //                        m_crouchTimer = 0.0f;
        //                    }
        //                }

        //                break;
        //            }
        //            case true:
        //            {
        //                if (m_crouchTimer < m_kneelTime)
        //                {
        //                    //Lerp kneeling down.
        //                    m_characterController.height = Mathf.Lerp(m_characterController.height, m_colliderCrouchHeight, countingUp);
        //                    m_crouchTimer += Time.deltaTime;
        //                }
        //                else
        //                    m_characterController.height = m_colliderCrouchHeight;
        //                break;
        //            }
        //        }
        //        #endregion
        //    }
        //}
        //#endregion

        //#region Acceleration
        //private void MoveSpeedAcceleration()
        //{
        //    switch (m_playerController.m_eCurrentMoveMode)
        //    {
        //        case EMoveModi.Walking:
        //        {
        //            m_individualMaxSpeed = m_walkSpeed;
        //            break;
        //        }
        //        case EMoveModi.Running:
        //            m_individualMaxSpeed = m_runSpeed;
        //            break;
        //        case EMoveModi.Crouching:
        //            m_individualMaxSpeed = m_crouchSpeed;
        //            break;
        //        default:
        //        {
        //            m_individualMaxSpeed = m_stopMovementValue;
        //            Debug.Log("PlayerMovement: Undefined MaxSpeed. Please set a definition for it.");
        //            break;
        //        }
        //    }

        //    switch (m_moveButtonIsPressed)
        //    {
        //        case false:
        //        {
        //            //m_currentVelocity += m_breakRatePerSec + Time.deltaTime;
        //            //m_currentVelocity += m_deceleRatePerSec + Time.deltaTime;
        //            //m_currentVelocity = Mathf.Max(m_currentVelocity, m_stopMovementValue);

        //            switch (m_activeBreaking)
        //            {
        //                case false:
        //                {
        //                    Acceleration(m_deceleRatePerSec);
        //                    break;
        //                }
        //                case true:
        //                {
        //                    Acceleration(m_breakRatePerSec);
        //                    break;
        //                }
        //            }
        //            //TODO: Think over CharController / RB - Velocity.
        //            break;
        //        }
        //        case true:
        //        {
        //            //m_currentVelocity += m_acceleRatePerSec + Time.deltaTime;
        //            //m_currentVelocity = Mathf.Min(m_currentVelocity, m_individualMaxSpeed);
        //            Acceleration(m_acceleRatePerSec);
        //            break;
        //        }
        //    }
        //}

        //private void SetTargetSpeedMode(float _currentMoveSpeed, EMoveModi _lastMoveMode = EMoveModi.Idle, EMoveModi _targetMoveMode = EMoveModi.Walking)
        //{
        //    m_lastMoveMode = _lastMoveMode;
        //    m_startMoveLerp = _currentMoveSpeed;

        //    switch (_lastMoveMode)
        //    {
        //        case EMoveModi.Idle:
        //        {
        //            break;
        //        }
        //        case EMoveModi.Walking:
        //        {
        //            m_deceleRatePerSec = -m_walkSpeed / m_durationToZeroSpeed;
        //            m_breakRatePerSec = -m_walkSpeed / m_breakToZeroSpeed;
        //            break;
        //        }
        //        case EMoveModi.Running:
        //        {
        //            m_deceleRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
        //            m_breakRatePerSec = -m_runSpeed / m_breakToZeroSpeed;
        //            break;
        //        }
        //        case EMoveModi.Crouching:
        //        {
        //            m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
        //            m_breakRatePerSec = -m_crouchSpeed / m_breakToZeroSpeed;
        //            break;
        //        }
        //        default:
        //            break;
        //    }

        //    switch (_targetMoveMode)
        //    {
        //        case EMoveModi.Walking:
        //        {
        //            m_acceleRatePerSec = m_walkSpeed / m_durationToMaxSpeed;
        //            break;
        //        }
        //        case EMoveModi.Running:
        //        {
        //            m_acceleRatePerSec = m_runSpeed / m_durationToMaxSpeed;
        //            break;
        //        }
        //        case EMoveModi.Crouching:
        //        {
        //            m_acceleRatePerSec = m_crouchSpeed / m_durationToMaxSpeed;
        //            break;
        //        }
        //        default:
        //            break;
        //    }

        //    m_playerController.m_eCurrentMoveMode = _targetMoveMode;
        //}

        //private void Acceleration(float _accelerationValue)
        //{
        //    m_currentVelocity += _accelerationValue + Time.deltaTime;
        //    m_currentVelocity = Mathf.Clamp(m_currentVelocity, m_stopMovementValue, m_individualMaxSpeed);
        //}
        //#endregion

        //#region Fall-Damage
        //private void FallDamageCalculationStart()
        //{
        //    if (!m_allowFallDistanceRecord)
        //    {
        //        m_lostGroundContact.y = transform.position.y - m_groundCheckDistance;
        //        m_allowFallDistanceRecord = true;
        //        m_allowApplyingDamageOnce = true;
        //    }
        //}

        //private void FallDamageCalculationEnd()
        //{
        //    if (m_allowFallDistanceRecord && m_allowApplyingDamageOnce)
        //    {
        //        m_regainedGroundContact.y = transform.position.y - m_groundCheckDistance;
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
        //#endregion

        //private void AreaFallOffReset()
        //{
        //    m_characterController.enabled = false;
        //    transform.position = m_repopPosition;
        //    m_characterController.enabled = true;
        //}

        //private void OnDrawGizmosSelected()
        //{
        //    Gizmos.color = Color.yellow;
        //    Debug.DrawLine(m_lineOrigin, m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance);
        //    Gizmos.DrawWireSphere(m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance, m_sphereRadius);
        //}
        //#endregion
        //#region CallbackContexts
        //#region InputDeviceChange
        //private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        //{
        //    //TODO: Possible Notifications on changing the inpunt device.
        //}
        //#endregion
        //#region Normal Acceleration
        ////Set normal moveSpeed by pressing WASD and controller relatives.
        //private void MoveCharacter(InputAction.CallbackContext _callbackContext)
        //{
        //    m_moveButtonIsPressed = true;
        //    //m_inputVector = _callbackContext.ReadValue<Vector2>();
        //}

        //private void StopMovement(InputAction.CallbackContext _callbackContext)
        //{
        //    m_moveButtonIsPressed = false;
        //    //m_inputVector = Vector2.zero;
        //}
        //#endregion
        //#region Active Breaking
        //private void ActiveBreaking(InputAction.CallbackContext _callbackContext)
        //{
        //    m_activeBreaking = true;
        //}
        //private void CancelActiveBreaking(InputAction.CallbackContext _callbackContext)
        //{
        //    m_activeBreaking = false;
        //}
        //#endregion
        //#region Increased Acceleration
        ////Set fast moveSpeed by pressing shift and controller relatives.
        //private void AccelerateMovespeed(InputAction.CallbackContext _callbackContext)
        //{
        //    SetTargetSpeedMode(m_currentVelocity, m_playerController.m_eCurrentMoveMode, EMoveModi.Running);
        //    //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EMoveModi.Running);
        //    m_lerpTimeCounter = 0.0f;
        //    m_shiftIsPressed = _callbackContext.ReadValueAsButton();
        //}

        //private void DecelerateMovespeed(InputAction.CallbackContext _callbackContext)
        //{
        //    SetTargetSpeedMode(m_currentVelocity, m_playerController.m_eCurrentMoveMode);
        //    //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode);
        //    m_shiftIsPressed = false;
        //}
        //#endregion
        //#region MoveModeSwitch
        //private void SwitchMoveMode(InputAction.CallbackContext _callbackContext)
        //{
        //    //TODO: Implementation of manual moveMode/MoveSpeed by index.
        //}
        //#endregion
        //#region Rotation
        //private void RotateCharacter(InputAction.CallbackContext _callbackContext)
        //{
        //    //_callbackContext.ReadValue<Vector2>();
        //}

        //private void StopRotation(InputAction.CallbackContext _callbackContext)
        //{

        //}
        //#endregion
        //#region Character Jump
        //private void CharacterJump(InputAction.CallbackContext _callbackContext)
        //{
        //    m_jumpIsPressed = _callbackContext.ReadValueAsButton();

        //    //Auslesen des 'Jump-Booleans' mit '= _context.ReadValueAsButton();' und '&& m_coyoteTimeCounter > 0' in if-Abfrage, die 'm_isGrounded' darin ersetzt.
        //    if (m_jumpIsPressed && m_playerIsGrounded) //m_playerIsGrounded <-> m_coyoteTimeCounter.
        //    {
        //        //Coyotetime Jump with more following.
        //        m_playerMoveVector.y += Mathf.Sqrt(m_jumpForce * -m_inversedGravityMultiplier * m_gravityValue);

        //        ////Einmaliges ausloesen bei verlorenem Bodenkontakt ueber den InputManager.
        //        //InputManager.m_LostGroundContact?.Invoke();

        //        ////Verzoegertes Ruecksetzen des Jump-Bools mit 'm_coyoteTimeCounter'.
        //        //if (m_jumpIsPressed && m_coyoteTimeCounter > 0)
        //        //    m_jumpIsPressed = false;
        //    }
        //}

        //private void StopJumping(InputAction.CallbackContext _callbackContext)
        //{
        //    m_jumpIsPressed = false;
        //}
        //#endregion
        //#region Ducking
        //private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        //{
        //    m_permitCrouchLerp = _callbackContext.ReadValueAsButton();
        //    m_kneelToCrouch = m_permitCrouchLerp;
        //    m_crouchTimer = 0;

        //    m_groundCheckHeightAdjustment = (m_colliderWalkHeight - m_colliderCrouchHeight) / 2;
        //    m_groundCheckTransform.position = new Vector3(m_groundCheckTransform.position.x, transform.position.y + m_groundCheckHeightAdjustment, m_groundCheckTransform.position.z);

        //    SetTargetSpeedMode(m_currentVelocity, m_playerController.m_eCurrentMoveMode, EMoveModi.Crouching);
        //    //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode, EMoveModi.Crouching);
        //}

        //private void StopDucking(InputAction.CallbackContext _callbackContext)
        //{
        //    if (m_permitCrouchLerp)
        //    {
        //        m_kneelToCrouch = false;
        //    }

        //    SetTargetSpeedMode(m_currentVelocity, m_playerController.m_eCurrentMoveMode);
        //    //SetTargetSpeedMode(m_lerpedMoveSpeed, m_playerController.m_eCurrentMoveMode);
        //    m_groundCheckTransform.position = transform.position;
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
}