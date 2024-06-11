using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagersDDOL : MonoBehaviour
{
    //public static Managers m_Instance => m_instance;
    //private static Managers m_instance;
    public static ManagersDDOL Instance { get; private set; }

    //public NetworkRunnerManager m_NetworkManager;
    [field: SerializeField] public NetworkRunnerManager NetworkManager { get; private set; }
    //[SerializeField] public MenuManager MenuManager { get; private set; }

    private void Awake()
    {
        //if (m_instance !=  null)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //m_instance = this;
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}