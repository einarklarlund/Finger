﻿using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEditor;
using SaveableProfile;

namespace Signals
{
    // UI Signals
    public class TransitionFadedSignal 
    { 
        public bool isFadeOut; 
    }

    public class MainMenuFadedSignal 
    { 
        public bool isFadeOut; 
    }

    public class PopupRequestedSignal
    {
        public string text;
        public Sprite background = null;
    }

    public class ClearPopupRequestedSignal
    {

    }

    // GameManager signals
    public class GameStateChangedSignal 
    { 
        public GameState currentState;
        public GameState previousState;
    }
    
    public class StageLoadedSignal 
    { 
        public bool isTransition;
        public bool isRestartingProgress = false;
    }

    // Interactable speech signals
    public class InteractableSpeechBeganSignal 
    { 
        public InteractableSpeech interactableSpeech;
    }

    public class InteractableSpeechEndedSignal 
    { 
        
    }

    // Item signals
    public class ItemCollectedSignal
    {
        public Item item;
    }
    
    public class ItemGivenSignal
    {
        public Item item;
        public CharacterInventory characterInventory;
    }

    public class ItemSwitchedSignal
    {
        public Item previousItem;
        public Item currentItem;
        public Item nextItem;
        public bool switchedForward;
    }

    public class ItemThrownSignal
    {
        public ThrowableItem throwableItem;
        public Transform transform;
        public Rigidbody rigidbody;
    }

    public class ProfileSavedSignal
    {
        public ISaveableProfile profile;
    }

    public class DreamTransitionSignal
    {
        public bool isEnding;
        public bool clearProfiles = false;
        public SceneAsset dreamScene;
        public Ending ending;
    }
}
