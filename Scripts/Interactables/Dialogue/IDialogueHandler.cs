using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueHandler
{
    bool isPlayerSpeaking { get; }
    void EnterDialogue();
}
