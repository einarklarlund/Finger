using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Events 
{
    // game manager stuff
    [System.Serializable] public class EventGameState : UnityEvent<GameState, GameState> { }
    [System.Serializable] public class EventStageLoadComplete : UnityEvent { };

    // ui stuff
    [System.Serializable] public class EventFadeComplete : UnityEvent<bool> { }
    // pickup stuff
    [System.Serializable] public class EventPickup : UnityEvent<PlayerPickupManager> { };

    // interactable speech stuff
    [System.Serializable] public class EventBeginInteractableSpeech : UnityEvent<InteractableSpeech> { }
    [System.Serializable] public class EventEndInteractableSpeech : UnityEvent { }
}
