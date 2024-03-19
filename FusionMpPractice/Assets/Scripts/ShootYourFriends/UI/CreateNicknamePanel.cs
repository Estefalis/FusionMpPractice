using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuManagement
{
    public class CreateNicknamePanel : MonoBehaviour
    {
        [SerializeField] private MenuManager m_menuManager;
        [SerializeField] internal Animator m_panelAnimator;

        [Header("CreateNicknamePanel")]
        [SerializeField] private TMP_InputField m_nameInputField;
        [SerializeField] private Button m_createNicknameButton;

        private void Awake()
        {            
            m_createNicknameButton.interactable = false;
        }

        private void OnEnable()
        {
            m_createNicknameButton.onClick.AddListener(OnClickCreateNickname);
            m_nameInputField.onValueChanged?.AddListener(OnInputValueChanged);
            //m_nameInputField.onEndEdit.AddListener(OnEndEditConfirmed);
        }

        private void OnDisable()
        {
            m_nameInputField.text = string.Empty;
            m_createNicknameButton.transform.localScale = Vector3.one;
            m_createNicknameButton.onClick.RemoveListener(OnClickCreateNickname);
            m_nameInputField.onValueChanged?.RemoveListener(OnInputValueChanged);
            //m_nameInputField.onEndEdit.RemoveListener(OnEndEditConfirmed);
            StopAllCoroutines();
        }

        private void Start()
        {
            if (m_panelAnimator != null)
                m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popInClipName, transform, false);
        }

        private void OnClickCreateNickname()
        {
            if (m_nameInputField.text.Length >= m_menuManager.m_minNameLength)
            {
                m_menuManager.PlayAnimatorAndSetState(m_panelAnimator, m_menuManager.popOutClipName, transform, false);
            }
        }

        private void OnInputValueChanged(string _nickname)
        {
            m_createNicknameButton.interactable = _nickname.Length >= m_menuManager.m_minNameLength;
        }

        //private void OnEndEditConfirmed(string _nickname)
        //{
        //    m_createNicknameButton.interactable = _nickname.Length >= m_menuManager.m_minNameLength;
        //}
    }
}