using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class StageTransitionBackdrop : MonoBehaviour
{
    public Events.EventFadeComplete OnTransitionFadeComplete;

    [SerializeField] private Animation animator = null;
    private Image backdrop = null;

    private GameManager _gameManager;
    private SignalBus _signalBus;

    [Inject]
    public void Construct(GameManager gameManager, SignalBus signalBus)
    {
        _gameManager = gameManager;
        _signalBus = signalBus;
    }

    // Start is called before the first frame update
    void Start()
    {
        backdrop = GetComponent<Image>();
        animator = GetComponent<Animation>();

        OnTransitionFadeComplete = new Events.EventFadeComplete();
    }

    public void FadeInTransition()
    {
        animator.Play("Stage Transition In");
    }

    public void FadeOutTransition()
    {
        animator.Play("Stage Transition Out");
    }

    public void OnFadeInComplete()
    {
        // OnTransitionFadeComplete.Invoke(false);
        _signalBus.Fire(new TransitionFadedSignal() { isFadeOut = false });
    }

    public void OnFadeOutComplete()
    {
        // OnTransitionFadeComplete.Invoke(true);
        _signalBus.Fire(new TransitionFadedSignal() { isFadeOut = true });
    }    
}
