using UnityEngine;

namespace PlayerManagement
{
    public class CameraOfflineBehaviour : MonoBehaviour
    {
        [SerializeField] private PlayerOfflineController m_playerOfflineController;

        #region Follow- and LookAtTarget
        [Header("Follow- and LookAtTarget")]
        [SerializeField] internal Camera m_camera;
        [SerializeField] internal Transform m_cameraPivot;
        [SerializeField] internal Transform m_lookAtTarget;
        //Increasing 'm_followTargetSpeed' with 'Relative Movement' results on Player moving out of Screen, while moving towards the Camera.
        [SerializeField] private float m_followTargetSpeed = 0.125f;
        private Transform m_cameraTransform;
        private Vector3 m_followTargetVelocity;

        //[SerializeField] private Transform m_setParentTransform;
        //[SerializeField] private bool m_keepWorldPos = true;
        //[SerializeField] private bool m_differentChildHeight;
        //[SerializeField] private Vector3 m_childPosOffset;
        //private Transform m_currentLookAtTarget;
        #endregion

        #region Camera Movement
        [Header("Camera Movement")]
        [SerializeField, Range(-85.0f, 85.0f)] private float m_minMousePitch;
        [SerializeField, Range(-85.0f, 85.0f)] private float m_maxMousePitch;
        [SerializeField, Range(1.0f, 100f)] private float m_xAxisRotationSpeed = 4.0f;
        [SerializeField, Range(1.0f, 100f)] private float m_yAxisRotationSpeed = 3.0f;
        [SerializeField] private bool m_invertXRotation = false;
        [SerializeField] private bool m_invertYRotation = false;
        [SerializeField] private bool m_disableCameraRotation = false;                  //Disabled CameraRotation
        [SerializeField] private bool m_disableCameraZoom = false;                      //Disabled CameraZoom
        private Vector3 m_cameraMoveDirection;
        internal Vector3 m_playerInputRotationVector;
        internal Vector2 m_mousePosition;
        #endregion

        #region Camera Collision
        [Header("Camera Collision")]
        [SerializeField] private float m_cameraCollisionOffset = 0.2f;
        [SerializeField] private float m_minCollisionOffset = 0.2f;
        [SerializeField] private float m_collisionCheckRadius = 0.2f;
        [SerializeField] private float m_lerpTime = 0.2f;
        [SerializeField] private LayerMask m_collisionCheckLayers;
        private Vector3 m_currentCameraPosition;
        [SerializeField] private float m_defaultCameraPosition;
        #endregion

        #region Camera-Zoom
        [Header("Camera-Zoom")]
        [SerializeField] internal float m_zoomSpeed;
        [SerializeField] private float m_zoomDampening;
        [SerializeField] private float m_minZoomDistance;
        [SerializeField] private float m_maxZoomDistance;
        private float m_clampedCameraDistance, m_runtimeMaxZoomDistance;
        internal float m_zoomScrollValue;
        #endregion

        #region Camera-Position Limitations
        [Header("Cursor Restrictions")]
        [SerializeField] private CursorLockMode m_cursorLockMode;
        [SerializeField] private bool m_cursorVisibility = false;
        #endregion

        private void Awake()
        {
            if (m_camera == null)
                m_camera = GetComponentInChildren<Camera>();

            m_cameraTransform = m_camera.transform;
            m_defaultCameraPosition = m_cameraTransform.localPosition.z;
            m_clampedCameraDistance = m_cameraTransform.localPosition.z;
            m_runtimeMaxZoomDistance = m_maxZoomDistance;

            SetCameraRestrictions();
        }

        private void Update()
        {
            //LookAtCurrentTarget(m_lookAtTarget);
        }

        private void FixedUpdate()
        {
            FollowTarget();
            CameraMovementSettings();
            CameraCollision();
        }

        private void LateUpdate()
        {
            CameraRotation();
            //CameraZoom();
        }

        private void FollowTarget()
        {
            Vector3 targetPosition = Vector3.SmoothDamp(m_cameraPivot.position, m_lookAtTarget.position, ref m_followTargetVelocity, m_followTargetSpeed);
            m_cameraPivot.position = targetPosition;
        }

        private void SetCameraRestrictions()
        {
            Cursor.lockState = m_cursorLockMode;
            Cursor.visible = m_cursorVisibility;
        }

        #region Custom Methods
        //        private void GetMousePosition()
        //        {
        //#if ENABLE_INPUT_SYSTEM
        //            m_mousePosition = m_playerInputActions.PlayerOnFoot.CameraRotation.ReadValue<Vector2>();
        //            //or Mouse.current.position.ReadValue();
        //#else
        //            m_mousePosition = Input.mousePosition;
        //#endif
        //        }

