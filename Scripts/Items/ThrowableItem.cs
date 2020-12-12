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
    
    private Rigidbody _rigidbody = null;

    [Inject]
    public void Construct(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public override void Use()
    {
        gameObject.SetActive(true);

        // Tell playerinventorymanager that item was thrown (will change parenting and scenes)
        signalBus.Fire(new ItemThrownSignal { throwableItem = this, transform = this.transform, rigidbody = _rigidbody });
    }

    public void OnCollisionEnter(Collision collision)
    {
        CharacterInventoryHandler characterInventory = collision.collider.GetComponent<CharacterInventoryHandler>();
        characterInventory = characterInventory ?? collision.collider.GetComponentInParent<CharacterInventoryHandler>();

        if(characterInventory && characterInventory.TryAdd(itemType))
        {            
            gameObject.SetActive(false);
            transform.SetParent(characterInventory.transform);
            pickedUp = true;
        }
    }
}
