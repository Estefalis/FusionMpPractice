using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PlayerInputManagement
{
    public class InputManager : MonoBehaviour
    {
        internal static PlayerInputActions m_InputManagerActions;
        internal static event Action<InputActionMap> m_changeActiveActionMap;

        private void Awake()
        {
            //if (m_InputManagerActions == null)
            //    m_InputManagerActions = new PlayerInputActions();
            m_InputManagerActions ??= new PlayerInputActions();

            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
        }

        private void Start()
        {
            ToggleActionMaps(m_InputManagerActions.PlayerOnFootRH);
        }

        private void OnSceneFinishedLoading(Scene _scene, LoadSceneMode _mode)
        {
            switch (_scene.buildIndex)
            {
                case 0:
                {
                    ToggleActionMaps(m_InputManagerActions.DefaultUI);
                    break;
                }
                case 1:
                case 2:
                {
                    ToggleActionMaps(m_InputManagerActions.PlayerOnFootRH);
                    break;
                }
                default:
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Please define an InputAction to set at start of this Scene!");
#endif
                    break;
                }
            }
        }

        public static void ToggleActionMaps(InputActionMap _actionMap)
        {
            if (_actionMap.enabled)
                return;

            m_InputManagerActions.Disable();
            m_changeActiveActionMap?.Invoke(_actionMap);
            _actionMap.Enable();
        }
    }
}