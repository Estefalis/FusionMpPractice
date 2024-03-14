using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private float m_maxHealth;

        private float m_currentHP;

        private void Awake()
        {
            m_currentHP = m_maxHealth;
        }

        #region HP-Management
        public void IncreaseHealth(float _healAmount)
        {
            //Erhoehen der HP, solange der daraus resultierende Betrag unter der MaxHP liegt, ansonsten wird der MaxHP-Wert gesetzt.
            if (m_currentHP + _healAmount >= m_maxHealth)
            {
                m_currentHP = m_maxHealth;
            }
            else
            {
                m_currentHP += _healAmount;
            }

            //UpdateHealthUI(m_currentHP);
        }
        public void TakeDamage(float _damage)
        {
            m_currentHP = Mathf.Max(m_currentHP - _damage, 0);

            //UpdateHealthUI(m_currentHP);

            m_playerController.m_isDead = m_currentHP <= 0;

            if (m_playerController.m_isDead)
            {
#if UNITY_EDITOR
                Debug.Log("TILT! <(x.x)>");
#endif
                //Do what has to be done!
            }
        }
        #endregion
    }
}