using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum LobbyPanelTypes
{
    None,
    CreateNicknamePanel,
    MiddleSectionPanel
}

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

    private void Awake()
    {
        SetUIElements();
    }

    private void SetUIElements()
    {
        SetFirstElement(m_firstElement);    //1st Panel for the stack (Index 0).

        for (int i = 0; i < m_keyTransform.Length; i++)
            m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

        SetSelectedElement(m_firstElement);
    }

    #region Methods to (de-)activate Menu-Transforms with a stack and to set the active Element in each UI-Window.
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
        //var animationLength = 0;

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

    #region Callback Contexts FOR LATER
    //private void EnableNavigation(InputAction.CallbackContext _callbackContext)
    //{
    //    if (!m_firstElement.gameObject.activeInHierarchy)
    //    {
    //        m_firstElement.gameObject.SetActive(true);
    //        SetSelectedElement(m_firstElement);
    //    }
    //}
    #endregion
}