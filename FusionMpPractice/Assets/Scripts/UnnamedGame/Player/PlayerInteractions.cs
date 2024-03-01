using UnityEngine;

namespace PlayerInputManagement
{
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;

        private void OnCollisionEnter(Collision _collision)
        {

        }

        private void OnCollisionStay(Collision collision)
        {
            
        }

        private void OnCollisionExit(Collision _collision)
        {

        }

        private void OnTriggerEnter(Collider _other)
        {

        }

        private void OnTriggerStay(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider _other)
        {

        }
    }
}