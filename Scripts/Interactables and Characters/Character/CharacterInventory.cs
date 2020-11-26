using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Signals;
using SaveableProfile;

public class CharacterInventory : MonoBehaviour, ISaveable
{
    [SerializeField]
    private List<Item.ItemType> _acceptableItems = null;

    [SerializeField]
    private List<ItemDialoguePair> _itemDialogueScreenPairs = null;

    [SerializeField]
    private List<InventoryAcceptancePair> _itemAcceptanceConditions = null;
    
    [SerializeField]
    private List<ItemDreamTransition> _ItemDreamTransitions = null;

    private Item.ItemType? _currentItemType;
    private SaveableProfileManager _profileManager;
    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus, SaveableProfileManager saveableProfileManager)
    {
        _signalBus = signalBus;
        _profileManager = saveableProfileManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(_acceptableItems == null)
        {
            _acceptableItems = new List<Item.ItemType>();
        }

        _currentItemType = null;
        
        Load();
    }

    public List<string> GetDialogueScreens()
    {
        if(_currentItemType != null)
        {
            // look through _itemDialogueScreenPairs for a tuple whose itemType matches the _currentItemType
            return _itemDialogueScreenPairs.Single(pair => pair.itemType == _currentItemType).dialogueScreens;
        }

        return null;
    }

    public ItemDreamTransition GetCurrentItemDreamTransition() =>
            _ItemDreamTransitions
            .Where(i => _currentItemType != null && i.itemType == _currentItemType)
            .FirstOrDefault();

    public bool Accepts(Item.ItemType itemType) =>
        _acceptableItems.Contains(itemType);

    public void Add(Item item)
    {
        _currentItemType = item.itemType;

        // make it so that CharacterInventory stops accepting items (unless more items are specified in _itemAcceptanceConditions)
        _acceptableItems = new List<Item.ItemType>();

        // search _itemAcceptanceConditions for a tuple whose oldItem has been added. then, add the newItem property of that tuple to the list of acceptable items. 
        _itemAcceptanceConditions.Where(cond => cond.oldItem == item.itemType)
            .ToList()
            .ForEach(cond => _acceptableItems.Add(cond.newItem));

        // save the configuration
        Save();
    }   

    public void Save()
    {
        Debug.Log("saving profile...");

        CharacterInventoryProfile profile =  new CharacterInventoryProfile() {
            id = GetID(),
            acceptableItems = _acceptableItems.Select(itemType => itemType).ToList(),
            itemAcceptanceConditions = _itemAcceptanceConditions.Select(tuple => new InventoryAcceptancePair()
                {
                   newItem = tuple.newItem,
                   oldItem = tuple.oldItem 
                }).ToList(),
            currentItemType = _currentItemType
        };

        _signalBus.Fire(new ProfileSavedSignal { profile = profile } );
    }

    public void Load()
    {
        ISaveableProfile profile = _profileManager.GetSavedProfile(this);

        // skip loading if no saved profile was 
        if(profile == default(ISaveableProfile))
        {
            return;
        }

        if(profile is CharacterInventoryProfile)
        {
            CharacterInventoryProfile invProfile = (CharacterInventoryProfile) profile;
            _acceptableItems = invProfile.acceptableItems;
            _itemAcceptanceConditions = invProfile.itemAcceptanceConditions;
            _currentItemType = invProfile.currentItemType;
        }
        else
        {
            Debug.LogError("[CharacterInventory] Tried to load a profile that wasn't a CharacterInventoryProfile");
        }
    }

    public string GetID() =>
        "CharacterInventoryHandler" + gameObject.scene.name + transform.parent.name;
}