using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Signals;

public class InteractionManager : MonoBehaviour
{
    public bool isInteracting { get; private set; }
    
    [Inject]
    public void Construct()
    {
        
    }

    public void Start()
    {
        isInteracting = false;
    }

    public void OnDialogueBegan(DialogueBeganSignal signal)
    {
        isInteracting = true;
    }

    public void OnDialogueEnded(DialogueEndedSignal signal)
    {
        isInteracting = false;
    }
}
