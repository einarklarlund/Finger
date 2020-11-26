using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    [HideInInspector]
    public bool selectable { get; protected set; }
    [HideInInspector]
    public bool selected { get; protected set; }
    [HideInInspector]
    public bool playerInSelectionCollider = false;
    
    public abstract void OnSelect();
    public abstract void OnUnselect();

    public Selectable TrySelect()
    {
        if(playerInSelectionCollider && !selected)
        {
            selected = true;
            OnSelect();
            return this;
        }

        return null;
    }
    
    public void TryUnselect()
    {
        if(selected)
        {
            selected = false;
            OnUnselect();
        }
    }

    public void Update()
    {
        if(selected && !playerInSelectionCollider)
        {
            TryUnselect();
        }
    }
}
