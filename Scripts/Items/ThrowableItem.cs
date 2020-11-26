using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;
using Signals;

public class ThrowableItem : Item
{
    public float throwSpeed = 4f;

    public bool pickedUp = false;
    
    [SerializeField]
    private Rigidbody _rigidbody = null;

    public override void Use()
    {
        gameObject.SetActive(true);

        // Tell playerinventorymanager that item was thrown (will change parenting and scenes)
        signalBus.Fire(new ItemThrownSignal { throwableItem = this, transform = this.transform, rigidbody = _rigidbody });
    }

    public void OnTriggerEnter(Collider collider)
    {
        CharacterInventory characterInventory = collider.GetComponent<CharacterInventory>();

        if(!pickedUp && characterInventory && characterInventory.Accepts(itemType))
        {            
            gameObject.SetActive(false);
            characterInventory.Add(this);
            transform.SetParent(characterInventory.transform);
            pickedUp = true;
        }
    }
}
