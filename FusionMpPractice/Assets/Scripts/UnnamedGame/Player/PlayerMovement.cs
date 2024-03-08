using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace PlayerInputManagement
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;
        //[SerializeField] private InputActionReference[] m_mouseRotationActionMaps;

        #region MoveCharacter-Variables
        [Header("Movement")]
        [SerializeField] internal float m_walkSpeed = 5f;
        [SerializeField] internal float m_runSpeed = 10f;
        [SerializeField] internal float m_crouchSpeed = 2.5f;
        internal float m_stopMovementValue = 0.0f;
        [SerializeField] internal float m_jumpForce = 10f;
        [SerializeField] internal float m_kneelTime = 0.1f;
        [SerializeField] internal float m_moveSpeedLerpTime = 0.5f;
        [SerializeField] private float m_rotationSpeed = 5.0f;
        /*[SerializeField] */
        internal bool m_blockRotation = false;
        internal Vector3 m_horizontalMovement, m_characterRotation;

        #region Acceleration
        [Header("Acceleration")]
        [SerializeField] internal float m_durationToMaxSpeed = 2.5f;
        [SerializeField] internal float m_durationToZeroSpeed = 6.0f;
        [SerializeField] internal float m_breakToZeroSpeed = 1.0f;
        internal float m_acceleRatePerSec, m_deceleRatePerSec, m_breakRatePerSec, m_currentVelocity, m_individualMaxSpeed;
        internal bool m_activeBreaking = false;
        internal EOnFootMoveModi m_lastMoveMode;
        #endregion

        #region Gravity-Variables
        [Header("GroundCheck")]
        [SerializeField] internal LayerMask m_groundCheckLayerMask;
        [SerializeField] internal Transform m_groundCheckTransform;
        [SerializeField] internal float m_groundCheckDistance = 0.2f;
        [SerializeField] internal float m_gravityValue = -9.81f;
        [SerializeField] internal float m_coyoteTime = 0.2f;
        [SerializeField, Range(0.0001f, 2f)] internal float m_inversedGravityMultiplier = 1.0f;
        #endregion

        #region Crouch-Variables
        [Header("Crouching")]
        [SerializeField] internal LayerMask m_crouchObstacles;
        [SerializeField] internal GameObject m_currentHitObject;
        [SerializeField] internal float m_sphereRadius = 0.2f;
        [SerializeField] internal float m_colliderWalkHeight = 2.0f;
        [SerializeField] internal float m_colliderCrouchHeight = 1.0f;
        internal float m_maxDistanceAbove;
        internal bool m_obstacleIsAbove, m_permitCrouchLerp = false, m_kneelToCrouch = false;
        internal float m_groundCheckHeightAdjustment;
        #endregion

        #region Debug.Drawline & DrawWireSphere
        [Header("Debug Drawings")]
        internal Vector3 m_lineOrigin;
        internal Vector3 m_sphereCastDirection;
        internal float m_hitCheckDistance;
        #endregion
        #endregion

        internal float m_crouchTimer;                        //Used to calculate the LerpTime to move up or down.
        internal float m_coyoteTimeCounter;                  //resets coyoteTimer on regained groundContact.

        internal float m_startMoveLerp, m_lerpTimeCounter;    //Start / End / runtime
        internal bool m_playerIsGrounded, m_moveButtonIsPressed, m_jumpIsPressed, m_shiftIsPressed = false;
        internal bool m_menuIsOpen = false;

        private void OnDisable()
        {
            m_playerController.m_playerInputActions.PlayerOnFootRH.Disable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed -= CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled -= StopJumping;
            m_playerController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed -= OpenMenu;

            InputUser.onChange -= OnInputDeviceChange;

            m_permitCrouchLerp = false;
        }

        private void Start()
        {
            m_playerController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Enable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled += StopJumping;
            m_playerController.m_playerInputActions.PlayerOnFootRH.OpenMenu.performed += OpenMenu;

            InputUser.onChange += OnInputDeviceChange;
            m_maxDistanceAbove = m_colliderWalkHeight;

            //Debug.Log($"CurrentActionMap Name {m_playerController.m_currentActionMap.name} - CurrentActionMap ID {m_playerController.m_currentActionMap.id}");
            //Debug.Log($"ControllerAction Name {m_playerController.m_playerInputActions.asset.actionMaps[0].name} - ControllerAction ID {m_playerController.m_playerInputActions.asset.actionMaps[0].id}");
            //Debug.Log($"ReferenceAction Name {m_mouseRotationActionMaps[0].action.name} - ReferenceAction ID {m_mouseRotationActionMaps[0].action.id}");
        }

        private void Update()
        {
            if (!m_playerController.m_isDead)
            {
                //YRotation with A and D.
                if (!m_blockRotation)
                    m_characterRotation = new Vector3(0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0);

                //TODO: Coyote Timer.
                //if (m_playerIsGrounded)
                //    {
                //        m_coyoteTimeCounter = m_coyoteTime;
                //    }
                //    else if (m_coyoteTimeCounter > 0)
                //    {
                //        m_coyoteTimeCounter -= Time.deltaTime;
                //    }

                Crouching();
                MoveSpeedAcceleration();
            }

            if (transform.position.y < m_playerController.m_fallLimit)
            {
                //AreaFallOffReset
                m_playerController.m_rigidbody.transform.position = m_playerController.m_repopPosition;
            }
        }

        private void FixedUpdate()
        {
            if (!m_playerController.m_isDead)
            {
                //simple Groundcheck without Arrays of hitted objects or memory allocation.
                m_playerIsGrounded = Physics.CheckSphere(m_groundCheckTransform.position, m_groundCheckDistance, m_groundCheckLayerMask);
                //m_playerController.m_playerIsGrounded = Physics.Raycast(m_playerController.m_groundCheckTransform.position, Vector3.down, m_playerController.m_groundCheckDistance, m_playerController.m_groundCheckLayerMask);

                MoveRigidbody();

                switch (m_playerIsGrounded) //Calculate FallDamage.
                {
                    case false:
                    {
                        FallDamageCalculationStart();
                        break;
                    }
                    case true:
                    {
                        FallDamageCalculationEnd();
                        break;
                    }
                }
            }
        }

        #region Custom Methods
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Debug.DrawLine(m_lineOrigin, m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance);
            Gizmos.DrawWireSphere(m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance, m_sphereRadius);
        }
