using UnityEngine;
using UnityEngine.InputSystem;

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

            m_permitCrouchLerp = false;
        }

        private void Start()
        {
            m_playerController.m_playerInputActions = InputManager.m_InputManagerActions;
            m_playerController.m_playerInputActions.PlayerOnFootRH.Enable();
            m_playerController.m_playerInputActions.PlayerOnFootRH.Jump.performed += CharacterJump;

            m_setRunTimeMaxSpeed = 0.0f;
            m_maxDistanceAbove = m_colliderWalkHeight;
        }

        private void Update()
        {
            if (!m_playerController.m_isDead)
            {
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
                SetMoveAcceleration();
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
                    m_characterRotation = new Vector3(0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0);
                    break;
                }
                case true:
                {
                    m_horizontalMovement = new(m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().x, 0, m_playerController.m_playerInputActions.PlayerOnFootRH.Movement.ReadValue<Vector2>().y);
                    m_characterRotation.y = 0.0f;
                    //TODO: Lerping CameraY-Rotation to RigidbodyY-Rotation.
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
#if UNITY_EDITOR
                            Debug.Log($"Active braking: {m_activeBraking} & Kneel: {m_kneelToCrouch}!");
#endif
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
                                    Debug.Log($"Active braking: {m_activeBraking} & Kneel: {m_kneelToCrouch}!");
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
#if UNITY_EDITOR
            Debug.Log($"MoveSpeed Switch: {m_individualMaxSpeed} on SetMaxSpeed: {m_setRunTimeMaxSpeed}.");
#endif
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
        #region Character Jump
        private void CharacterJump(InputAction.CallbackContext _callbackContext)
        {
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
        #endregion        
        #endregion
    }
}