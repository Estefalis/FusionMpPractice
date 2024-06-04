using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerManagement
{
    public class PlayerNetworkMovement : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_networkObjectId;
        [SerializeField] private TextMeshProUGUI m_rigidbodyPos;

        private PlayerInputActions m_playerInputActions;
        [SerializeField] private PlayerNetworkController m_playerNetworkController;
        /*[SerializeField] */
        internal Rigidbody m_rigidbody;
        [SerializeField] internal CapsuleCollider m_capsuleCollider;

        #region MoveCharacter-Variables
        #region Movement
        [Header("Movement")]
        [SerializeField] internal float m_walkSpeed = 5.0f;
        [SerializeField] internal float m_runSpeed = 10.0f;
        [SerializeField] internal float m_crouchSpeed = 2.5f;
        internal float m_stopMovementValue = 0.0f;
        [SerializeField] internal float m_jumpForce = 3.0f;
        [SerializeField] internal float m_kneelTime = 0.1f;
        [SerializeField] internal float m_moveSpeedLerpTime = 0.5f;
        internal bool m_canJumpAgain;
        internal bool m_switchMoveMethod = false;
        private Transform m_rigidbodyTransform;
        #endregion

        #region Rotation
        [Header("Rotation")]
        [SerializeField] private float m_smoothRotationTime = 15.0f;
        [SerializeField] private float m_quaternionRotTime = 300.0f;
        [SerializeField, Range(0.5f, 1.0f)] private float m_aDRotYReduction = 0.85f;
        [SerializeField, Range(0.001f, 0.5f)] private float m_mouseRotYReduction = 0.1f;
        /*[SerializeField] */
        float m_mathfSmoothValue;
        private Quaternion m_targetRotation;
        #endregion

        #region Acceleration
        [Header("Acceleration")]
        [SerializeField] internal float m_durationToMaxSpeed = 2.5f;
        [SerializeField] internal float m_durationToZeroSpeed = 6.0f;
        [SerializeField] internal float m_brakeToZeroSpeed = 1.0f;
        internal float m_acceleRatePerSec, m_deceleRatePerSec, m_brakeRatePerSec;
        internal float m_individualMaxSpeed, m_setRunTimeMaxSpeed;
        internal EAvatarMoveState m_lastMoveMode;
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
        [SerializeField] internal bool m_permitCrouchLerp = true;
        internal float m_maxDistanceAbove;
        internal bool m_obstacleIsAbove;
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

        #region RuntimeValues
        internal bool m_playerIsGrounded, m_moveButtonIsPressed, m_shiftIsPressed = false;
        internal bool m_menuIsOpen = false;
        internal Vector3 m_startPosition;
        #endregion

        #region Network
        private Vector3 m_moveDirection;
        private float m_rightVector, m_forwardVector, m_rotationVector;
        private Quaternion m_quatDeltaRot;
        //[Networked] private PlayerNetworkData PlayerNetworkedData { get; set; }
        private static bool m_jumpButtonGotPressed = false, m_kneelButtonGotPressed = false;
        #endregion

        private void Awake()
        {
            m_rigidbody = GetComponentInChildren<Rigidbody>();
            m_rigidbodyTransform = m_rigidbody.transform;
            m_startPosition = transform.position;
            m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Walking;
        }

        private void OnDisable()
        {
            if (m_playerNetworkController.m_playerNetworkInput.gameObject.activeInHierarchy)
            {
                m_playerInputActions = InputManager.m_InputManagerActions;
                m_playerInputActions.PlayerOnFoot.Disable();
                m_playerInputActions.PlayerOnFoot.Jump.canceled -= OnJumpButtonRelease;
                m_playerInputActions.PlayerOnFoot.Duck.performed -= CharacterDuck;
                m_playerInputActions.PlayerOnFoot.Duck.canceled -= StopDucking;
            }
        }

        private void Start()
        {
            m_networkObjectId.text = $"{Object.Id}";

            if (m_playerNetworkController.m_playerNetworkInput.gameObject.activeInHierarchy)
            {
                m_playerInputActions = InputManager.m_InputManagerActions;
                m_playerInputActions.PlayerOnFoot.Enable();
                m_playerInputActions.PlayerOnFoot.Jump.canceled += OnJumpButtonRelease;
                m_playerInputActions.PlayerOnFoot.Duck.performed += CharacterDuck;
                m_playerInputActions.PlayerOnFoot.Duck.canceled += StopDucking;

                m_setRunTimeMaxSpeed = 0.0f;
                m_maxDistanceAbove = m_colliderWalkHeight;
                m_canJumpAgain = true;  //Enable local Jump Variable to enable Jump on first Button press. 
                m_playerNetworkController.m_rePopPosition = m_rigidbodyTransform.position;
            }
        }

        private void Update()
        {
            m_rigidbodyPos.text = $"{m_rigidbodyTransform.position}";

            if (!m_playerNetworkController.m_isDead)
            {
                //simple Groundcheck without Arrays of hitted objects or memory allocation.
                m_playerIsGrounded = Physics.CheckSphere(m_groundCheckTransform.position, m_groundCheckDistance, m_groundCheckLayerMask);
                //m_playerNetworkController.m_playerIsGrounded = Physics.Raycast(m_playerNetworkController.m_groundCheckTransform.position, Vector3.down, m_playerNetworkController.m_groundCheckDistance, m_playerNetworkController.m_groundCheckLayerMask);

                CoyoteTimerReSet();
                MoveAcceleration();
            }

            if (m_rigidbodyTransform.position.y < m_playerNetworkController.m_fallLimit)
            {
                m_rigidbodyTransform.position = m_playerNetworkController.m_rePopPosition; //AreaFallOffReset
            }
        }

        public override void FixedUpdateNetwork()
        {
            //if (GetInput<CombinedPlayerInputs>(out var networkInput))
            //{
            //    m_rightVector = networkInput[m_playerNetworkController.m_myPlayerId].MoveDirection.x;
            //    m_rotationVector = networkInput[m_playerNetworkController.m_myPlayerId].MoveDirection.y;
            //    m_forwardVector = networkInput[m_playerNetworkController.m_myPlayerId].MoveDirection.z;
            //    m_jumpButtonGotPressed = networkInput[m_playerNetworkController.m_myPlayerId].JumpButtonGotPressed;
            //    m_kneelButtonGotPressed = networkInput[m_playerNetworkController.m_myPlayerId].DuckButtonGotPressed;
            //}

            if (GetInput<PlayerNetworkData>(out var networkInput))
            {
                m_rightVector = networkInput.MoveDirection.x;
                m_rotationVector = networkInput.MoveDirection.y;
                m_forwardVector = networkInput.MoveDirection.z;
                m_jumpButtonGotPressed = networkInput.JumpButtonGotPressed;
                m_kneelButtonGotPressed = networkInput.DuckButtonGotPressed;

                Crouching();    //Move from Update because of the need of 'm_kneelButtonGotPressed' being static.

                if (!m_playerNetworkController.m_isDead)
                {
                    switch (m_playerNetworkController.m_eRigidbodyMoveMethod)
                    {
                        //Runner.DeltaTime instead of Time.fixedDeltaTime in Movement_Methods.
                        case ERigidbodyMoveMethod.Basic:
                        {
                            MoveRigidbodyBasic();
                            break;
                        }
                        case ERigidbodyMoveMethod.KbRotateY:
                        {
                            MoveRigidbodyKbY();
                            break;
                        }
                        case ERigidbodyMoveMethod.MouseRotateY:
                        {
                            MoveRigidBodyMouseY();
                            break;
                        }
                        case ERigidbodyMoveMethod.Locked:
                        {
                            MoveRigidbodyLocked();
                            break;
                        }
                        case ERigidbodyMoveMethod.Relative:
                        {
                            MoveRigidbodyRelative();
                            break;
                        }
                    }

                    Jumping();

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

                            if (!m_jumpButtonGotPressed && !m_canJumpAgain)
                            {
                                m_canJumpAgain = true;
                            }
                            break;
                        }
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
        private void Jumping()
        {
            if (m_coyoteTimeCounter >= 0 && m_jumpButtonGotPressed && m_canJumpAgain && m_playerIsGrounded)
            {
                m_canJumpAgain = false; //local Jump Variable to just apply Addforce ONCE.
                m_rigidbody.AddForce(m_rigidbody.transform.up * Mathf.Sqrt(m_jumpForce * -m_inversedGravityMultiplier * m_gravityValue), ForceMode.Impulse);
            }
        }

        #region MoveRigidbody Alternatives
        private void MoveRigidbodyBasic()
        {
            m_moveDirection = new Vector3(m_rightVector, 0.0f, m_forwardVector);
            m_rigidbody.MovePosition(m_rigidbodyTransform.position + m_individualMaxSpeed * Runner.DeltaTime * m_moveDirection.normalized);

            if (m_moveDirection != Vector3.zero)
            {
                m_targetRotation = Quaternion.LookRotation(m_moveDirection, Vector3.up);
                m_targetRotation = Quaternion.RotateTowards(m_rigidbodyTransform.rotation, m_targetRotation, m_quaternionRotTime * Runner.DeltaTime);
                m_rigidbody.MoveRotation(m_targetRotation);
            }
        }

        private void MoveRigidbodyKbY()
        {
            m_moveDirection = new Vector3(0.0f, 0.0f, m_forwardVector);
            m_moveDirection = m_rigidbodyTransform.TransformDirection(m_moveDirection);
            m_rigidbody.MovePosition(m_rigidbodyTransform.position + m_individualMaxSpeed * Runner.DeltaTime * m_moveDirection.normalized);

            m_quatDeltaRot =
                Quaternion.Euler(0.0f, m_rotationVector * Runner.DeltaTime * (m_quaternionRotTime * m_aDRotYReduction), 0.0f);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_quatDeltaRot);
        }

        private void MoveRigidBodyMouseY()
        {
            m_moveDirection = new Vector3(m_rightVector, 0.0f, m_forwardVector);
            m_moveDirection = m_rigidbodyTransform.TransformDirection(m_moveDirection);
            m_rigidbody.MovePosition(m_rigidbodyTransform.position + m_individualMaxSpeed * Runner.DeltaTime * m_moveDirection.normalized);

            m_quatDeltaRot =
                Quaternion.Euler(0.0f, m_rotationVector * Runner.DeltaTime * (m_quaternionRotTime * m_mouseRotYReduction), 0.0f);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * m_quatDeltaRot);
        }

        private void MoveRigidbodyLocked()
        {
            m_moveDirection = new Vector3(m_rightVector, 0.0f, m_forwardVector);
            m_moveDirection = m_rigidbodyTransform.TransformDirection(m_moveDirection);
            //TODO: Lerping CameraY-Rotation to RigidbodyY-Rotation while being locked?
            m_rigidbody.MovePosition(m_rigidbodyTransform.position + m_individualMaxSpeed * Runner.DeltaTime * m_moveDirection.normalized);
        }

        private void MoveRigidbodyRelative()
        {
            m_moveDirection = m_rightVector * m_playerNetworkController.m_cameraNetworkBehaviour.m_camera.transform.right;
            m_moveDirection += m_forwardVector * m_playerNetworkController.m_cameraNetworkBehaviour.m_camera.transform.forward;
            m_moveDirection.y = 0.0f;
            m_rigidbody.MovePosition(m_rigidbodyTransform.position + m_individualMaxSpeed * Runner.DeltaTime * m_moveDirection.normalized);

            if (m_moveDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(m_moveDirection.x, m_moveDirection.z) * Mathf.Rad2Deg;
                float smoothRotation =
                    Mathf.SmoothDampAngle(m_rigidbodyTransform.eulerAngles.y, angle, ref m_mathfSmoothValue, 1 / m_smoothRotationTime);
                m_rigidbodyTransform.rotation = Quaternion.Euler(0.0f, smoothRotation, 0.0f);
            }
        }
        #endregion
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
                m_crouchTimer += Runner.DeltaTime;
                float countingUp = m_crouchTimer / m_kneelTime;
                m_crouchTimer *= m_crouchTimer;

                SphereCastCheckAbove(); //Locks Player in 'crouch-mode', if obstacles are detected above.

                switch (m_kneelButtonGotPressed) //fomor: m_kneelToCrouch.
                {
                    case false:
                    {
                        if (m_currentHitObject == null)
                        {
                            if (m_crouchTimer < m_kneelTime)
                            {
                                //Lerp getting up.
                                m_capsuleCollider.height = Mathf.Lerp(m_capsuleCollider.height, m_colliderWalkHeight, countingUp);
                                m_crouchTimer += Runner.DeltaTime;
                            }
                            else
                            {
                                m_capsuleCollider.height = m_colliderWalkHeight;
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
                            m_capsuleCollider.height = Mathf.Lerp(m_capsuleCollider.height, m_colliderCrouchHeight, countingUp);
                            m_crouchTimer += Runner.DeltaTime;
                        }
                        else
                            m_capsuleCollider.height = m_colliderCrouchHeight;
                        break;
                    }
                }
            }
        }
        #endregion
        #region Acceleration
        private void MoveAcceleration()
        {
            switch (m_moveButtonIsPressed)
            {
                #region While Movement Buttons are not pressed (WASD, Left Stick).
                case false: //When no Movement button is pressed.
                {
                    switch (m_kneelButtonGotPressed) //In case the character shall slow down from Walking or Running.
                    {
                        case false:
                        {
                            m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Idle;
                            m_setRunTimeMaxSpeed = m_stopMovementValue;
                            m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                            AccelerationRate(m_deceleRatePerSec);
                            break;
                        }
                        case true:
                        {
                            m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Crouching;
                            m_setRunTimeMaxSpeed = m_crouchSpeed;
                            m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                            AccelerationRate(m_deceleRatePerSec);
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
                            switch (m_kneelButtonGotPressed)
                            {
                                case false: //Shift is not pressed and character shall walk.
                                {
                                    //In case the character shall speed up from walking.
                                    m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Walking;
                                    m_setRunTimeMaxSpeed = m_walkSpeed;
                                    if (m_individualMaxSpeed < m_setRunTimeMaxSpeed)    //current vs. set speed.
                                    {
                                        m_acceleRatePerSec = m_walkSpeed / m_durationToZeroSpeed;
                                        AccelerationRate(m_acceleRatePerSec);
                                    }
                                    else
                                    {
                                        m_deceleRatePerSec = -m_walkSpeed / m_durationToZeroSpeed;
                                        AccelerationRate(m_deceleRatePerSec);
                                    }
                                    break;
                                }
                                case true:  //Shift is not pressed and character shall kneel down from Walking or Running.
                                {
                                    m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Crouching;
                                    m_setRunTimeMaxSpeed = m_crouchSpeed;
                                    m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                    AccelerationRate(m_deceleRatePerSec);
                                    break;
                                }
                            }
                            break;
                        }
                        case true:  //Shift IS pressed! <---
                        {
                            switch (m_kneelButtonGotPressed)
                            {
                                case false: //If Shift IS pressed and the character shall not kneel down, but run.
                                {
                                    m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Running;
                                    m_setRunTimeMaxSpeed = m_runSpeed;
                                    m_acceleRatePerSec = m_runSpeed / m_durationToMaxSpeed;
                                    AccelerationRate(m_acceleRatePerSec);
                                    break;
                                }
                                case true:  //If Shift IS pressed and the character shall kneel down.
                                {
                                    m_playerNetworkController.m_eAvatarMoveState = EAvatarMoveState.Crouching;
                                    m_setRunTimeMaxSpeed = m_crouchSpeed;
                                    m_deceleRatePerSec = -m_crouchSpeed / m_durationToZeroSpeed;
                                    AccelerationRate(m_deceleRatePerSec);
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

        private void AccelerationRate(float _sentDeAccelerationRate)
        {
            switch (m_playerNetworkController.m_eAvatarMoveState)
            {
                case EAvatarMoveState.Walking:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EAvatarMoveState.Running:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EAvatarMoveState.Crouching:
                {
                    m_individualMaxSpeed += _sentDeAccelerationRate * Time.deltaTime;
                    m_individualMaxSpeed = Mathf.Clamp(m_setRunTimeMaxSpeed, m_stopMovementValue, m_setRunTimeMaxSpeed);
                    break;
                }
                case EAvatarMoveState.Idle:
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
            m_playerNetworkController.m_playerNetworkHealth.TakeDamage(Mathf.Round(_finalFallDistance));
        }
        #endregion
        #endregion
        #region CallbackContexts
        #region Character Jump
        private void OnJumpButtonRelease(InputAction.CallbackContext _callbackContext)
        {
            m_coyoteTimeCounter = 0.0f; //Prevents the player from 'double jumping' on pressing the JumpButton multiple times.
        }
        #endregion
        #region Ducking
        private void CharacterDuck(InputAction.CallbackContext _callbackContext)
        {
            m_crouchTimer = 0;

            m_groundCheckHeightAdjustment = (m_colliderWalkHeight - m_colliderCrouchHeight) / 2;
            m_groundCheckTransform.position = new Vector3(m_rigidbody.position.x, m_rigidbody.position.y + m_groundCheckHeightAdjustment, m_rigidbody.position.z);
        }

        private void StopDucking(InputAction.CallbackContext _callbackContext)
        {
            //Whenever the m_groundCheckTransform.position gets ReSetted, it has to be the same position as the moving Rigidbody!
            m_groundCheckTransform.position = m_rigidbody.position;
        }
        #endregion
        #endregion

        ///// <summary>
        ///// Received PlayerInput-Data via 'PlayerNetworkedData' and Photon.
        ///// </summary>
        ///// <param name="data"></param>
        //internal void SetInputData(PlayerNetworkData data)
        //{
        //    PlayerNetworkedData = data;
        //}
    }
}