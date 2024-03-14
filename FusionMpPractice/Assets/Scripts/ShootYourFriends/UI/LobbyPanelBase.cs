using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbyPanelType
{
    None,
    CreateNicknamePanel,
    MiddleSectionPanel
}

public class LobbyPanelBase : MonoBehaviour
{
    //[field: SerializeField, Header("LobbyPanelBase")]
    //public LobbyPanelType LobbyPanelType { get; private set; }

    ////[SerializeField] private LobbyPanelType m_lobbyPanelType;
    //[SerializeField] private Animator m_panelAnimator;
    //protected LobbyManager m_lobbyUiManager;

    //public virtual void InitializePanel(LobbyManager _uiManager)
    //{
    //    m_lobbyUiManager = _uiManager;
    //}

    //public void ShowPanel()
    //{
    //    gameObject.SetActive(true);
    //    const string popInClipName = "In";
    //    AnimationCoroutine(popInClipName, true);
    //}

    //protected void ClosePanel()
    //{
    //    const string popOutClipName = "Out";
    //    AnimationCoroutine(popOutClipName, false);
    //}

    //private void AnimationCoroutine(string _clipName, bool _stateChange)
    //{
    //    StartCoroutine(Utilities.PlayAnimatorAndSetState(gameObject, m_panelAnimator, _clipName, _stateChange));
    //}
}