using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerInputManagement
{
    public class PlayerMovement : MonoBehaviour
    {
        internal enum EmoveMethod
        {
            Basic,
            Relative,
            ADRotateY,
            MouseRotateY,
            Locked
        }

        [SerializeField] internal EmoveMethod m_eMoveMethod;

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
        [SerializeField] private float m_smoothRotationTime = 15.0f;
        [SerializeField] private float m_quaternionRotTime = 600.0f;
        /*[SerializeField] */
        internal bool m_switchMoveMethod = false;
        internal Vector3 m_horizontalMovement, m_characterRotation;
        float m_mathfSmoothValue;
        private Quaternion m_targetRotation;

        #region Acceleration
        [Header("Acceleration")]
        [SerializeField] internal float m_durationToMaxSpeed = 2.5f;
        [SerializeField] internal float m_durationToZeroSpeed = 6.0f;
        [SerializeField] internal float m_brakeToZeroSpeed = 1.0f;
        internal float m_acceleRatePerSec, m_deceleRatePerSec, m_brakeRatePerSec;
        internal float m_individualMaxSpeed, m_setRunTimeMaxSpeed;
        internal bool m_activeBraking = false;
        internal EOnFootTargetMoveModi m_lastMoveMode;
        #endregion

        #region Gravity-Variables
        [Header("GroundCheck")]
        [SerializeField] internal LayerMask m_groundCheckLayerMask;
        [SerializeField] internal Transform m_groundCheckTransform;
        [SerializeField] internal float m_groundCheckDistance = 0.2f;
        [SerializeField] internal float m_gravityValue = -9.81f;
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

        #region Fall-Damage
        [Header("Fall Damage")]
        [SerializeField] internal int m_minFallDistance = 5;             //MinimumDistance to take damage.
        [SerializeField] internal float m_fallDamageMultiplier = 1;      //Adjustment-variable.
        [SerializeField] internal float m_finalFallDistance;             //Calculated fallDamage.
        [SerializeField] internal bool m_fallDamageEnabled = true;

        internal bool m_allowApplyingDamageOnce = false;
        internal bool m_isGroundContactLost = false;
        internal Vector3 m_lostGroundContactVector;
        internal Vector3 m_regainedGroundContactVector;
        #endregion

        #region Debug.Drawline & DrawWireSphere
        [Header("Debug Drawings")]
        internal Vector3 m_lineOrigin;
        internal Vector3 m_sphereCastDirection;
        internal float m_hitCheckDistance;
        #endregion
        #endregion

        #region Coyote Time
        [Header("Coyote Time")]
        [SerializeField] internal float m_coyoteTime = 0.2f;
        internal float m_crouchTimer;                        //Used to calculate the LerpTime to move up or down.
        internal float m_coyoteTimeCounter;                  //resets coyoteTimer on regained groundContact.
        #endregion

        internal bool m_playerIsGrounded, m_moveButtonIsPressed, m_jumpButtonIsPressed, m_jumpButtonIsReleased = false, m_shiftIsPressed = false;
        internal bool m_menuIsOpen = false;

        private void OnDisable()
        {
            m_playerController.m_playerInputActions.PlayerOnFootRH.Disable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed -= CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled -= OnJumpButtonRelease;

            m_permitCrouchLerp = false;
        }

        private void Start()
        {
            m_playerController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Enable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.canceled += OnJumpButtonRelease;

            m_setRunTimeMaxSpeed = 0.0f;
            m_maxDistanceAbove = m_colliderWalkHeight;
        }

        private void Update()
        {
            if (!m_playerController.m_isDead)
            {
                CoyoteTimerReSet();
                Crouching();
                SetMoveAcceleration();
            }

            if (transform.position.y < m_playerController.m_fallLimit)
            {
                m_playerController.m_rigidbody.transform.position = m_playerController.m_repopPosition; //AreaFallOffReset
            }
        }

        private void FixedUpdate()
        {
            if (!m_playerController.m_isDead)
            {
                //simple Groundcheck without Arrays of hitted objects or memory allocation.
                m_playerIsGrounded = Physics.CheckSphere(m_groundCheckTransform.position, m_groundCheckDistance, m_groundCheckLayerMask);
                //m_playerController.m_playerIsGrounded = Physics.Raycast(m_playerController.m_groundCheckTransform.position, Vector3.down, m_playerController.m_groundCheckDistance, m_playerController.m_groundCheckLayerMask);

                switch (m_eMoveMethod)
                {
                    case EmoveMethod.Basic:
                    {
                        MoveRigidbodyBasic();
                        break;
                    }
                    case EmoveMethod.Relative:
                    {
                        MoveRigidbodyRelative();
                        break;
                    }
                    case EmoveMethod.ADRotateY:
                    {
                        MoveRigidbodyAD();
                        break;
                    }
                    //TODO: Mouse Rotate Y MoveRB adden!
                    case EmoveMethod.Locked:
                    {
                        MoveRigidbodyLocked();
                        break;
                    }
                }

                //switch (m_switchMoveMethod)
                //{
                //    case false:
                //        MoveRigidbodyRelative();
                //        break;
                //    case true:
                //        MoveRigidbodyLocked();
                //        break;
                //}

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
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Debug.DrawLine(m_lineOrigin, m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance);
            Gizmos.DrawWireSphere(m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance, m_sphereRadius);
        }
#endif
        private void MoveRigidbodyBasic()
        {
            m_horizontalMovement = new(m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);

            m_playerController.m_rigidbody.MovePosition(m_playerController.m_rigidbody.transform.position + m_individualMaxSpeed * Time.fixedDeltaTime * m_horizontalMovement.normalized);

            if (m_horizontalMovement != Vector3.zero)
            {
                m_targetRotation = Quaternion.LookRotation(m_horizontalMovement, Vector3.up);
                m_targetRotation = Quaternion.RotateTowards(m_playerController.m_rigidbody.transform.rotation, m_targetRotation, m_quaternionRotTime * Time.fixedDeltaTime);
                m_playerController.m_rigidbody.MoveRotation(m_targetRotation);
            }
        }

        private void MoveRigidbodyRelative()
        {
            #region Use of RelativeHelperPositioning(){} HelperConstruct in CameraBehaviour.cs
            //Vector3 fakecameraForward = m_playerController.m_cameraBehaviour.m_relativeHelperTransform.forward;
            //Vector3 cameraRight = m_playerController.m_cameraBehaviour.m_camera.transform.right;
            ////cameraForward = cameraForward.normalized;
            //cameraRight.y = 0;    //prevents characterJumps.
            //cameraRight = cameraRight.normalized;
            //Vector3 relativeForward = m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y * fakecameraForward;
            #endregion

            #region No use of RelativeHelperPositioning(){} HelperConstruct in CameraBehaviour.cs
            Vector3 cameraForward = m_playerController.m_cameraBehaviour.m_camera.transform.forward;
            Vector3 cameraRight = m_playerController.m_cameraBehaviour.m_camera.transform.right;
            cameraForward.y = 0;   //prevents characterJumps.
            cameraRight.y = 0;    //prevents characterJumps.
            cameraForward = cameraForward.normalized;   //Rotating the camera up or down does not influence the movementSpeed anymore.
            cameraRight = cameraRight.normalized;   //Rotating the camera up or down does not influence the movementSpeed anymore.
            Vector3 relativeForward = m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y * cameraForward;
            #endregion

            Vector3 relativeRight = m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x * cameraRight;
            Vector3 relativeMoveVector = relativeRight + relativeForward;

            m_playerController.m_rigidbody.MovePosition(m_playerController.m_rigidbody.transform.position + m_individualMaxSpeed * Time.fixedDeltaTime * relativeMoveVector.normalized);

            if (relativeMoveVector != Vector3.zero)
            {
                float angle = Mathf.Atan2(relativeMoveVector.x, relativeMoveVector.z) * Mathf.Rad2Deg;
                float smoothRotation =
                    Mathf.SmoothDampAngle(m_playerController.m_rigidbody.transform.eulerAngles.y, angle, ref m_mathfSmoothValue, 1 / m_smoothRotationTime);
                m_playerController.m_rigidbody.transform.rotation = Quaternion.Euler(0, smoothRotation, 0);
            }
        }

        private void MoveRigidbodyAD()
        {
            m_horizontalMovement = new(0, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);        //W & S
            m_characterRotation = new Vector3(0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0); //A & D

            m_horizontalMovement = m_playerController.m_rigidbody.transform.TransformDirection(m_horizontalMovement);
            m_playerController.m_rigidbody.MovePosition(m_playerController.m_rigidbody.transform.position + m_individualMaxSpeed * Time.fixedDeltaTime * m_horizontalMovement.normalized);

            Quaternion deltaRotation = Quaternion.Euler(0, m_characterRotation.y * Time.fixedDeltaTime * m_quaternionRotTime, 0);
            m_playerController.m_rigidbody.MoveRotation(m_playerController.m_rigidbody.rotation * deltaRotation);
        }

        private void MoveRigidbodyLocked()
        {
            m_horizontalMovement = new(m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);

            //TODO: Lerping CameraY-Rotation to RigidbodyY-Rotation?

            m_horizontalMovement = m_playerController.m_rigidbody.transform.TransformDirection(m_horizontalMovement);

            //if (m_playerIsGrounded && m_horizontalMovement.y <= 0)
            //{
            //Einmaliges ausloesen bei wiedererlangtem Bodenkontakt ueber den InputManager.
            //if (m_jumpEventTriggered)
            //InputManager.m_RegainedGroundContact?.Invoke();
            //}

            m_playerController.m_rigidbody.MovePosition(m_playerController.m_rigidbody.transform.position + m_individualMaxSpeed * Time.fixedDeltaTime * m_horizontalMovement.normalized);
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
        private void SetMoveAcceleration()
        {
            switch (m_moveButtonIsPressed)
            {
                #region While Movement Buttons are not pressed (WASD, Left Stick).
                case false: //When no Movement button is pressed.
                {
                    switch (m_activeBraking)
                    {
                        case false: //If the character shall not stop fast.
                        {
                            switch (m_kneelToCrouch) //In case the character shall slow down from Walking or Running.
                            {
                                case false:
                                {
                                    m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Idle;
                                    m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                    m_setRunTimeMaxSpeed = m_stopMovementValue;
                                    Acceleration(m_deceleRatePerSec);
                                    break;
                                }
                                case true:
                                {
                                    m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                    m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                    m_setRunTimeMaxSpeed = m_crouchSpeed;
                                    Acceleration(m_deceleRatePerSec);
                                    break;
                                }
                            }
                            break;
                        }
                        case true:  //In case the character shall stop (brake) fast.
                        {
                            switch (m_kneelToCrouch) //In case the character shall slow down from Walking or Running.
                            {
                                case false: //Fast stop while not crouching.
                                {
                                    m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Idle;
                                    m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                    m_setRunTimeMaxSpeed = m_stopMovementValue;
                                    Acceleration(m_brakeRatePerSec);
                                    break;
                                }
                                case true:  //Fast stop while crouching.
                                {
                                    m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                    m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                    m_setRunTimeMaxSpeed = m_stopMovementValue;
                                    Acceleration(m_brakeRatePerSec);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
                #endregion
                #region While Movement Buttons ARE pressed (WASD, Left Stick).
                case true:  //When a Movement button IS pressed.
                {
                    switch (m_shiftIsPressed)   //Runspeed Switch
                    {
                        case false:             //Shift IS NOT pressed.
                        {
                            switch (m_activeBraking)
                            {
                                case false:     //Character shall NOT stop fast, while Shift is pressed.
                                {
                                    switch (m_kneelToCrouch)
                                    {
                                        case false: //Shift is not pressed and character shall walk.
                                        {
                                            //In case the character shall speed up from walking.
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Walking;
                                            m_setRunTimeMaxSpeed = m_walkSpeed;
                                            if (m_individualMaxSpeed < m_setRunTimeMaxSpeed)    //current vs. set speed.
                                            {
                                                m_acceleRatePerSec = m_walkSpeed / m_durationToZeroSpeed;
                                                Acceleration(m_acceleRatePerSec);
                                            }
                                            else
                                            {
                                                m_deceleRatePerSec = -m_walkSpeed / m_durationToZeroSpeed;
                                                Acceleration(m_deceleRatePerSec);
                                            }
                                            break;
                                        }
                                        case true:  //Shift is not pressed and character shall kneel down from Walking or Running.
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                            m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_crouchSpeed;
                                            Acceleration(m_deceleRatePerSec);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case true:      //Character SHALL stop fast, while shift is NOT pressed.
                                {
                                    switch (m_kneelToCrouch)
                                    {
                                        case false:
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Idle;
                                            m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_stopMovementValue;
                                            Acceleration(m_brakeRatePerSec);
                                            break;
                                        }
                                        case true:  //Fast stop while crouching.
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                            m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_stopMovementValue;
                                            Acceleration(m_brakeRatePerSec);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                        case true:  //Shift IS pressed! <---
                        {
                            switch (m_activeBraking)
                            {
                                case false:     //Character shall act as normal, while m_activeBraking is NOT activated.
                                {
                                    switch (m_kneelToCrouch)
                                    {
                                        case false: //If Shift IS pressed and the character shall not kneel down, but run.
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Running;
                                            m_acceleRatePerSec = m_runSpeed / m_durationToMaxSpeed;
                                            m_setRunTimeMaxSpeed = m_runSpeed;
                                            Acceleration(m_acceleRatePerSec);
                                            break;
                                        }
                                        case true:  //If Shift IS pressed and the character shall kneel down.
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                            m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_crouchSpeed;
                                            Acceleration(m_deceleRatePerSec);
                                            break;
                                        }
                                    }
                                    break;
                                }
                                case true:      //Character shall STOP while m_activeBraking is activated, even when Movement Buttons ARE pressed.
                                {
                                    switch (m_kneelToCrouch)
                                    {
                                        case false:
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Idle;
                                            m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_stopMovementValue;
                                            Acceleration(m_brakeRatePerSec);
                                            break;
                                        }
                                        case true:
                                        {
                                            m_playerController.m_eCurrentMoveMode = EOnFootTargetMoveModi.Crouching;
                                            m_brakeRatePerSec = -m_runSpeed / m_durationToZeroSpeed;
                                            m_setRunTimeMaxSpeed = m_stopMovementValue;
                                            Acceleration(m_brakeRatePerSec);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
                #endregion
            }
        }

        private void Acceleration(float _sentDeAccelerationRate)
        {
            switch (m_playerController.m_eCurrentMoveMode)
            {
                case EOnFootTargetMoveModi.Walking:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EOnFootTargetMoveModi.Running:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EOnFootTargetMoveModi.Crouching:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EOnFootTargetMoveModi.Idle:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
            }
        }

        #endregion
        #region Coyote Time
        private void CoyoteTimerReSet()
        {
            switch (m_playerIsGrounded) //Coyote Timer Subtraction/Reset.
            {
                case false:
                {
                    if (m_coyoteTimeCounter > 0)
                    {
                        m_coyoteTimeCounter -= Time.deltaTime;
                    }
                    break;
                }
                case true:
                {
                    m_coyoteTimeCounter = m_coyoteTime;
                    break;
                }
            }
        }
        #endregion
        #region Fall-Damage
        private void FallDamageCalculationStart()
        {
            if (!m_isGroundContactLost)
            {
                m_lostGroundContactVector.y = transform.position.y - (m_groundCheckDistance * 0.5f);    //Radius instead of Diameter.
                m_isGroundContactLost = true;
                m_allowApplyingDamageOnce = true;
            }
        }

        private void FallDamageCalculationEnd()
        {
            if (m_isGroundContactLost && m_allowApplyingDamageOnce)
            {
                m_regainedGroundContactVector.y = transform.position.y - (m_groundCheckDistance * 0.5f);    //Radius instead of Diameter.
                m_isGroundContactLost = false;
                CalculateFallDamage();
            }
        }

        private void CalculateFallDamage()
        {
            if ((m_lostGroundContactVector.y - m_regainedGroundContactVector.y) >= m_minFallDistance && m_fallDamageEnabled)
            {
                m_finalFallDistance = m_lostGroundContactVector.y - m_regainedGroundContactVector.y;

                //"Entrance"-Bool ensures that the calculated damage only gets applied once.
                if (m_allowApplyingDamageOnce)
                {
                    m_allowApplyingDamageOnce = false;
                    ApplyFallDamage(m_finalFallDistance);
                }
            }
        }

        private void ApplyFallDamage(float _finalFallDistance)
        {
            _finalFallDistance *= m_fallDamageMultiplier;
            m_playerController.m_playerHealth.TakeDamage(Mathf.Round(_finalFallDistance));
        }
        #endregion        
        #endregion
        #region CallbackContexts
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
            //if (m_jumpButtonIsPressed && m_playerIsGrounded)    //Original.
            if (m_jumpButtonIsPressed && m_coyoteTimeCounter > 0) //m_playerIsGrounded <-> m_coyoteTimeCounter.
            {
                m_playerController.m_rigidbody.AddForce(Vector3.up * Mathf.Sqrt(m_jumpForce * -m_inversedGravityMultiplier * m_gravityValue), ForceMode.Impulse);

                ////Einmaliges ausloesen bei verlorenem Bodenkontakt ueber den InputManager.
                //InputManager.m_LostGroundContact?.Invoke();
            }
        }

        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            m_coyoteTimeCounter = 0.0f; //Prevents the player from 'double jumping' on pressing the JumpButton multiple times.
        }
        #endregion        
        #endregion
    }
}