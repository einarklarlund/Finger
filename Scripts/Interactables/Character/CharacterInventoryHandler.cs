using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Signals;
using SaveableProfile;

public class CharacterInventoryHandler : MonoBehaviour, ISaveable
{
    [SerializeField]
    private InventoryConfiguration _inventoryConfiguration = null;
    // private CharacterConfiguration _inventoryConfiguration = null;
    private SignalBus _signalBus;
    private SaveableProfileManager _saveableProfileManager;

    public ItemType? CurrentItem
    {
        get { return _inventoryConfiguration.CurrentItem; }
    }

    [Inject]
    public void Construct(SignalBus signalBus, SaveableProfileManager saveableProfileManager)
    {
        _signalBus = signalBus;
        _saveableProfileManager = saveableProfileManager;
        // _inventoryConfiguration = characterConfiguration;
    }

    public bool TryAdd(ItemType newItem)
    {
        if(!_inventoryConfiguration.Accepts(newItem))
        {
            return false;
        }

        _inventoryConfiguration.UpdateItemAcceptance(newItem);

        // save the configuration
        Save();

        return true;
    }   

    public void OnDialogueEnded()
    {
        _signalBus.Unsubscribe<DialogueEndedSignal>(OnDialogueEnded);

        ItemDreamTransition itemDreamTransition = default(ItemDreamTransition);

        if(CurrentItem != null)
        {
            itemDreamTransition = _inventoryConfiguration.GetItemDreamTransition(CurrentItem);
        }

        if(itemDreamTransition != default(ItemDreamTransition))
        {
            SwitchDream(itemDreamTransition);
        }
    }

    public void SwitchDream(ItemDreamTransition itemDreamTransition)
    {
        _signalBus.Fire(new DreamTransitionSignal() 
            { 
                clearProfiles = itemDreamTransition.clearProfiles,
                dreamScene = itemDreamTransition.dreamScene,
            });
    }
    
    public void Save()
    {
        Debug.Log("saving profile... ");
        
        CharacterProfile profile =  _inventoryConfiguration.GetConfigurationProfile();
        profile.id = GetID();

        _signalBus.Fire(new ProfileSavedSignal { profile = profile } );
    }

    public void Load()
    {
        ISaveableProfile profile = _saveableProfileManager.GetSavedProfile(this);

        // skip loading if no saved profile was found
        if(profile == default(ISaveableProfile))
        {
            return;
        }

        if(profile is CharacterProfile)
        {
            _inventoryConfiguration.LoadFromProfile((CharacterProfile) profile);
        }
        else
        {
            Debug.LogError("[CharacterConfiguration] Tried to load a profile that wasn't a CharacterProfile");
        }
    }

    public string GetID() =>
        "InventoryConfiguration" + gameObject.scene.name + transform.name;
}