        private void CameraMovementSettings()
        {
            if (!m_disableCameraRotation)
            {
                switch (m_playerInputRotationVector.magnitude)
                {
                    case 0:
                        break;
                    default:
                    {
                        switch (m_invertXRotation)
                        {
                            case false:
                                m_cameraMoveDirection.x -= m_playerInputRotationVector.y * m_xAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                            case true:
                                m_cameraMoveDirection.x += m_playerInputRotationVector.y * m_xAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                        }

                        if (m_playerOfflineController.m_eRigidbodyMoveMethod != ERigidbodyMoveMethod.MouseRotateY)
                        {
                            switch (m_invertYRotation)
                            {
                                case false:
                                {
                                    m_cameraMoveDirection.y -= m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                    break;
                                }
                                case true:
                                {
                                    m_cameraMoveDirection.y += m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }

                m_cameraMoveDirection.x = Mathf.Clamp(m_cameraMoveDirection.x, m_minMousePitch, m_maxMousePitch);   //Rotation around X.
            }
        }

        private void CameraCollision()
        {
            float targetPosition = m_defaultCameraPosition;
            Vector3 cameraDirection = m_cameraTransform.position - m_cameraPivot.position;
            cameraDirection.Normalize();

            if (Physics.SphereCast(m_cameraPivot.transform.position, m_collisionCheckRadius, cameraDirection, out RaycastHit hitObject, Mathf.Abs(targetPosition), m_collisionCheckLayers, QueryTriggerInteraction.UseGlobal))
            {
                float objectHitDistance = Vector3.Distance(m_cameraPivot.position, hitObject.point);
                m_runtimeMaxZoomDistance = objectHitDistance;
                targetPosition = -(objectHitDistance - m_cameraCollisionOffset);    //direction towards the Player on CameraCollision.
                //targetPosition -= objectHitDistance - m_cameraCollisionOffset;      //direction away from the Player on CameraCollision.
            }
            else
            {
                if (m_runtimeMaxZoomDistance != m_maxZoomDistance)
                {
                    m_runtimeMaxZoomDistance = m_maxZoomDistance;
                }
            }

            if (Mathf.Abs(targetPosition) < m_minCollisionOffset)
            {
                targetPosition -= m_minCollisionOffset;
            }

            m_currentCameraPosition.z = Mathf.Lerp(m_cameraTransform.localPosition.z, targetPosition, m_lerpTime);
            m_cameraTransform.localPosition = m_currentCameraPosition;
        }

        private void CameraZoom()
        {
            if (!m_disableCameraZoom)
            {
                if (m_zoomScrollValue != 0.0f)
                {
                    float scrollAmount = m_zoomScrollValue * m_zoomSpeed;
                    scrollAmount *= m_clampedCameraDistance * m_zoomDampening;
                    m_clampedCameraDistance += scrollAmount * -1f;

                    m_clampedCameraDistance = Mathf.Clamp(m_clampedCameraDistance, m_minZoomDistance, m_runtimeMaxZoomDistance);
                }

                if (m_cameraTransform.localPosition.z != m_clampedCameraDistance * -1f)
                {
                    switch (m_zoomScrollValue)
                    {
                        case 0.0f:
                        {
                            //Camera gets stopped here!
                            m_cameraTransform.localPosition = new Vector3(0f, 0f, m_cameraTransform.localPosition.z);
                            break;
                        }
                        default:
                        {
                            //m_clampedCameraDistance Interpolation.
                            m_cameraTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(m_cameraTransform.localPosition.z, m_clampedCameraDistance * -1f, Time.deltaTime * m_zoomSpeed));
                            break;
                        }
                    }
                }
            }
        }

        private void CameraRotation()
        {
            if (!m_disableCameraRotation)
            {
                Quaternion runtimeCameraOrientation = Quaternion.Euler(m_cameraMoveDirection.x, m_cameraMoveDirection.y, 0.0f);
                m_cameraPivot.rotation = Quaternion.Lerp(m_cameraPivot.rotation, runtimeCameraOrientation, Time.deltaTime * (m_xAxisRotationSpeed * m_yAxisRotationSpeed * 0.5f));  //(x * y) * 0.5f prevents rotationHickUps on unsynchronous values.
            }
        }

        //private void LookAtCurrentTarget(Transform _lookAtTarget)
        //{
        //    if (m_currentLookAtTarget != _lookAtTarget || m_currentLookAtTarget == null)
        //    {
        //        m_currentLookAtTarget = _lookAtTarget;
        //        m_cameraPivot.SetParent(_lookAtTarget, m_keepWorldPos);
        //    }

        //    m_cameraTransform.LookAt(m_currentLookAtTarget);
        //}

        //private void SetLookAtParent(Transform _lookAtTarget = null, bool _keepWorldPos = true)
        //{
        //    if (m_currentLookAtTarget != _lookAtTarget || m_currentLookAtTarget == null)
        //    {
        //        m_setParentTransform.SetParent(_lookAtTarget, _keepWorldPos);
        //        if (m_differentChildHeight)
        //            m_setParentTransform.position = new Vector3(_lookAtTarget.position.x + m_childPosOffset.x, _lookAtTarget.position.y + m_childPosOffset.y, _lookAtTarget.position.z + m_childPosOffset.z);
        //        else if (_lookAtTarget != null)
        //            m_setParentTransform.position = _lookAtTarget.position;

        //        m_currentLookAtTarget = _lookAtTarget;
        //    }

        //    m_cameraTransform.LookAt(_lookAtTarget);
        //}
        #endregion
    }
}