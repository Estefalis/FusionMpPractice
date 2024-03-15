using Extensions;
using Fusion;
using Fusion.Photon.Realtime;   //For Regions.
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EGameScene
{
    MainMenu = 0,
    /*Local*/
    MainGame = 1,  //Local/Lan/NetGame in development.
    //Lobby //Somewhere ? separate : intergrated 
    //LanMainGame = 2,
    //NetMainGame = 3
}

namespace MenuManagement
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private EventSystem m_eventSystem;

        #region Select First Elements by using the EventSystem.
        // The stack of active (Transform-)elements for menu-navigation needs to have a Start-Transform to prevent an error. It gets set active in 'Awake()'.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        private Stack<Transform> m_activeElement = new();

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        [SerializeField] internal int m_minNameLength = 6;
        internal string popInClipName = "In";
        internal string popOutClipName = "Out";

        #region Networking
        #region Waiting
        [SerializeField] private TextMeshProUGUI m_waitingInfoText;
        [SerializeField] private GameObject m_cancelButton;
        private readonly string m_hostWaitingText = "Waiting to create game";
        private readonly string m_clientWaitingText = "Waiting to join game";
        private readonly float m_timeToChangeDot = 0.5f;
        private IEnumerator m_waitingStartGameTextCoroutine;
        private CancellationTokenSource m_cancellationTokenSource;
        #endregion

        [SerializeField] private TMP_InputField m_roomCodeInputField;
        #endregion

        private void Awake()
        {
            //PhotonAppSettings photonAppSettings = new PhotonAppSettings();
            //photonAppSettings.AppSettings.FixedRegion = "eu";
            //photonAppSettings.AppSettings.BestRegionSummaryFromStorage = ?;
            //https://forum.photonengine.com/discussion/9392/changing-regions-manually  //PUN
            //https://forum.photonengine.com/discussion/20264/how-to-change-region-by-script-in-fusion // Fusion
            SetUIStack();
        }

        #region (De-)activate Menu-Transform-Groups and set the first selected Element in each UI-Window using a stack.
        private void SetUIStack()
        {
            SetFirstElement(m_firstElement);    //1st Panel entry for the stack (Index 0).

            for (int i = 0; i < m_keyTransform.Length; i++)
                m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

            SetSelectedElement(m_firstElement);
        }

        public void NextElement(Transform _next)
        {
            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetSelectedElement(_next);
        }

        public void CloseToPreviousElement()
        {
            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetSelectedElement(previousElement);
        }

        /// <summary>
        /// The Transform gets used as the 'Key' to find the correct 'Value' in the dictionary.
        /// </summary>
        /// <param name="_activeTransform"></param>
        protected void SetSelectedElement(Transform _activeTransform)
        {
            GameObject selectElement = m_selectedElement[_activeTransform];
            m_eventSystem.SetSelectedGameObject(selectElement);
        }

        protected void SetFirstElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
        }
        #endregion

        #region Public async ButtonClicks
        public async void CreateRoomAsHost()        //In Transform with the CreateRoom-Button.
        {
            m_waitingStartGameTextCoroutine = WaitingTextTask(true);
            StartCoroutine(m_waitingStartGameTextCoroutine);

            m_cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = m_cancellationTokenSource.Token;

            //$"{PhotonNetwork.CloudRegion.ToUpper()}" to set roomname/region.
            await Managers.Instance.NetworkManager.StartGame(GameMode.Host, SessionCodeGenerator.GenerateSessionCode(), cancellationToken);
        }

        public async void JoinRoomAsClient()
        {
            m_waitingStartGameTextCoroutine = WaitingTextTask(false);
            StartCoroutine(m_waitingStartGameTextCoroutine);

            m_cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = m_cancellationTokenSource.Token;

            await Managers.Instance.NetworkManager.StartGame(GameMode.Client, m_roomCodeInputField.text, cancellationToken);

            if (m_waitingStartGameTextCoroutine != null)
            {
                StopCoroutine(m_waitingStartGameTextCoroutine);
            }

            m_waitingInfoText.text = string.Empty;

            if (m_cancelButton != null)
            {
                m_cancelButton.SetActive(false);
            }
        }
        #endregion
        #region CancelCancellationTokenSource
        public void OnCancelButtonClicked()
        {
            m_cancellationTokenSource?.Cancel();

            if (m_waitingStartGameTextCoroutine != null)
            {
                StopCoroutine(m_waitingStartGameTextCoroutine);
            }

            m_waitingInfoText.text = string.Empty;

            ReSetToFirstElement();
        }

        private IEnumerator WaitingTextTask(bool isHost)
        {
            while (true)
            {
                m_waitingInfoText.text = isHost ? m_hostWaitingText : m_clientWaitingText;

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

        private void ReSetToFirstElement()
        {
            if (!m_firstElement.gameObject.activeInHierarchy)
            {
                m_firstElement.gameObject.SetActive(true);
                SetSelectedElement(m_firstElement);
            }
        }
        #endregion

        public void LeaveSceneCoroutine(Transform _currentElement)
        {
            GameObject createRoomIF = m_selectedElement[_currentElement];
            if (createRoomIF.GetComponent<TMP_InputField>().text.Length >= m_minNameLength)
            {
                StartCoroutine(PlayAnimatorAndSetState(_currentElement.gameObject.GetComponent<Animator>(), popOutClipName/*, _currentElement, true*/));
            }
        }

        internal IEnumerator PlayAnimatorAndSetState(Animator _animator, string _clipName, Transform _nextElement = null, bool _willLeaveScene = true)
        {
            Transform currentElement = m_activeElement.Peek();
            _animator.Play(_clipName);
            var animationLength = _animator.GetCurrentAnimatorClipInfo(0).Length;
            yield return new WaitForSecondsRealtime(animationLength);
            currentElement.gameObject.SetActive(false);

            if (!_willLeaveScene && _nextElement != null)
            {
                m_activeElement.Push(_nextElement);
                _nextElement.gameObject.SetActive(true);

                SetSelectedElement(_nextElement);
            }
        }

        //internal void AnimationCoroutine(GameObject _gameObject, Animator _animator, string _clipName, bool _stateChange)
        //{
        //    StartCoroutine(Utilities.PlayAnimatorAndSetState(_gameObject, _animator, _clipName, _stateChange));
        //}
    }
}