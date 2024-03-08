using UnityEngine;

public class CameraControlOIS : MonoBehaviour
{
    /// <summary>
    /// Steuerung der Kamera um den Player herum. Enum für die Wahl zwischen X&Y, X oder Y-Einstellmöglichkeit.
    /// </summary>
    private enum MouseAxisAdjustments { MouseXAndY = 0, MouseX = 1, MouseY = 2, DefaultReset = 3 };
    [SerializeField] private MouseAxisAdjustments m_mouseAxisAdjustments;

    private Transform m_camera;
    private Transform m_cameraHolder;   //CameraParent

    [Header("Movement-Settings")]
    [SerializeField] private string m_mouseHorizontalAxis = "Mouse X";
    [SerializeField] private string m_mouseVerticalAxis = "Mouse Y";
    [SerializeField] private string m_mouseScrollWheel = "Mouse ScrollWheel";

    /// <summary>
    /// Begrenzung der Kamerabewegung nach oben/unten und Möglichkeiten zur Feineinstellungen.
    /// </summary>
    [SerializeField] private float m_minMousePitch = 0f;
    [SerializeField] private float m_maxMousePitch = 90f;
    [SerializeField, Range(0.0001f, 4f)] private float m_mouseSensitivitySideways = 3f;
    [SerializeField, Range(0.0001f, 4f)] private float m_mouseSensitivityUpDown = 4f;
    [SerializeField] private float m_minimalSensitivity = -4f;
    [SerializeField] private float m_maximalSensitivity = 4f;
    [SerializeField, Min(0.05f)] private float m_adjustSensitivityStep = 0.1f;

    //Invertierungsmöglichkeiten der Achsen.
    [SerializeField] private bool m_invertXAxis = false;
    [SerializeField] private bool m_invertYAxis = false;

    //Einstellung für die Bewegungs-/ Zoomgeschwindigkeit der Kamera.
    [Header("Zoom-Settings")]
    [SerializeField] private float m_cameraDampeningAmount = 0.3f;
    [SerializeField] private float m_scrollSensitivity = 2.5f;
    [SerializeField] private float m_closestZoomDistance = 1.5f;
    [SerializeField] private float m_farestZoomDistance = 50f;
    [SerializeField] private float m_moveSpeed = 5f;    //OrbitDampening

    [SerializeField] private bool m_disableCameraRotation = false;  //CameraDisabled

    bool lockCursor = true;

    private float m_xMouseSensitivityDefault;
    private float m_yMouseSensitivityDefault;

    private float m_scrollSpeed = 6f;   //ScrollDampening

    private float m_cameraDistance = 10f;

    private Vector3 m_updatedRotation;  //LocalRotation

