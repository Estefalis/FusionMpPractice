using System.Collections;
using UnityEngine;

public static class Utilities
{
    public static IEnumerator PlayAnimatorAndSetState(GameObject _parent, Animator _animator, string _clipName, bool _stateOnFinished = true)
    {
        _animator.Play(_clipName);
        var animationLength = _animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return new WaitForSeconds(animationLength);
        _parent.SetActive(_stateOnFinished);
    }
}