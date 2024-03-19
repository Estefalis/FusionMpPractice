using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MenuManagement
{
    public class MiddleSectionPanel : MonoBehaviour
    {
        //Keeping the lambda expression to keep the syntax in mind.
        [SerializeField] private MenuManager m_menuManager;
        [SerializeField] private Animator m_panelAnimator;

        //[Header("MiddleSectionPanel")]
        [SerializeField] private Button m_joinRandomRoomButton;
        [SerializeField] private Button m_joinRoomButton;
        [SerializeField] private Button m_createRoomButton;
        [SerializeField] private Button m_backButton;

        [SerializeField] private TMP_InputField m_joinRoomIF;
        [SerializeField] private TMP_InputField m_createRoomIF;

        private void OnEnable()
        {
            m_joinRoomIF.onValueChanged?.AddListener(OnJoinRoomNameChange);
            m_createRoomIF.onValueChanged?.AddListener(OnCreateRoomNameChange);

            m_joinRoomButton.interactable = false;
            m_createRoomButton.interactable = false;
        }

        private void OnDisable()
        {
            ReSetTextAndIF();
            m_joinRoomIF.onValueChanged.RemoveListener(OnJoinRoomNameChange);
            m_createRoomIF.onValueChanged.RemoveListener(OnCreateRoomNameChange);
            StopAllCoroutines();

            //m_joinRandomRoomButton.onClick.RemoveListener(OnJoinRandomRoom);
            //m_joinRoomButton.onClick.RemoveListener(() => OnCreateRoom(GameMode.Client, m_joinRoomIF.text));
            //m_createRoomButton.onClick.RemoveListener(() => OnCreateRoom(GameMode.Host, m_createRoomIF.text));
        }

        private void Start()
        {
            if (m_panelAnimator != null)
                m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popInClipName, transform, false);

            //m_networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;
            //m_joinRandomRoomButton.onClick.AddListener(OnJoinRandomRoom);
            //m_joinRoomButton.onClick.AddListener(() => OnCreateRoom(GameMode.Client, m_joinRoomIF.text));
            //m_createRoomButton.onClick.AddListener(() => OnCreateRoom(GameMode.Host, m_createRoomIF.text));
        }

        private void ReSetTextAndIF()
        {
            m_joinRoomIF.text = string.Empty;
            m_createRoomIF.text = string.Empty;
            m_joinRoomButton.gameObject.SetActive(true);
            m_joinRoomButton.transform.localScale = Vector3.one;
            m_createRoomButton.gameObject.SetActive(true);
            m_createRoomButton.transform.localScale = Vector3.one;
            m_backButton.gameObject.SetActive(true);
            m_backButton.transform.localScale = Vector3.one;
        }

        private void OnJoinRoomNameChange(string _roomName)
        {
            m_joinRoomButton.interactable = _roomName.Length >= m_menuManager.m_minNameLength;
        }

        private void OnCreateRoomNameChange(string _playerInput)
        {
            m_createRoomButton.interactable = _playerInput.Length >= m_menuManager.m_minNameLength;
        }

        //private void OnJoinRandomRoom()
        //{
        //    Debug.Log("Joining a random room!");
        //    m_networkRunnerController.StartGame(GameMode.AutoHostOrClient, string.Empty);
        //}

        //private void OnCreateRoom(GameMode _gameMode, string _fieldString)
        //{
        //    if (m_createRoomIF.text.Length >= m_menuManager.m_minNameLength)
        //    {
        //        Debug.Log($"Joining as {_gameMode}.");
        //        m_networkRunnerController.StartGame(_gameMode, _fieldString);
        //    }
        //}
    }
}