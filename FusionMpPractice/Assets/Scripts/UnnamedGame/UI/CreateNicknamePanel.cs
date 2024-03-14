using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNicknamePanel : MonoBehaviour
{
    [SerializeField] private MenuManager m_menuManager;
    [Header("CreateNicknamePanel")]
    [SerializeField] private TMP_InputField m_nameInputField;
    [SerializeField] private Button m_createNicknameButton;
    //[SerializeField] private int m_minNameLength = 6;
    [SerializeField] LobbyPanelTypes m_ownLobbyType;

    [SerializeField] internal Animator m_panelAnimator;
    //const string popInClipName = "In";
    //const string popOutClipName = "Out";

    private void Awake()
    {
        m_createNicknameButton.interactable = false;
        m_nameInputField.onEndEdit.AddListener(OnEndEditConfirmed);
        m_createNicknameButton.onClick.AddListener(OnClickCreateNickname);
        //m_nameInputField.onValueChanged?.AddListener(OnInputValueChanged);
    }

    private void Start()
    {
        m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popInClipName, transform, false);
    }

    private void OnDisable()
    {
        m_createNicknameButton.onClick.RemoveListener(OnClickCreateNickname);
        //m_nameInputField.onValueChanged?.RemoveListener(OnInputValueChanged);
        m_nameInputField.onEndEdit.RemoveListener(OnEndEditConfirmed);
        StopAllCoroutines();
    }

    private void OnClickCreateNickname()
    {
        if (m_nameInputField.text.Length >= m_menuManager.m_minNameLength)
        {
            m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popOutClipName, transform, false);
        }
    }

    //private void OnInputValueChanged(string _nickname)
    //{
    //    m_createNicknameButton.interactable = _nickname.Length >= m_minNameLength;
    //}

    private void OnEndEditConfirmed(string _nickname)
    {
        m_createNicknameButton.interactable = _nickname.Length >= m_menuManager.m_minNameLength;
    }
}