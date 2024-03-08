using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInputManagement
{
    public class CameraBehaviour : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;

        #region SetParent and LookAtTarget
        [Header("SetParent and LookAtTarget")]
        [SerializeField] internal Camera m_camera;
        [SerializeField] private Transform m_parentTransform;
        [SerializeField] internal Transform m_lookAtTarget;
        private Transform m_currentLookAtTarget;
        #endregion

        #region Camera-Position Limitations
        [Header("Camera-Position Limitations")]
        [SerializeField] private CursorLockMode m_cursorLockMode;
        [SerializeField] private float m_sphereCheckRadius = 0.5f;
        [SerializeField] private LayerMask m_sphereCheckLayerMask;
        [SerializeField] private bool m_keepWorldPos = true;
        internal bool m_obstacleIsBelow;
        [SerializeField] private float m_lastHitObjectYPos;
        #endregion

        #region Camera-Rotation
        [Header("Camera-Rotation")]
        //[SerializeField] private MouseAxisAdjustments m_mouseAxisAdjustments;
        //private enum MouseAxisAdjustments { MouseXAndY = default, MouseX = 1, MouseY = 2, DefaultReset = 3 };
        //[SerializeField, Min(0.05f)] private float m_adjustSensitivityStep = 0.1f;
        //private float m_xMouseSensitivityDefault = 3.0f, m_yMouseSensitivityDefault = 4.0f;
        [SerializeField, Range(-85.0f, 85.0f)] private float m_minMousePitch;
        [SerializeField, Range(-85.0f, 85.0f)] private float m_maxMousePitch;
        [SerializeField, Range(0.0001f, 20f)] private float m_xAxisRotationSpeed = 4.0f;
        [SerializeField, Range(0.0001f, 20f)] private float m_yAxisRotationSpeed = 3.0f;
        [SerializeField] private bool m_invertXRotation = false;
        [SerializeField] private bool m_invertYRotation = false;
        [SerializeField] private bool m_disableCameraRotation = false;                  //Disabled CameraRotation
        [SerializeField] private bool m_disableCameraZoom = false;                      //Disabled CameraZoom
        private Vector3 runtimeRotationVector;
        private float m_flexibleMinMousePitch;
        private bool m_runtimePitchSwitch;  //<(~.^)"
        internal Vector3 m_playerInputRotationVector;
        internal Vector2 m_mousePosition;
        #endregion

        #region Camera-Zoom
        [Header("Camera-Zoom")]
        [SerializeField] internal float m_zoomSpeed;
        [SerializeField] private float m_zoomDampening;
        [SerializeField] private float m_startZoomDistance = 10.0f;
        [SerializeField] private float m_minZoomDistance;
        [SerializeField] private float m_maxZoomDistance;
        internal float m_clampedCameraDistance;                                              //Clamped in CameraZoom.
        internal float m_runtimeZoomHeight;
        #endregion                                             //Zoom/Scroll-Variable.

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

            m_clampedCameraDistance = m_startZoomDistance;
            m_flexibleMinMousePitch = m_minMousePitch;
            m_mousePosition = Vector2.zero;
            Cursor.lockState = m_cursorLockMode;
            SetCurrentLookAtTarget(m_lookAtTarget, m_keepWorldPos);
        }

        private void Update()
        {
            GetMousePosition();
            SetCurrentLookAtTarget(m_lookAtTarget, true);
            MinCameraPosSphereCast();   //Shall set a new MinCamPos on hitting an Object.
        }

        private void FixedUpdate()
        {
            UpdateRotation();
        }

        private void LateUpdate()
        {
            LerpCameraPosition();
            CameraZoom();
        }

        #region Custom Methods
        private void GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            m_mousePosition = m_playerController.m_playerInputActions.PlayerOnFootRH.Rotation.ReadValue<Vector2>();
            //or Mouse.current.position.ReadValue();
#else
            m_mousePosition = Input.mousePosition;