#endif

        private void MoveRigidbody()
        {
            switch (m_blockRotation)
            {
                case false: //m_characterRotation set the Y-Rotation with PlayerOnFootRH.Movement.ReadValue<Vector2>().x.
                {
                    m_horizontalMovement = new(0, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    break;
                }
                case true:
                {
                    m_horizontalMovement = new(m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    m_characterRotation.y = 0;
                    break;
                }
            }

            m_horizontalMovement = m_playerController.m_rigidbody.transform.TransformDirection(m_horizontalMovement);

            //if (m_playerIsGrounded && m_horizontalMovement.y <= 0)
            //{
            //Einmaliges ausloesen bei wiedererlangtem Bodenkontakt ueber den InputManager.
            //if (m_jumpEventTriggered)
            //InputManager.m_RegainedGroundContact?.Invoke();
            //}

            m_playerController.m_rigidbody.MovePosition(m_playerController.m_rigidbody.transform.position + m_individualMaxSpeed * Time.fixedDeltaTime * m_horizontalMovement);

            Quaternion deltaRotation = Quaternion.Euler(0, m_characterRotation.y * Time.fixedDeltaTime * m_rotationSpeed, 0);
            m_playerController.m_rigidbody.MoveRotation(m_playerController.m_rigidbody.rotation * deltaRotation);
        }

        #region Crouching
        private void SphereCastCheckAbove()
        {
            m_lineOrigin = m_groundCheckTransform.position;
            m_sphereCastDirection = m_groundCheckTransform.up;

            m_obstacleIsAbove =
                Physics.SphereCast(m_lineOrigin, m_sphereRadius, m_sphereCastDirection, out RaycastHit hitObject, m_maxDistanceAbove, m_crouchObstacles, QueryTriggerInteraction.UseGlobal);

            switch (m_obstacleIsAbove)
            {
                case false:
                {
                    m_currentHitObject = null;
                    m_hitCheckDistance = m_maxDistanceAbove;
                    break;
                }
                case true:
                {
                    m_currentHitObject = hitObject.transform.gameObject;
                    m_hitCheckDistance = hitObject.distance;
                    break;
                }
            }
        }

        private void Crouching()
        {
            //switch (m_playerIsGrounded)
            //{
            //    case false:
            //    {
            //        if (m_coyoteTimeCounter > 0)
            //            m_coyoteTimeCounter -= Time.deltaTime;
            //        break;
            //    }
            //    case true:
            //    {
            //        m_coyoteTimeCounter = m_coyoteTime;
            //        break;
            //    }
            //}

            if (m_permitCrouchLerp)
            {
                m_crouchTimer += Time.deltaTime;
                float countingUp = m_crouchTimer / m_kneelTime;
                m_crouchTimer *= m_crouchTimer;

                SphereCastCheckAbove(); //Locks Player in 'crouch-mode', if obstacles are detected above.

                switch (m_kneelToCrouch)
                {
                    case false:
                    {
                        if (m_currentHitObject == null)
                        {
                            if (m_crouchTimer < m_kneelTime)
                            {
                                //Lerp getting up.
                                m_playerController.m_capsuleCollider.height =
                                    Mathf.Lerp(m_playerController.m_capsuleCollider.height, m_colliderWalkHeight, countingUp);
                                m_crouchTimer += Time.deltaTime;
                            }
                            else
                            {
                                m_playerController.m_capsuleCollider.height = m_colliderWalkHeight;
                                m_crouchTimer = 0.0f;
                            }
                        }

                        break;
                    }
                    case true:
                    {
                        if (m_crouchTimer < m_kneelTime)
                        {
                            //Lerp kneeling down.
                            m_playerController.m_capsuleCollider.height =
                                Mathf.Lerp(m_playerController.m_capsuleCollider.height, m_colliderCrouchHeight, countingUp);
                            m_crouchTimer += Time.deltaTime;
                        }
                        else
                            m_playerController.m_capsuleCollider.height = m_colliderCrouchHeight;
                        break;
                    }
                }
            }
        }
        #endregion
        #region Acceleration
        private void MoveSpeedAcceleration()
        {
            switch (m_playerController.m_eCurrentMoveMode)
            {
                case EOnFootMoveModi.Walking:
                {
                    m_individualMaxSpeed = m_walkSpeed;
                    break;
                }
                case EOnFootMoveModi.Running:
                    m_individualMaxSpeed = m_runSpeed;
                    break;
                case EOnFootMoveModi.Crouching:
                    m_individualMaxSpeed = m_crouchSpeed;
                    break;
                default:
                {
                    m_individualMaxSpeed = m_stopMovementValue;
#if UNITY_EDITOR
                    Debug.Log("PlayerMovement: Undefined MaxSpeed. Please set a definition for it.");
#endif
                    break;
                }
            }

            switch (m_moveButtonIsPressed)
            {
                case false:
                {
                    //m_currentVelocity += m_breakRatePerSec + Time.deltaTime;
                    //m_currentVelocity += m_deceleRatePerSec + Time.deltaTime;
                    //m_currentVelocity = Mathf.Max(m_currentVelocity, m_stopMovementValue);

                    switch (m_activeBreaking)
                    {
                        case false:
                        {
                            Acceleration(m_deceleRatePerSec);
                            break;
                        }
                        case true:
                        {
                            Acceleration(m_breakRatePerSec);
                            break;
                        }
                    }

                    break;
                }
                case true:
                {
                    //m_currentVelocity += m_acceleRatePerSec + Time.deltaTime;
                    //m_currentVelocity = Mathf.Min(m_currentVelocity, m_individualMaxSpeed);
                    Acceleration(m_acceleRatePerSec);
                    break;
                }
            }
        }

        private void Acceleration(float _accelerationValue)
        {
            m_currentVelocity += _accelerationValue + Time.deltaTime;
            m_currentVelocity = Mathf.Clamp(m_currentVelocity, /*m_playerController.*/m_stopMovementValue, m_individualMaxSpeed);
        }

        internal void SetTargetSpeedMode(float _currentMoveSpeed, EOnFootMoveModi _lastMoveMode = EOnFootMoveModi.Idle, EOnFootMoveModi _targetMoveMode = EOnFootMoveModi.Walking)
        {
            m_lastMoveMode = _lastMoveMode;
            m_startMoveLerp = _currentMoveSpeed;

            switch (_lastMoveMode)
            {
                case EOnFootMoveModi.Idle:
                {
                    break;
                }
                case EOnFootMoveModi.Walking:
                {
                    m_deceleRatePerSec = -m_walkSpeed / m_durationToZeroSpeed;
                    m_breakRatePerSec = -m_walkSpeed / m_breakToZeroSpeed;
                    break;
                }
                case EOnFootMoveModi.Running:
                {
                    m_deceleRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                    m_breakRatePerSec = -m_runSpeed / m_breakToZeroSpeed;
                    break;
                }
                case EOnFootMoveModi.Crouching:
                {
                    m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                    m_breakRatePerSec = -m_crouchSpeed / m_breakToZeroSpeed;
                    break;
                }
                default:
                    break;
            }

            switch (_targetMoveMode)
            {
                case EOnFootMoveModi.Walking:
                {
                    m_acceleRatePerSec = m_walkSpeed / m_durationToMaxSpeed;
                    break;
                }
                case EOnFootMoveModi.Running:
                {
                    m_acceleRatePerSec = m_runSpeed / m_durationToMaxSpeed;
                    break;
                }
                case EOnFootMoveModi.Crouching:
                {
                    m_acceleRatePerSec = m_crouchSpeed / m_durationToMaxSpeed;
                    break;
                }
                default:
                    break;
            }

            m_playerController.m_eCurrentMoveMode = _targetMoveMode;
        }
        #endregion
        #region Fall-Damage
        private void FallDamageCalculationStart()
        {
            if (!m_playerController.m_isGroundContactLost)
            {
                m_playerController.m_lostGroundContact.y = transform.position.y - m_groundCheckDistance;
                m_playerController.m_isGroundContactLost = true;
                m_playerController.m_allowApplyingDamageOnce = true;
            }
        }

        private void FallDamageCalculationEnd()
        {
            if (m_playerController.m_isGroundContactLost && m_playerController.m_allowApplyingDamageOnce)
            {
                m_playerController.m_regainedGroundContact.y = transform.position.y - m_groundCheckDistance;
                m_playerController.m_isGroundContactLost = false;
                CalculateFallDamage();
            }
        }

        private void CalculateFallDamage()
        {
            //TODO: Calculate the final FallDamage.

            ////Minimale Anpassung der Falldistanz, je nach Absprung oder Vorwärtslaufen mit Sicht auf die GroundCheck-Sphäre.
            ////Bessere Anpassung gehen vielleicht mit mehr Wissen.
            //if ((m_lostGroundContact.y - m_regainedGroundContact.y) >= m_minFallDistance && m_fallDamageEnabled)
            //{
            //    if (m_rigidbody.velocity.y > 0)
            //    {
            //        m_finalFallDistance = m_lostGroundContact.y - m_regainedGroundContact.y - m_groundCheckDistance;
            //    }
            //    else
            //    {
            //        m_finalFallDistance = m_lostGroundContact.y - m_regainedGroundContact.y + m_groundCheckDistance;
            //    }

            //    //Die sofortige Blockierung der Schadensübertragung auf den Player, wird die Falldistanz nur einmal übergeben.
            //    //Damit die Health-Komponente den Schaden verarbeiten kann, wird er in einer Extra-Methode erst einmal berechnet. 
            //    if (m_allowApplyingDamageOnce)
            //    {
            //        m_allowApplyingDamageOnce = false;
            //        ApplyFallDamage(m_finalFallDistance);
            //    }
            //}
        }
        #endregion        
        #endregion
        #region CallbackContexts
        #region InputDeviceChange
        private void OnInputDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            //TODO: Possible Notifications on changing the inpunt device.
        }
        #endregion
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
            m_jumpIsPressed = _callbackContext.ReadValueAsButton();
            if (m_jumpIsPressed && m_playerIsGrounded) //m_playerIsGrounded <-> m_coyoteTimeCounter.
            {
                //Coyotetime Jump with more following.
                m_playerController.m_rigidbody.AddForce(Vector3.up * Mathf.Sqrt(m_jumpForce * -m_inversedGravityMultiplier * m_gravityValue), ForceMode.Impulse);

                ////Einmaliges ausloesen bei verlorenem Bodenkontakt ueber den InputManager.
                //InputManager.m_LostGroundContact?.Invoke();

                ////Verzoegertes Ruecksetzen des Jump-Bools mit 'm_coyoteTimeCounter'.
                //if (m_jumpIsPressed && m_coyoteTimeCounter > 0)
                //    m_jumpIsPressed = false;
            }
        }

        private void StopJumping(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_jumpIsPressed = false;
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