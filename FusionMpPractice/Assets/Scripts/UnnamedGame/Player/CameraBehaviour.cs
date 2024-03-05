using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInputManagement
{
    public class CameraBehaviour : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;

        [Header("Camera-Positions")]
        [SerializeField] internal float m_minCamHeight = 1.5f;
        [SerializeField] internal float m_maxCamHeight = 6.0f;
        [SerializeField] private bool m_keepWorldPos = true;

        [Header("Smooth Following")]
        [SerializeField] private Transform m_parentTransform;
        [SerializeField] private Transform m_lookAtTarget;
        [SerializeField] internal Camera m_camera;
        private Transform m_currentLookAtTarget;

        [Header("Camera-Zoom")]
        [SerializeField] internal float m_zoomStep;
        [SerializeField] private float m_zoomDampening;

        private Vector3 m_zoomTarget;
        internal float m_runtimeZoomHeight;        //Zoom/Scroll-Variable.

        [Header("Camera-Rotation")]
        //[SerializeField] private MouseAxisAdjustments m_mouseAxisAdjustments;
        //private enum MouseAxisAdjustments { MouseXAndY = default, MouseX = 1, MouseY = 2, DefaultReset = 3 };
        //[SerializeField, Min(0.05f)] private float m_adjustSensitivityStep = 0.1f;
        //private float m_xMouseSensitivityDefault = 3.0f, m_yMouseSensitivityDefault = 4.0f;
        [SerializeField, Range(-85.0f, 45.0f)] private float m_minMousePitch;
        [SerializeField, Range(-85.0f, 45.0f)] private float m_maxMousePitch;
        [SerializeField, Range(0.0001f, 100f)] private float m_horiRotSpeed = 60.0f;
        [SerializeField, Range(0.0001f, 100f)] private float m_vertRotSpeed = 100.0f;
        internal Vector3 m_rotVectorYXZero;  //Sent from PlayerInput.

        private void Awake()
        {
            if (m_camera == null)
                m_camera = GetComponentInChildren<Camera>();
        }

        private void OnEnable()
        {
            SetCurrentLookAtTarget(m_lookAtTarget, m_keepWorldPos);
        }

        private void Start()
        {
            m_runtimeZoomHeight = m_camera.transform.localPosition.y;
        }

        private void Update()
        {
            Quaternion cameraParentRotation = m_parentTransform.rotation;
            //Both rotation MUST BE .rotation.
            cameraParentRotation = Quaternion.Euler(Time.deltaTime * m_horiRotSpeed * m_rotVectorYXZero.x + m_parentTransform.rotation.eulerAngles.x,
                Time.deltaTime * m_vertRotSpeed * m_rotVectorYXZero.y + m_parentTransform.rotation.eulerAngles.y, m_parentTransform.rotation.z);
            cameraParentRotation.x = Mathf.Clamp(cameraParentRotation.x, m_minMousePitch, m_maxMousePitch);
            cameraParentRotation.y = Mathf.Clamp(cameraParentRotation.y, -m_maxMousePitch * 2, m_maxMousePitch);
            m_parentTransform.rotation = cameraParentRotation;

            Debug.Log(m_rotVectorYXZero.x + m_parentTransform.rotation.eulerAngles.x);
            //ClampAngle(m_parentTransform.transform.localRotation.x, m_minMousePitch, m_maxMousePitch);
        }

        private void FixedUpdate()
        {
            UpdateZoomPosition();
        }

        #region Stack Overflow ClampAngle Method
        //private float ClampAngle(float angle, float min, float max)
        //{
        //    if (min < 0 && max > 0 && (angle > max || angle < min))
        //    {
        //        angle -= 360;
        //        if (angle > max || angle < min)
        //        {
        //            if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max)))
        //                return min;
        //            else
        //                return max;
        //        }
        //    }
        //    else if (min > 0 && (angle > max || angle < min))
        //    {
        //        angle += 360;
        //        if (angle > max || angle < min)
        //        {
        //            if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) < Mathf.Abs(Mathf.DeltaAngle(angle, max)))
        //                return min;
        //            else
        //                return max;
        //        }
        //    }

        //    if (angle < min)
        //        return min;
        //    else if (angle > max)
        //        return max;
        //    else
        //        return angle;
        //}
        #endregion

        #region Custom Methods
        private void SetCurrentLookAtTarget(Transform _lookAtTarget, bool _keepWorldPosition)
        {
            if (m_currentLookAtTarget != _lookAtTarget || m_currentLookAtTarget == null)
            {
                m_parentTransform.SetParent(_lookAtTarget, _keepWorldPosition);
                m_currentLookAtTarget = _lookAtTarget;
            }

            m_camera.transform.LookAt(_lookAtTarget);
        }
        #endregion

        private void UpdateZoomPosition()
        {
            m_zoomTarget = new Vector3(m_camera.transform.localPosition.x, m_runtimeZoomHeight, m_camera.transform.localPosition.z);

            m_zoomTarget -= m_zoomStep * (m_runtimeZoomHeight - m_camera.transform.localPosition.y) * Vector3.forward;
            m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, m_zoomTarget, Time.fixedDeltaTime * m_zoomDampening);
            SetCurrentLookAtTarget(m_lookAtTarget, true);
        }

        #region Axis Sensitivity
        //internal void IncreaseAxisSensitivity()
        //{
        //    switch (m_mouseAxisAdjustments)
        //    {
        //        case MouseAxisAdjustments.MouseXAndY:
        //        {
        //            m_horiRotSpeed += m_adjustSensitivityStep;
        //            m_vertRotSpeed += m_adjustSensitivityStep;
        //            break;
        //        }
        //        case MouseAxisAdjustments.MouseX:
        //            m_horiRotSpeed += m_adjustSensitivityStep;
        //            break;
        //        case MouseAxisAdjustments.MouseY:
        //            m_vertRotSpeed += m_adjustSensitivityStep;
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
        //            m_horiRotSpeed -= m_adjustSensitivityStep;
        //            m_vertRotSpeed -= m_adjustSensitivityStep;
        //            break;
        //        }
        //        case MouseAxisAdjustments.MouseX:
        //            m_horiRotSpeed -= m_adjustSensitivityStep;
        //            break;
        //        case MouseAxisAdjustments.MouseY:
        //            m_vertRotSpeed -= m_adjustSensitivityStep;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //internal void ResetAxisSensitivity()
        //{
        //    m_mouseAxisAdjustments = MouseAxisAdjustments.MouseXAndY;
        //    m_horiRotSpeed = m_xMouseSensitivityDefault;
        //    m_vertRotSpeed = m_yMouseSensitivityDefault;
        //}
        #endregion
    }
}