    private void Awake()
    {
        //Verbinden der Kamera mit dessen Parent.
        this.m_camera = this.transform;
        this.m_cameraHolder = this.transform.parent;

        //Cursor unsichtbar in der Mitte halten.
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Start()
    {
        //Standardeinstellung der Kamera-Achsenstuerung einstellen.
        m_mouseAxisAdjustments = MouseAxisAdjustments.MouseXAndY;
        m_xMouseSensitivityDefault = m_mouseSensitivitySideways;
        m_yMouseSensitivityDefault = m_mouseSensitivityUpDown;
    }

    private void Update()
    {
        //Wechsel zwischen den Modi für die Achseneinstellung und Möglichkeit der Achseninvertierung.
        if (Input.GetKeyDown(KeyCode.Alpha1))
            m_mouseAxisAdjustments = MouseAxisAdjustments.MouseXAndY;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            m_mouseAxisAdjustments = MouseAxisAdjustments.MouseX;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            m_mouseAxisAdjustments = MouseAxisAdjustments.MouseY;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            m_mouseAxisAdjustments = MouseAxisAdjustments.DefaultReset;

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.X))
            m_invertXAxis = !m_invertXAxis;
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Y))
            m_invertYAxis = !m_invertYAxis;

        //Toggle CameraMovement On/Off.
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
            m_disableCameraRotation = !m_disableCameraRotation;

        SelectAxisAdjustmentMode();
    }

    public void LateUpdate()
    {
        if (!m_disableCameraRotation)
        {
            //Wenn die Kamera sich bewegen soll, wird hier die Invertierung der Achsen vorgenommen.
            if (Input.GetAxis(m_mouseHorizontalAxis) != 0f || Input.GetAxis(m_mouseVerticalAxis) != 0f)
            {
                if (m_invertXAxis)
                    m_updatedRotation.x -= Input.GetAxis(m_mouseHorizontalAxis) * m_mouseSensitivitySideways;
                else
                    m_updatedRotation.x += Input.GetAxis(m_mouseHorizontalAxis) * m_mouseSensitivitySideways;

                if (m_invertYAxis)
                    m_updatedRotation.y += Input.GetAxis(m_mouseVerticalAxis) * m_mouseSensitivityUpDown;
                else
                    m_updatedRotation.y -= Input.GetAxis(m_mouseVerticalAxis) * m_mouseSensitivityUpDown;

                //Clamp the rotation, so the camera does not flip over.
                //if (m_updatedRotation.y < m_minMousePitch)
                //    m_updatedRotation.y = m_minMousePitch;
                //else if (m_updatedRotation.y > m_maxMousePitch)
                //    m_updatedRotation.y = m_maxMousePitch;
                //oder
                m_updatedRotation.y = Mathf.Clamp(m_updatedRotation.y, m_minMousePitch, m_maxMousePitch);
            }

            //Wenn das Mausrad nicht stillsteht und schächt die Kamerabewegung je nach Entfernung zum Parent ab.
            if (Input.GetAxis(m_mouseScrollWheel) != 0f)
            {
                float scrollAmount = Input.GetAxis(m_mouseScrollWheel) * m_scrollSensitivity;

                //Hier 30%. Je weiter die Kamera weg ist, desto schneller soll sie sich bewegen.
                scrollAmount *= (this.m_cameraDistance * m_cameraDampeningAmount);

                this.m_cameraDistance += scrollAmount * -1f;

                //Limitierung auf Mindest- und Maximalabstand von 1.5m - 50m.
                this.m_cameraDistance = Mathf.Clamp(this.m_cameraDistance, m_closestZoomDistance, m_farestZoomDistance);
            }
        }

        //Einstellung der Kamera-Orientatierung, lerpen zwischen den Rotationen.
        Quaternion actualCameraOrientation = Quaternion.Euler(m_updatedRotation.y, m_updatedRotation.x, 0f);
        this.m_cameraHolder.rotation = Quaternion.Lerp(m_cameraHolder.rotation, actualCameraOrientation, Time.deltaTime * m_moveSpeed);

        //Update nur, wenn ein Positionswechsel stattgefunden hat.
        if (m_camera.localPosition.z != this.m_cameraDistance * -1f)
        {
            //Interpolierung zwischen aktuieller und Zielposition/m_clampedCameraDistance.
            this.m_camera.localPosition = new Vector3(0f, 0f, Mathf.Lerp(this.m_camera.localPosition.z, this.m_cameraDistance * -1f,
                Time.deltaTime * m_scrollSpeed));
        }
    }

    //Erhöhung der Maus-Sensitivity je nach Modi X&Y, X/Y.
    private void SelectAxisAdjustmentMode()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseXAndY)
            {
                m_mouseSensitivitySideways += m_adjustSensitivityStep;
                m_mouseSensitivityUpDown += m_adjustSensitivityStep;
            }
            else if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseX)
            {
                m_mouseSensitivitySideways += m_adjustSensitivityStep;
            }
            else if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseY)
            {
                m_mouseSensitivityUpDown += m_adjustSensitivityStep;
            }
        }

        //Verringerung der Maus-Sensitivity je nach Modi X&Y, X/Y.
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseXAndY)
            {
                m_mouseSensitivitySideways -= m_adjustSensitivityStep;
                m_mouseSensitivityUpDown -= m_adjustSensitivityStep;
            }
            else if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseX)
            {
                m_mouseSensitivitySideways -= m_adjustSensitivityStep;
            }
            else if (m_mouseAxisAdjustments == MouseAxisAdjustments.MouseY)
            {
                m_mouseSensitivityUpDown -= m_adjustSensitivityStep;
            }
        }

        //Reset des Modus auf die Beinflussung beider Achsen und den eingestellten Standardwert.
        if (m_mouseAxisAdjustments == MouseAxisAdjustments.DefaultReset)
        {
            m_mouseSensitivitySideways = m_xMouseSensitivityDefault;
            m_mouseSensitivityUpDown = m_yMouseSensitivityDefault;
            m_mouseAxisAdjustments = MouseAxisAdjustments.MouseXAndY;
        }

        //Begrenzung auf Mininmal- und Maximalwert nach der Eingabe, damit zu große, bzw. zu kleine Eingaben angefangen werden.
        if (m_mouseSensitivitySideways >= m_maximalSensitivity)
        {
            m_mouseSensitivitySideways = m_maximalSensitivity;
        }
        else if (m_mouseSensitivitySideways <= m_minimalSensitivity)
        {
            m_mouseSensitivitySideways = m_minimalSensitivity;
        }

        if (m_mouseSensitivityUpDown >= m_maximalSensitivity)
        {
            m_mouseSensitivityUpDown = m_maximalSensitivity;
        }
        else if (m_mouseSensitivityUpDown <= m_minimalSensitivity)
        {
            m_mouseSensitivityUpDown = m_minimalSensitivity;
        }
    }
}