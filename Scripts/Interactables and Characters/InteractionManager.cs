using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Signals;

public class InteractionManager : MonoBehaviour
{
    public bool isInteracting { get; private set; }

    private UIManager m_UIManager;
    private SignalBus m_SignalBus;
    
    private InteractableSpeech m_CurrentInteractableSpeech;

    [Inject]
    public void Construct(UIManager UIManager, SignalBus signalBus)
    {
        m_UIManager = UIManager;
        m_SignalBus = signalBus;
    }

    public void Start()
    {
        isInteracting = false;
    }

    public void OnInteractableSpeechBegan(InteractableSpeechBeganSignal signal)
    {
        isInteracting = true;

        m_CurrentInteractableSpeech = signal.interactableSpeech;
    }

    public void OnInteractableSpeechEnded(InteractableSpeechEndedSignal signal)
    {
        isInteracting = false;

        m_CurrentInteractableSpeech.ExitInteraction();

        m_CurrentInteractableSpeech = null;
    }
}
