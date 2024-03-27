using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerOfflineHealth : MonoBehaviour
    {
        [SerializeField] private PlayerOfflineController m_playerOfflineController;
        [SerializeField] private float m_maxHealth;

        private float m_currentHP;

        private void Awake()
        {
            m_currentHP = m_maxHealth;
        }

        #region HP-Management
        internal void IncreaseHealth(float _healAmount)
        {
            //HP cap on m_maxHealth, even if the total healAmount goes beyond.
            //TODO: May add an option to reuse any value above m_maxHeath here. 
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
        internal void TakeDamage(float _damage)
        {
            m_currentHP = Mathf.Max(m_currentHP - _damage, 0);

            //UpdateHealthUI(m_currentHP);
#if UNITY_EDITOR
            Debug.Log(m_currentHP);
#endif
            m_playerOfflineController.m_isDead = m_currentHP <= 0;

            if (m_playerOfflineController.m_isDead)
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