#endif
        }

        private void UpdateRotation()
        {
            if (!m_disableCameraRotation)
            {
                if (m_camera.transform.position.y <= 0 && m_playerInputRotationVector.y < 0.5f)
                    m_playerInputRotationVector.y += m_camera.transform.position.y + 50f * Time.fixedDeltaTime;

                switch (m_playerInputRotationVector.magnitude)
                {
                    case 0:
                        break;
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
                                runtimeRotationVector.y -= m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                            case true:
                                runtimeRotationVector.y += m_playerInputRotationVector.x * m_yAxisRotationSpeed * Time.fixedDeltaTime;
                                break;
                        }
                    }
                    break;
                }

                runtimeRotationVector.x = Mathf.Clamp(runtimeRotationVector.x, m_flexibleMinMousePitch, m_maxMousePitch);   //Rotation around X.                
            }
        }

        private void CameraZoom()
        {
            if (!m_disableCameraZoom)
            {
                if (m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.ReadValue<Vector2>().y != 0.0f)
                {
                    float scrollAmount = m_playerController.m_playerInputActions.PlayerOnFootRH.CameraZoom.ReadValue<Vector2>().y * m_zoomSpeed;
                    scrollAmount *= m_clampedCameraDistance * m_zoomDampening;
                    m_clampedCameraDistance += scrollAmount * -1f;
                    m_clampedCameraDistance = Mathf.Clamp(m_clampedCameraDistance, m_minZoomDistance, m_maxZoomDistance);
                }

                if (m_camera.transform.localPosition.z != m_clampedCameraDistance * -1f)
                {
                    //m_clampedCameraDistance Interpolation.
                    m_camera.transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(m_camera.transform.localPosition.z, m_clampedCameraDistance * -1f,
                        Time.deltaTime * m_zoomSpeed));
                }
            }
        }

        private void MinCameraPosSphereCast()
        {
            m_lineOrigin = m_camera.transform.position;
            m_sphereCastDirection = -m_parentTransform.up;

            m_obstacleIsBelow =
            Physics.SphereCast(m_lineOrigin, m_sphereCheckRadius, m_sphereCastDirection, out RaycastHit hitObject, m_sphereCastLength, m_sphereCheckLayerMask, QueryTriggerInteraction.UseGlobal);

            switch (m_obstacleIsBelow)
            {
                case false:
                {
                    m_hitCheckDistance = m_sphereCheckRadius;
                    m_flexibleMinMousePitch = m_minMousePitch;
                    SetNewMinMousePitch(m_flexibleMinMousePitch);
                    break;
                }
                case true:
                {
                    m_hitCheckDistance = hitObject.distance;
                    m_lastHitObjectYPos = hitObject.transform.position.y;
                    m_flexibleMinMousePitch = m_parentTransform.localRotation.x;
                    SetNewMinMousePitch(m_flexibleMinMousePitch);
                    break;
                }
            }
        }

        private void SetNewMinMousePitch(float _newMinMousePitch)
        {
            m_runtimePitchSwitch = _newMinMousePitch == m_minMousePitch;
            switch (m_runtimePitchSwitch)
            {
                case false: //m_parentTransform.(local)Rotation.x;
                {
                    if (m_flexibleMinMousePitch != _newMinMousePitch)
                    {
                        m_flexibleMinMousePitch = _newMinMousePitch;
                        Debug.Log(m_flexibleMinMousePitch);
                    }
                    break;
                }
                case true:  //m_minMousePitch;
                {
                    if (m_minMousePitch != _newMinMousePitch)
                    {
                        m_flexibleMinMousePitch = _newMinMousePitch;
                        Debug.Log(m_flexibleMinMousePitch);
                    }
                    break;
                }
            }
        }

        private void SetCurrentLookAtTarget(Transform _lookAtTarget, bool _keepWorldPosition)
        {
            if (m_currentLookAtTarget != _lookAtTarget || m_currentLookAtTarget == null)
            {
                m_parentTransform.SetParent(_lookAtTarget, _keepWorldPosition);
                m_parentTransform.position = _lookAtTarget.position;
                m_currentLookAtTarget = _lookAtTarget;
            }

            m_camera.transform.LookAt(_lookAtTarget);
        }
        #endregion

        private void LerpCameraPosition()
        {
            if (!m_disableCameraRotation)
            {
                Quaternion runtimeCameraOrientation = Quaternion.Euler(runtimeRotationVector.x, runtimeRotationVector.y, 0.0f);
                m_parentTransform.rotation = Quaternion.Lerp(m_parentTransform.rotation, runtimeCameraOrientation, Time.deltaTime * (m_xAxisRotationSpeed * m_yAxisRotationSpeed * 0.5f));
            }
        }

        #region Axis Sensitivity
        //internal void IncreaseAxisSensitivity()
        //{
        //    switch (m_mouseAxisAdjustments)
        //    {
        //        case MouseAxisAdjustments.MouseXAndY:
        //        {
        //            m_xAxisRotationSpeed += m_adjustSensitivityStep;
        //            m_yAxisRotationSpeed += m_adjustSensitivityStep;
        //            break;
        //        }
        //        case MouseAxisAdjustments.MouseX:
        //            m_xAxisRotationSpeed += m_adjustSensitivityStep;
        //            break;
        //        case MouseAxisAdjustments.MouseY:
        //            m_yAxisRotationSpeed += m_adjustSensitivityStep;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //internal void DecreaseAxisSensitivity()
        //{
        //    switch (m_mouseAxisAdjustments)
        //    {
        //        case MouseAxisAdjustments.MouseXAndY:
        //        {
        //            m_xAxisRotationSpeed -= m_adjustSensitivityStep;
        //            m_yAxisRotationSpeed -= m_adjustSensitivityStep;
        //            break;
        //        }
        //        case MouseAxisAdjustments.MouseX:
        //            m_xAxisRotationSpeed -= m_adjustSensitivityStep;
        //            break;
        //        case MouseAxisAdjustments.MouseY:
        //            m_yAxisRotationSpeed -= m_adjustSensitivityStep;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //internal void ResetAxisSensitivity()
        //{
        //    m_mouseAxisAdjustments = MouseAxisAdjustments.MouseXAndY;
        //    m_xAxisRotationSpeed = m_xMouseSensitivityDefault;
        //    m_yAxisRotationSpeed = m_yMouseSensitivityDefault;
        //}
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