using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers m_Instance => m_instance;
    private static Managers m_instance;

    public NetworkManager m_NetworkManager;
    public LobbyUIManager m_LobbyUIManager;

    private void Awake()
    {
        if (m_instance !=  null)
        {
            Destroy(gameObject);
            return;
        }

        m_instance = this;

        DontDestroyOnLoad(gameObject);
    }
}