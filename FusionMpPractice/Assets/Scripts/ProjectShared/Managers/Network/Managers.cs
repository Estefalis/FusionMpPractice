using UnityEngine;

public class Managers : MonoBehaviour
{
    //public static Managers m_Instance => m_instance;
    //private static Managers m_instance;
    public static Managers Instance { get; private set; }

    //public NetworkManager m_NetworkManager;
    [field: SerializeField] public NetworkManager NetworkManager { get; private set; }
    public LobbyUIManager m_LobbyUIManager;

    private void Awake()
    {
        //if (m_instance !=  null)
        if (Instance !=  null)
        {
            Destroy(gameObject);
            return;
        }

        //m_instance = this;
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}