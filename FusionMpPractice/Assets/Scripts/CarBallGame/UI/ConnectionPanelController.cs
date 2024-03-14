using Extensions;
using Fusion;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;

namespace LobbyUI
{
    public class ConnectionPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject hostButton;
        [SerializeField] private GameObject joinButton;
        [SerializeField] private GameObject roomCodeInputField;
        [SerializeField] private GameObject enterButton;
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject cancelButton;
        [SerializeField] private TextMeshProUGUI infoText;
        
        private TMP_InputField _roomCodeInputField;
        private IEnumerator _waitingStartGameTextCoroutine;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _hostWaitingText = "Waiting to create game";
        private readonly string _clientWaitingText = "Waiting to join game";
        private readonly float _timeToChangeDot = 0.5f;
        
        private void Awake()
        {
            _roomCodeInputField = roomCodeInputField.GetComponent<TMP_InputField>();
        }
        
        private void Start()
        {
            SetDefaultState();
        }
        
        public async void OnHostButtonClicked()
        {
            cancelButton.SetActive(true);
            hostButton.SetActive(false);
            joinButton.SetActive(false);

            _waitingStartGameTextCoroutine = WaitingTextTask(true);
            StartCoroutine(_waitingStartGameTextCoroutine);

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            await Managers.Instance.NetworkManager.StartGame(GameMode.Host, SessionCodeGenerator.GenerateSessionCode(), cancellationToken);

            #region Removed in Guide Process
            //if (gameObject == null || gameObject.activeInHierarchy == false)
            //    return;

            //if (m_waitingStartGameTextCoroutine != null)
            //{
            //    StopCoroutine(m_waitingStartGameTextCoroutine);
            //}

            //m_waitingInfoText.text = string.Empty;

            //if (m_cancelButton != null)
            //{
            //    m_cancelButton.SetActive(false);
            //}
            #endregion
        }
        
        public async void OnClientEnteredButtonClicked()
        {
            cancelButton.SetActive(true);
            roomCodeInputField.SetActive(false);
            enterButton.SetActive(false);
            backButton.SetActive(false);

            _waitingStartGameTextCoroutine = WaitingTextTask(false);
            StartCoroutine(_waitingStartGameTextCoroutine);

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            await Managers.Instance.NetworkManager.StartGame(GameMode.Client, _roomCodeInputField.text, cancellationToken);

            if (_waitingStartGameTextCoroutine != null)
            {
                StopCoroutine(_waitingStartGameTextCoroutine);
            }

            infoText.text = string.Empty;

            if (cancelButton != null)
            {
                cancelButton.SetActive(false);
            }
        }
        
        public void OnCancelButtonClicked()
        {
            //m_waitingInfoText.text = string.Empty;
            _cancellationTokenSource?.Cancel();

            if (_waitingStartGameTextCoroutine != null)
            {
                StopCoroutine(_waitingStartGameTextCoroutine);
            }

            infoText.text = string.Empty;

            SetDefaultState();
        }

        public void OnJoinButtonClicked()
        {
            cancelButton.SetActive(false);
            hostButton.SetActive(false);
            joinButton.SetActive(false);
            roomCodeInputField.SetActive(true);
            enterButton.SetActive(true);
            backButton.SetActive(true);
        }

        public void OnBackButtonClicked()
        {
            SetDefaultState();
        }
        
        private void SetDefaultState()
        {
            hostButton.SetActive(true);
            joinButton.SetActive(true);
            roomCodeInputField.SetActive(false);
            enterButton.SetActive(false);
            backButton.SetActive(false);
            cancelButton.SetActive(false);
            infoText.text = string.Empty;
        }
        
        private IEnumerator WaitingTextTask(bool isHost)
        {
            while (true)
            {
                infoText.text = isHost ? _hostWaitingText : _clientWaitingText;
            
                yield return new WaitForSeconds(_timeToChangeDot);
                infoText.text += ".";
                yield return new WaitForSeconds(_timeToChangeDot);
                infoText.text += ".";
                yield return new WaitForSeconds(_timeToChangeDot);
                infoText.text += ".";
                yield return new WaitForSeconds(_timeToChangeDot);
                infoText.text = infoText.text.Remove(infoText.text.Length - 3);
            }
        }
    }
}
