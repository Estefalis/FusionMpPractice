using UnityEngine;

public class PlayerController : MonoBehaviour
{
    internal enum EMoveModi
    {
        Idle,
        Walking,
        Running,
        Crouching
    }

    [SerializeField] internal EMoveModi m_eCurrentMoveMode;

    internal enum MoveInputDevice
    {
        KeyboardDominant,
        KeyboardAndMouse,
        CommonController,
        PlaystationController,
        XBoxController
    }
    
    internal MoveInputDevice m_moveInputDevice;

    [SerializeField] internal PlayerMovement m_playerMovement;
    [SerializeField] internal PlayerInteractions m_playerInteractions;
    [SerializeField] internal PlayerHealth m_playerHealth;
    
    internal bool m_isDead = false;
}