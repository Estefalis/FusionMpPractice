using UnityEngine;

public class GlobalManagers : MonoBehaviour
{
    public static GlobalManagers Instance { get; private set; }

    [SerializeField] private GameObject m_parentObject;
    [field: SerializeField] public NetworkRunnerController NetworkRunnerController { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(m_parentObject);
        }
    }
}