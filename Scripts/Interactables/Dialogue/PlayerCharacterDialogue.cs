using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCharacterDialogue: IDialogue
{
    [SerializeField]
    private List<DialogueScreen> _dialogueScreens = null;

    private int _currentScreenIndex = 0;

    public DialogueScreen GetNextDialogueScreen()
    {
        if(_currentScreenIndex < _dialogueScreens.Count)
        {
            return _dialogueScreens[_currentScreenIndex++];
        }
        else
        {
            _currentScreenIndex = 0;
            return null;
        }
    }
}
