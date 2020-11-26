using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class InteractableSpeechInventoried : InteractableSpeech
{   
    [SerializeField]
    private List<string> _initialDialogueScreens = null;
    [SerializeField]
    public  bool canInitiateDream = false;
    
    private CharacterInventory _characterInventory;
    
    private Queue<string> _dialogueQueue;

    // public override void Construct(SignalBus signalBus, CharacterInventory characterInventory)
    [Inject]
    public override void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
        // _characterInventory = characterInventory;
    }
    
    private void Start()
    {
        _dialogueQueue = new Queue<string>();
        _characterInventory = GetComponent<CharacterInventory>();
    }

    public override void OnDialogueEntered() => 
        RefillQueue();

    public override bool HasNextDialogue() =>
        _dialogueQueue.Count > 0;

    public override string GetNextDialogue() =>
        _dialogueQueue.Dequeue();

    private void RefillQueue()
    {
        _dialogueQueue = new Queue<string>();
        List<string> dialogueScreens = _characterInventory.GetDialogueScreens();

        if(dialogueScreens == null)
        {
            dialogueScreens = _initialDialogueScreens;
        }

        dialogueScreens.ForEach(d => _dialogueQueue.Enqueue(d));
    }

    public void OnInteractableSpeechEnded(InteractableSpeechEndedSignal signal)
    {
        ItemDreamTransition itemDreamTransition = _characterInventory.GetCurrentItemDreamTransition();
        if(itemDreamTransition != default(ItemDreamTransition))
        {
            Debug.Log("Firing DreamTransitionSignal");
            SwitchDream(itemDreamTransition);
        }
    }

    public void SwitchDream(ItemDreamTransition itemDreamTransition)
    {
        signalBus.Fire(new DreamTransitionSignal() 
            { 
                isEnding = itemDreamTransition.isEnding,
                clearProfiles = itemDreamTransition.clearProfiles,
                dreamScene = itemDreamTransition.dreamScene,
                ending = itemDreamTransition.ending
            });
    }
}
