using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiddleSectionPanel : MonoBehaviour
{
    [SerializeField] private MenuManager m_menuManager;
    [Header("MiddleSectionPanel")]
    [SerializeField] private Button m_joinRandomRoomButton;
    [SerializeField] private Button m_joinRoomByArgButton;
    [SerializeField] private Button m_createButton;

    [SerializeField] private TMP_InputField m_createRoomIF;
    [SerializeField] private TMP_InputField m_joinRoomByArgIF;

    //[SerializeField] private int m_minNameLength = 6;
    //[SerializeField] internal LobbyPanelTypes m_ownLobbyType;

    private NetworkRunnerController m_networkRunnerController;

    [SerializeField] private Animator m_panelAnimator;
    //const string popInClipName = "In";
    //const string popOutClipName = "Out";

    private void Start()
    {
        m_networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;
        m_joinRandomRoomButton.onClick.AddListener(OnJoinRandomRoom);
        m_joinRoomByArgButton.onClick.AddListener(() => OnCreateRoom(GameMode.Client, m_joinRoomByArgIF.text));
        m_createButton.onClick.AddListener(() => OnCreateRoom(GameMode.Host, m_createRoomIF.text));
        m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popInClipName, transform, false);
    }

    private void OnDisable()
    {
        m_joinRandomRoomButton.onClick.RemoveListener(OnJoinRandomRoom);
        m_joinRoomByArgButton.onClick.RemoveListener(() => OnCreateRoom(GameMode.Client, m_joinRoomByArgIF.text));
        m_createButton.onClick.RemoveListener(() => OnCreateRoom(GameMode.Host, m_createRoomIF.text));
        StopAllCoroutines();
    }

    private void OnJoinRandomRoom()
    {
        Debug.Log("Joining a random room!");
        m_networkRunnerController.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }
    
    private void OnCreateRoom(GameMode _gameMode, string _fieldString)
    {
        if (m_createRoomIF.text.Length >= m_menuManager.m_minNameLength)
        {
            Debug.Log($"Joining as {_gameMode}.");
            m_networkRunnerController.StartGame(_gameMode, _fieldString);
        }
    }
}