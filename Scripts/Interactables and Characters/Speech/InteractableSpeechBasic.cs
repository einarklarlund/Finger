using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class InteractableSpeechBasic : InteractableSpeech
{   
    [SerializeField]
    private List<string> _dialogueScreens = null;
    private Queue<string> _dialogueQueue;
    private string s;

    // using Construct as a constructor bc superclass InteractableSpeech implements Start()
    [Inject]
    public void Construct()
    {
        _dialogueQueue = new Queue<string>();
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
        _dialogueScreens.ForEach(d => _dialogueQueue.Enqueue(d));
    }
}
