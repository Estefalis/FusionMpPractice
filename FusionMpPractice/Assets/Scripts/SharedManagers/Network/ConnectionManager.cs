using Extensions;
using Fusion;
using MenuManagement;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//PhotonAppSettings photonAppSettings = new PhotonAppSettings();
//photonAppSettings.AppSettings.FixedRegion = "eu";
//photonAppSettings.AppSettings.BestRegionSummaryFromStorage = ?;
//https://forum.photonengine.com/discussion/9392/changing-regions-manually  //PUN
//https://forum.photonengine.com/discussion/20264/how-to-change-region-by-script-in-fusion // Fusion

public class ConnectionManager : MonoBehaviour
{
    #region Networking
    #region Waiting
    [SerializeField] private TextMeshProUGUI m_waitingInfoText;
    [SerializeField] private Button m_joinRandomRoomButton;
    [SerializeField] private Button m_joinRoomButton;
    [SerializeField] private Button m_createRoomButton;
    [SerializeField] private Button[] m_cancelButtons;
    private readonly string m_hostWaitingText = "Waiting to create game";
    private readonly string m_clientWaitingText = "Waiting to join game";
    private readonly string m_autoWaitingText = "Let me decide for you, what to do.";
    private readonly float m_timeToChangeDot = 0.5f;
    private IEnumerator m_waitingStartGameTextCoroutine;
    private CancellationTokenSource m_cancellationTokenSource;
    #endregion

    [SerializeField] private TMP_InputField m_joinRoomIF;
    [SerializeField] private TMP_InputField m_createRoomIF;
    #endregion

    #region Public async ButtonClicks
    public async void CreateRoomAsHost()        //In Transform with the CreateRoom-Button.
    {
        m_createRoomButton.gameObject.SetActive(false);
        m_cancelButtons[2].gameObject.SetActive(true);  //CreateRoomCancelButton

        m_waitingStartGameTextCoroutine = WaitingTextTask(GameMode.Host);
        StartCoroutine(m_waitingStartGameTextCoroutine);

        m_cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = m_cancellationTokenSource.Token;

        //$"{PhotonNetwork.CloudRegion.ToUpper()}" to set roomname/region.
        await ManagersDDOL.Instance.NetworkManager.StartGame(GameMode.Host, SessionCodeGenerator.GenerateSessionCode(), cancellationToken);
    }

    public async void JoinRoomAsClient()
    {
        m_joinRoomButton.gameObject.SetActive(false);
        m_cancelButtons[1].gameObject.SetActive(true);  //JoinRoomCancelButton

        m_waitingStartGameTextCoroutine = WaitingTextTask(GameMode.Client);
        StartCoroutine(m_waitingStartGameTextCoroutine);

        m_cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = m_cancellationTokenSource.Token;

        await ManagersDDOL.Instance.NetworkManager.StartGame(GameMode.Client, m_createRoomIF.text, cancellationToken);

        if (m_waitingStartGameTextCoroutine != null)
        {
            StopCoroutine(m_waitingStartGameTextCoroutine);
        }

        m_waitingInfoText.text = string.Empty;

        if (m_cancelButtons[1] != null)
        {
            m_cancelButtons[1].gameObject.SetActive(false);
        }
    }

    public async void CreateRoomAsOr()
    {
        m_joinRandomRoomButton.gameObject.SetActive(false);
        m_cancelButtons[0].gameObject.SetActive(true);  //JoinRandomCancelBtn.

        m_waitingStartGameTextCoroutine = WaitingTextTask(GameMode.AutoHostOrClient);
        StartCoroutine(m_waitingStartGameTextCoroutine);

        m_cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = m_cancellationTokenSource.Token;

        //$"{PhotonNetwork.CloudRegion.ToUpper()}" to set roomname/region.
        await ManagersDDOL.Instance.NetworkManager.StartGame(GameMode.AutoHostOrClient, SessionCodeGenerator.GenerateSessionCode(), cancellationToken);
    }
    #endregion

    #region CancelCancellationTokenSource
    public void OnCancelButtonClicked()
    {
        foreach(var button in m_cancelButtons)
        {
            if(button.gameObject.activeInHierarchy)
            {
                button.gameObject.SetActive(false);
            }
        }

        m_cancellationTokenSource?.Cancel();

        if (m_waitingStartGameTextCoroutine != null)
        {
            StopCoroutine(m_waitingStartGameTextCoroutine);
        }

        m_waitingInfoText.text = string.Empty;
        ReSetTextAndIF();
    }

    private void ReSetTextAndIF()
    {
        m_joinRoomIF.text = string.Empty;
        m_createRoomIF.text = string.Empty;
        m_joinRoomButton.gameObject.SetActive(true);
        m_joinRoomButton.transform.localScale = Vector3.one;
        m_createRoomButton.gameObject.SetActive(true);
        m_createRoomButton.transform.localScale = Vector3.one;
        m_joinRandomRoomButton.gameObject.SetActive(true);
        m_joinRandomRoomButton.transform.localScale = Vector3.one;
    }

    private IEnumerator WaitingTextTask(GameMode _gameMode)
    {
        while (true)
        {
            switch (_gameMode)
            {
                case GameMode.Host:
                {
                    m_waitingInfoText.text = m_hostWaitingText;
                    break;
                }
                case GameMode.Client:
                {
                    m_waitingInfoText.text = m_clientWaitingText;
                    break;
                }
                case GameMode.AutoHostOrClient:
                {
                    m_waitingInfoText.text = m_autoWaitingText;
                    break;
                }
            }

            //m_waitingInfoText.text = _isHost ? m_hostWaitingText : m_clientWaitingText;

            yield return new WaitForSeconds(m_timeToChangeDot);
            m_waitingInfoText.text += ".";
            yield return new WaitForSeconds(m_timeToChangeDot);
            m_waitingInfoText.text += ".";
            yield return new WaitForSeconds(m_timeToChangeDot);
            m_waitingInfoText.text += ".";
            yield return new WaitForSeconds(m_timeToChangeDot);
            m_waitingInfoText.text = m_waitingInfoText.text.Remove(m_waitingInfoText.text.Length - 3);
        }
    }
    #endregion
}