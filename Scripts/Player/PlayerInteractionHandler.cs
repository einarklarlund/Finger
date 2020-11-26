using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerInteractionHandler : MonoBehaviour
{
    private Selectable _lastSelectable;
    private Camera _playerCamera; 
    private InteractionManager _interactionManager;

    [Inject]
    public void Construct(InteractionManager interactionManager, Camera playerCamera)
    {
        _playerCamera = playerCamera;
        _interactionManager = interactionManager;
    }

    public void Update()
    {
        Selectable selectable = null;

        // Look for selectable collider
        Collider selectableCollider = RaycastForSelectableCollider();
        
        // Set selectable component if we found one in raycast, null otherwise
        if(selectableCollider)
        {
            selectable = selectableCollider.GetComponent<Selectable>() ? 
                selectableCollider.GetComponent<Selectable>() :
                selectableCollider.GetComponentInParent<Selectable>() ? 
                selectableCollider.GetComponentInParent<Selectable>() :
                null;
        }
        
        if(selectable && !_interactionManager.isInteracting)
        {
            int selectableID = selectable.gameObject.GetInstanceID();

            if(!selectable.selected)
            {
                // unselect the last selectable
                _lastSelectable?.TryUnselect();

                // try to select the new selectable
                _lastSelectable = selectable.TrySelect();
            }

            // if mouse clicked, look for an interactable
            if(PlayerInputHandler.Instance.GetUseInputDown())
            {            
                Interactable interactable = selectable.GetComponent<Interactable>();

                // interact with Interactable if it has been found. 
                interactable?.Interact();
            }
        }
        else if(_lastSelectable && !_interactionManager.isInteracting)
        {
            // if not pointing at a selectable and we were in the last frame, then unset _lastSelectable and unselect it
            _lastSelectable.TryUnselect();
            _lastSelectable = null;
        }
    }

    public Collider RaycastForSelectableCollider()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Selectable");
        RaycastHit hit;

        if(Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            return hit.collider;
        }

        return null;
    }

    public void OnTriggerEnter(Collider other)
    {
        Selectable selectable = other.gameObject.GetComponent<Selectable>();
        
        if(selectable)
        {
            selectable.playerInSelectionCollider = true;
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        Selectable selectable = other.gameObject.GetComponent<Selectable>();

        if(selectable)
        {
            selectable.playerInSelectionCollider = false;
        }
    }
}
