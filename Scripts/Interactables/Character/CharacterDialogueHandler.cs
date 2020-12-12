using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class CharacterDialogueHandler : MonoBehaviour
{   
    public Transform viewPoint = null;
    
    [SerializeField] 
    private DialogueConfiguration _dialogueConfiguration = null;
    
    private SignalBus _signalBus;
    // private CharacterConfiguration _dialogueConfiguration;
    
    private Queue<string> _dialogueQueue;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
        // _dialogueConfiguration = characterConfiguration;
    }

    public void EnterDialogue(ItemType? currentItem)
    {
        // find the 
        IDialogue dialogue = _dialogueConfiguration.GetCurrentDialogue(currentItem);
        // List<string> dialogue = _dialogueConfiguration.GetCurrentDialogue();

        // tell the ui manager about the dialogue and shit);
        _signalBus.Fire(new DialogueBeganSignal() 
            {
                dialogue = dialogue,
                viewPoint = this.viewPoint,
            });
    } 
}
