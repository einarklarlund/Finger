using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour, IAnimatedMenu
{
    public Text welcomeText;
    public Events.EventFadeComplete OnMainMenuAnimationComplete;

    [SerializeField] private Animation _mainMenuAnimator = null;
    
    private void Start()
    {
        welcomeText.text = "Press Space to start.";
    }

    public void OnAnimationOutComplete()
    {
        OnMainMenuAnimationComplete.Invoke(true);

        gameObject.SetActive(false);
    }

    public void OnAnimationInComplete()
    {
        OnMainMenuAnimationComplete.Invoke(false);
    }

    public void AnimateIn()
    {
        gameObject.SetActive(true);

        _mainMenuAnimator.Stop();
        _mainMenuAnimator.Play("Main Menu Fade In");
    }

    public void AnimateOut()
    {
        _mainMenuAnimator.Stop();
        _mainMenuAnimator.Play("Main Menu Fade Out");
    }

    public void SetTransparent(bool transparent = true)
    {
        _mainMenuAnimator.Stop();
        _mainMenuAnimator.Play("Main Menu Set Transparent");
    }
}
