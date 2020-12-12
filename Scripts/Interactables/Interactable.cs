using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : Selectable
{

    public void Interact()
    {
        if(selected && playerInSelectionCollider)
        {
            OnInteraction();
        }
    }

    public abstract void OnInteraction();
}
