using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public abstract class InteractableSpeech : Interactable
{   
    [Header("Dialogue bubble variables")]
    [SerializeField] private float _dialogeBubbleSpeed = 2;

    [Header("Dialogue variables")]
    public Transform viewPoint = null;
    public bool isInteracting { get; protected set; }
    public bool isPlayerSpeaking { get; protected set; }
    
    // to be set in Start method
    private CharacterBase _characterBase;
    private Transform _playerCameraTransform;
    private Canvas _dialogueBubbleCanvas;
    private IEnumerator _dialogueBubbleCoroutine;

    protected SignalBus signalBus;
    
    public abstract bool HasNextDialogue();
    public abstract string GetNextDialogue();
    public abstract void OnDialogueEntered();

    [Inject]
    public virtual void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    void Start()
    {
        isInteracting = false;

        _playerCameraTransform = PlayerCharacterController.Instance.playerCamera.transform;

        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
        }

        _dialogueBubbleCoroutine = AnimateDialogueBubble();

        _characterBase = GetComponent<CharacterBase>();

        _dialogueBubbleCanvas = GetComponentInChildren<Canvas>();

        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
        }
    }

    public void Speak()
    {
        if(_dialogueBubbleCanvas)
        {
            StartCoroutine(_dialogueBubbleCoroutine);
        }
    }

    public void SetTalkingAnimationActive(bool active)
    {
        if(_characterBase)
        {
            _characterBase.isTalking = active;
        }
    }

    IEnumerator AnimateDialogueBubble()
    { 
        while(selected && !isInteracting)
        {
            _dialogueBubbleCanvas.enabled = !_dialogueBubbleCanvas.enabled;

            yield return new WaitForSeconds(1 / _dialogeBubbleSpeed);
        }

        yield break;
    }

    public override void OnSelect()
    {
        Speak();
    }

    public override void OnInteraction()
    {
        isInteracting = true;

        // disable dialogue canvas
        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
            StopCoroutine(_dialogueBubbleCoroutine);
        }
        
        OnDialogueEntered();

        // tell the interaction handler about the dialogue and shit
        signalBus.Fire(new InteractableSpeechBeganSignal() { interactableSpeech = this });
    }

    public void ExitInteraction()
    {
        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = true;
        }

        isInteracting = false;
        
        Speak();
    }

    public override void OnUnselect()
    {
        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
            StopCoroutine(_dialogueBubbleCoroutine);
        }
    }
}
