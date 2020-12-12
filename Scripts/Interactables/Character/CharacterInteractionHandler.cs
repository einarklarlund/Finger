using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class CharacterInteractionHandler : Interactable
{  
    public bool isInteracting { get; private set; }

    private DialogueBubbleAnimator _animator;
    private CharacterDialogueHandler _characterDialogueHandler;
    private CharacterInventoryHandler _characterInventoryHandler;
    private SignalBus _signalBus;

    [Inject]
    // public virtual void Construct(DialogueBubbleAnimator animator, PlayerCharacterDialoguecharacterDialogue)
    public virtual void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    //     _playerCharacterDialogue= characterDialogue;
    //     _animator = animator;
    }

    public void Start()
    {
        // I would normally do this from the construct method but Zenject is a FUCKING ASSHOLE TO ME
        _characterDialogueHandler = GetComponent<CharacterDialogueHandler>();
        _characterInventoryHandler = GetComponent<CharacterInventoryHandler>();
        _animator = GetComponent<DialogueBubbleAnimator>();
    }

    public override void OnSelect()
    {
        _animator.Play();
    }

    public override void OnInteraction()
    {
        _animator.Stop();
        
        isInteracting = true;

        // listen for DialogueEndedSignal only after OnInteraction has been executed
        _signalBus.Subscribe<DialogueEndedSignal>(_characterInventoryHandler.OnDialogueEnded);

        _characterDialogueHandler.EnterDialogue(_characterInventoryHandler.CurrentItem);
    }

    public void ExitInteraction()
    {
        isInteracting = false;
        
        _animator.Stop();
    }

    public override void OnUnselect()
    {
        _animator.Stop();
    }
}
