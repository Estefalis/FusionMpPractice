using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInputManagement
{
    public class CameraNetworkBehaviour : MonoBehaviour
    {
        [SerializeField] internal PlayerPersPective m_playerPerspective;

        //TODO: Script an FirstPerson enum anpassen!
        [SerializeField] private PlayerNetworkController m_playerNetworkController;

        #region SetParent and LookAtTarget
        [Header("SetParent and LookAtTarget")]
        [SerializeField] internal Camera m_camera;
        [SerializeField] private Transform m_setParentTransform;
        [SerializeField] internal Transform m_rotateParentTransform;
        //[SerializeField] internal Transform m_relativeHelperTransform;    //HelperTransform before knowing the need of cameraForward/cameraRight.normalize. 
        [SerializeField] internal Transform m_lookAtTarget;
        [SerializeField] private bool m_keepWorldPos = true;
        [SerializeField] private bool m_differentChildHeight;
        [SerializeField] private Vector3 m_childPosOffset;
        private Transform m_currentLookAtTarget;
        #endregion

        #region Camera-Position Limitations
        [Header("Camera-Position Limitations")]
        [SerializeField] private CursorLockMode m_cursorLockMode;
        [SerializeField] private float m_sphereCheckRadius = 0.5f;
        [SerializeField] private LayerMask m_sphereCheckLayerMask;
        internal bool m_obstacleIsBelow;
        #region Calculations
        /*[SerializeField] */
        private float m_flexibleMinMouseAngle;
        /*[SerializeField] */
        private float m_lastHitObjectYPos;
        #endregion
        #endregion

        #region Camera-Rotation
        [Header("Camera-Rotation")]
        [SerializeField, Range(-85.0f, 85.0f)] private float m_minMousePitch;
        [SerializeField, Range(-85.0f, 85.0f)] private float m_maxMousePitch;
        [SerializeField, Range(1.0f, 100f)] private float m_xAxisRotationSpeed = 4.0f;
        [SerializeField, Range(1.0f, 100f)] private float m_yAxisRotationSpeed = 3.0f;
        [SerializeField] private bool m_invertXRotation = false;
        [SerializeField] private bool m_invertYRotation = false;
        [SerializeField] private bool m_disableCameraRotation = false;                  //Disabled CameraRotation
        [SerializeField] private bool m_disableCameraZoom = false;                      //Disabled CameraZoom
        private Vector3 runtimeRotationVector;
        private float m_runtimeMinMousePitch;
        internal bool m_runtimePitchSwitch;
        internal Vector3 m_playerInputRotationVector;
        internal Vector2 m_mousePosition;
        #endregion

        #region Camera-Zoom
        //Possible Array of ZoomDistances for different distances, instead of back to front?
        [Header("Camera-Zoom")]
        [SerializeField] internal float m_zoomSpeed;
        [SerializeField] private float m_zoomDampening;
        [SerializeField] private float m_startZoomDistance = 8.0f;
        [SerializeField] private float m_minZoomDistance;
        [SerializeField] private float m_maxZoomDistance;
        internal float m_clampedCameraDistance;                                         //Clamped in private void CameraZoom().
        internal float m_zoomScrollValue;
        #endregion

        #region Debug.Drawline & DrawWireSphere
        [Header("Debug Drawings")]
        [SerializeField] private float m_sphereCastLength;
        internal Vector3 m_lineOrigin;
        internal Vector3 m_sphereCastDirection;
        internal float m_hitCheckDistance;
        #endregion

        private void Awake()
        {
            if (m_camera == null)
                m_camera = GetComponentInChildren<Camera>();

            ResetCameraByPerspective();
        }

        private void Update()
        {
            //GetMousePosition();

            switch (m_playerPerspective)
            {
                case PlayerPersPective.ThirdPerson:
                {
                    m_rotateParentTransform.position = m_setParentTransform.position;
                    //RelativeHelperPositioning();
                    SetLookAtParent(m_lookAtTarget, true);
                    MinCameraPosSphereCast();
                    break;
                }
                case PlayerPersPective.FirstPerson:
                {
                    break;
                }
                default:
                    break;
            }
        }

        private void FixedUpdate()
        {
            UpdateRotation();
        }

        private void LateUpdate()
        {
            switch (m_playerPerspective)
            {
                case PlayerPersPective.ThirdPerson:
                {
                    ThirdPersonCameraLerp();
                    break;
                }
                case PlayerPersPective.FirstPerson:
                {
                    //FirstPersonCameraLerp();
                    break;
                }
                default:
                    break;
            }

            CameraZoom();
        }

        private void ResetCameraByPerspective()
        {
            m_runtimeMinMousePitch = m_minMousePitch;
            Cursor.lockState = m_cursorLockMode;
            m_rotateParentTransform.rotation = m_setParentTransform.rotation;

            switch (m_playerPerspective)
            {
                case PlayerPersPective.ThirdPerson:
                {
                    //m_mousePosition = Vector2.zero;
                    m_clampedCameraDistance = m_startZoomDistance;          //m_clampedCameraDistance -= variable to adjust the cameraPos by the player?
                    SetLookAtParent(m_lookAtTarget, m_keepWorldPos);
                    break;
                }
                case PlayerPersPective.FirstPerson:
                {
                    m_camera.transform.SetParent(m_setParentTransform, m_keepWorldPos);
                    //min & max - ZoomVariables get switched within the CameraZoom - Method itself.
                    break;
                }
                default:
                    break;
            }
        }

        //private void RelativeHelperPositioning()
        //{
        //    m_relativeHelperTransform.position = new Vector3(m_camera.transform.position.x, m_playerNetworkController/*.m_rigidbody*/.transform.position.y, m_camera.transform.position.z);
        //    m_relativeHelperTransform.LookAt(m_playerNetworkController.transform.position);
        //}

        #region Custom Methods
        private void GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
        m_mousePosition = m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>();
        //or Mouse.current.position.ReadValue();
#else
            m_mousePosition = Input.mousePosition;
#endif
        }

        private void UpdateRotation()
        {
            if (!m_disableCameraRotation)
            {
                switch (m_playerInputRotationVector.magnitude)
                {
                    //case 0:
                    //    break;
                    default:
                    {
                        switch (m_invertXRotation)
                        {
                            case false:
                                runtimeRotationVector.x -= m_playerInputRotationVector.y * m_xAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                            case true:
                                runtimeRotationVector.x += m_playerInputRotationVector.y * m_xAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                        }

                        switch (m_invertYRotation)
                        {
                            case false:
                            {
                                runtimeRotationVector.y -= m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                            }
                            case true:
                            {
                                runtimeRotationVector.y += m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                            }
                        }
                    }
                    break;
                }

                switch (m_runtimePitchSwitch)
                {
                    case false:
                    {
                        if (!m_playerNetworkController.m_playerNetworkMovement.m_obstacleIsAbove)
                            m_flexibleMinMouseAngle = m_runtimeMinMousePitch;
                        break;
                    }
                    case true:
                    {
                        switch (m_playerNetworkController.m_eCurrentMoveMode)
                        {
                            case EOnFootTargetMoveModi.Crouching:
                            {
                                //Magic Number == 4!
                                m_flexibleMinMouseAngle = m_lastHitObjectYPos + (4 * m_playerNetworkController.m_playerNetworkMovement.m_colliderWalkHeight - m_playerNetworkController.m_playerNetworkMovement.m_colliderCrouchHeight);
                                break;
                            }
                            default:
                                m_flexibleMinMouseAngle = m_lastHitObjectYPos + (4 * m_playerNetworkController.m_playerNetworkMovement.m_colliderWalkHeight - m_playerNetworkController.m_playerNetworkMovement.m_colliderCrouchHeight);
                                break;
                        }
                        break;
                    }
                }

                runtimeRotationVector.x = Mathf.Clamp(runtimeRotationVector.x, m_flexibleMinMouseAngle, m_maxMousePitch);   //Rotation around X.
            }
        }

        private void CameraZoom()
        {
            if (!m_disableCameraZoom)
            {
                if (m_zoomScrollValue != 0.0f)
                //if (m_playerNetworkController.m_playerInputActions.PlayerOnFootRH.CameraZoom.ReadValue<Vector2>().y != 0.0f)
                {
                    float scrollAmount = m_zoomScrollValue * m_zoomSpeed;
                    scrollAmount *= m_clampedCameraDistance * m_zoomDampening;
                    m_clampedCameraDistance += scrollAmount * -1f;
                    //Debug.Log($"ScrollAmount{scrollAmount} - ClampCamDis {m_clampedCameraDistance} - ZoomDamp {m_zoomDampening}");

                    switch (m_playerPerspective)
                    {
                        //Switches CameraZoom-Direction, if not disabled in FirstPerson.
                        case PlayerPersPective.ThirdPerson:
                        {
                            m_clampedCameraDistance = Mathf.Clamp(m_clampedCameraDistance, m_minZoomDistance, m_maxZoomDistance);
                            break;
                        }
                        //If a different Clamping method is needed.
                        case PlayerPersPective.FirstPerson:
                        {
                            m_clampedCameraDistance = Mathf.Clamp(m_clampedCameraDistance, m_minZoomDistance, m_maxZoomDistance);
                            break;
                        }
                    }
                }

                if (m_camera.transform.localPosition.z != m_clampedCameraDistance * -1f)
                {
                    switch (m_zoomScrollValue)
                    {
                        case 0.0f:
                        {
                            //Camera gets stopped here!
                            m_camera.transform.localPosition = new Vector3(0f, 0f, m_camera.transform.localPosition.z);
                            break;
                        }
                        default:
                        {
                            //m_clampedCameraDistance Interpolation.
                            m_camera.transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(m_camera.transform.localPosition.z, m_clampedCameraDistance * -1f, Time.deltaTime * m_zoomSpeed));
                            break;
                        }
                    }
                }
            }
        }

        private void MinCameraPosSphereCast()
        {
            m_lineOrigin = m_camera.transform.position;
            m_sphereCastDirection = -m_rotateParentTransform.up;

            m_obstacleIsBelow =
            Physics.SphereCast(m_lineOrigin, m_sphereCheckRadius, m_sphereCastDirection, out RaycastHit hitObject, m_sphereCastLength, m_sphereCheckLayerMask, QueryTriggerInteraction.UseGlobal);

            switch (m_obstacleIsBelow)
            {
                case false:
                {
                    m_hitCheckDistance = m_sphereCheckRadius;

                    if (m_minMousePitch != m_runtimeMinMousePitch)
                        m_minMousePitch = m_runtimeMinMousePitch;

                    SetNewMinMousePitch(false);
                    break;
                }
                case true:
                {
                    m_hitCheckDistance = hitObject.distance;
                    m_lastHitObjectYPos = hitObject.transform.position.y;

                    if (m_minMousePitch != m_lastHitObjectYPos)
                        m_minMousePitch = m_lastHitObjectYPos;

                    SetNewMinMousePitch(true);
                    break;
                }
            }
        }

        private void SetNewMinMousePitch(bool _switch)
        {
            switch (_switch)
            {
                case false:
                    m_runtimePitchSwitch = false;
                    break;
                case true:
                    m_runtimePitchSwitch = true;
                    break;
            }
        }

        private void SetLookAtParent(Transform _lookAtTarget = null, bool _keepWorldPosition = true)
        {
            if (m_currentLookAtTarget != _lookAtTarget || m_currentLookAtTarget == null)
            {
                m_setParentTransform.SetParent(_lookAtTarget, _keepWorldPosition);
                if (m_differentChildHeight)
                    m_setParentTransform.position = new Vector3(_lookAtTarget.position.x + m_childPosOffset.x, _lookAtTarget.position.y + m_childPosOffset.y, _lookAtTarget.position.z + m_childPosOffset.z);
                else if (_lookAtTarget != null)
                    m_setParentTransform.position = _lookAtTarget.position;

                m_currentLookAtTarget = _lookAtTarget;
            }

            m_camera.transform.LookAt(_lookAtTarget);
        }

        //private void FirstPersonCameraLerp()
        //{
        //    if (!m_disableCameraRotation)
        //    {
        //        //TODO: HeadRotationY with Rigidbody, HeadRotationX with runtimeRotationVector.x from UpdateRotation(). Look at Targeting from YBot?
        //    }
        //}

        private void ThirdPersonCameraLerp()
        {
            if (!m_disableCameraRotation)
            {
                Quaternion runtimeCameraOrientation = Quaternion.Euler(runtimeRotationVector.x, runtimeRotationVector.y, 0.0f);
                m_rotateParentTransform.rotation = Quaternion.Lerp(m_rotateParentTransform.rotation, runtimeCameraOrientation, Time.deltaTime * (m_xAxisRotationSpeed * m_yAxisRotationSpeed * 0.5f));  //(x * y) * 0.5f prevents rotationHickUps on unsynchronous values.
            }
        }
        #endregion
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Debug.DrawLine(m_lineOrigin, m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance);
        Gizmos.DrawWireSphere(m_lineOrigin + m_sphereCastDirection * m_hitCheckDistance, m_sphereCheckRadius);
    }
#endif
    }
}