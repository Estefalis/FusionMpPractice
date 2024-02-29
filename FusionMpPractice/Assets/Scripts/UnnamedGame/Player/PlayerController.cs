using UnityEngine;
using PlayerInputManagement;

    internal enum EMoveModi
    {
        Idle,
        Walking,
        Running,
        Crouching
    }

    internal enum MoveInputDevice
    {
        KeyboardDominant,
        KeyboardAndMouse,
        CommonController,
        PlaystationController,
        XBoxController
    }

public class PlayerController : MonoBehaviour
{
    [SerializeField] internal EMoveModi m_eCurrentMoveMode;
    
    internal MoveInputDevice m_moveInputDevice;

    [SerializeField] internal PlayerMovement m_playerMovement;
    [SerializeField] internal PlayerInteractions m_playerInteractions;
    [SerializeField] internal PlayerHealth m_playerHealth;
    
    internal bool m_isDead = false;
}