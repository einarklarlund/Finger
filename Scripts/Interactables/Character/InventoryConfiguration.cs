using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Signals;
using SaveableProfile;

[Serializable]
public class InventoryConfiguration
{
    [SerializeField]
    private List<ItemType> _acceptableItems = null;

    [SerializeField]
    private List<InventoryAcceptancePair> _itemAcceptanceConditions = null;
    
    [SerializeField]
    private List<ItemDreamTransition> _itemDreamTransitions = null;

    private SaveableProfileManager _saveableProfileManager;
    private SignalBus _signalBus;

    private ItemType? _currentItemType;

    public ItemType? CurrentItem 
    {
        get { return _currentItemType; }
    }

    public CharacterProfile GetConfigurationProfile()
    {
        return new CharacterProfile() 
        {
            acceptableItems = _acceptableItems.Select(itemType => itemType).ToList(),
            itemAcceptanceConditions = _itemAcceptanceConditions.Select(tuple => new InventoryAcceptancePair()
                {
                   newItem = tuple.newItem,
                   oldItem = tuple.oldItem 
                }).ToList(),
            currentItemType = _currentItemType
        };
    }

    public void LoadFromProfile(CharacterProfile profile)
    {
        _acceptableItems = profile.acceptableItems;
        _itemAcceptanceConditions = profile.itemAcceptanceConditions;
        _currentItemType = profile.currentItemType;
    }

    public InventoryConfiguration(SignalBus signalBus, SaveableProfileManager saveableProfileManager)
    {
        _saveableProfileManager = saveableProfileManager;
        _signalBus = signalBus;

        if(_acceptableItems == null)
        {
            _acceptableItems = new List<ItemType>();
        }
    }

    public bool Accepts(ItemType itemType)
    {
        return _acceptableItems.Contains(itemType);
    }

    public ItemDreamTransition GetItemDreamTransition(ItemType? itemType)
    {
        return _itemDreamTransitions.Where(i => 
                _currentItemType != null && i.itemType == itemType)
            .FirstOrDefault();
    }

    public void UpdateItemAcceptance(ItemType newItem)
    {
        _currentItemType = newItem;

        // make it so that CharacterInventory stops accepting items (unless more items are specified in _itemAcceptanceConditions)
        _acceptableItems = new List<ItemType>();

        // search _itemAcceptanceConditions for a tuple whose oldItem has been added. then, add the newItem property of that tuple to the list of acceptable items. 
        _itemAcceptanceConditions.Where(cond => cond.oldItem == newItem)
            .ToList()
            .ForEach(cond => _acceptableItems.Add(cond.newItem));
    }
}
