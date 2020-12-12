using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DialogueBubbleAnimator : MonoBehaviour
{
    [Header("Dialogue bubble variables")]
    [SerializeField] 
    private float _dialogeBubbleSpeed = 2;
    [SerializeField]
    private Canvas _dialogueBubbleCanvas = null;

    // to be set in Start method
    private IEnumerator _dialogueBubbleCoroutine;
    private CharacterInteractionHandler _characterInteractionHandler;
    
    // [Inject]
    // public void Construct(CharacterInteractionHandler characterInteractionHandler)
    // {
    //     _characterInteractionHandler = characterInteractionHandler;
    // }

    void Start()
    {
        // I would normally do this from the construct method but Zenject is a FUCKING ASSHOLE TO ME
        _characterInteractionHandler = GetComponent<CharacterInteractionHandler>();

        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
        }

        _dialogueBubbleCoroutine = AnimateDialogueBubble();
    }

    public void Play()
    {
        if(_dialogueBubbleCanvas)
        {
            StartCoroutine(_dialogueBubbleCoroutine);
        }
    }
    
    public void Stop()
    {
        // disable dialogue canvas
        if(_dialogueBubbleCanvas)
        {
            _dialogueBubbleCanvas.enabled = false;
            StopCoroutine(_dialogueBubbleCoroutine);
        }
    }

    IEnumerator AnimateDialogueBubble()
    { 
        while(!_characterInteractionHandler.isInteracting && _characterInteractionHandler.playerInSelectionCollider)
        {
            _dialogueBubbleCanvas.enabled = !_dialogueBubbleCanvas.enabled;

            yield return new WaitForSeconds(1 / _dialogeBubbleSpeed);
        }

        yield break;
    }

}
