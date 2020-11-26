using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Signals;

public abstract class Item : Interactable
{
    public enum ItemType
    {
        EYEBALL,
        CATEYEBALL
    }

    protected SignalBus signalBus;
    
    public ItemType itemType;

    public abstract void Use();

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public override void OnSelect()
    {
        signalBus.Fire(new PopupRequestedSignal() { text = $"\"{itemType.ToString()}\"" });
    }

    public override void OnUnselect()
    {
        signalBus.Fire(new ClearPopupRequestedSignal());
    }

    public override void OnInteraction()
    {
        signalBus.Fire(new ItemCollectedSignal() { item = this });

        gameObject.SetActive(false);
    }
}
