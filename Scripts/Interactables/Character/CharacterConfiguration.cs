// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using Zenject;
// using Signals;
// using SaveableProfile;
// using System.Linq;

// public class CharacterConfiguration : MonoBehaviour, ISaveable
// {
//     [Header("Dialogue")]

//     // [SerializeField]
//     // private List<string> _initialDialogueScreens = null;

//     // [SerializeField]
//     // private List<ItemDialoguePair> _itemDialogueScreenPairs = null;

//     [SerializeField]
//     private PlayerCharacterDialogue_initialDialogues = null;

//     [SerializeField]
//     private ItemDialogue _itemDialogues = null;

//     [Header("Inventory")]

//     [SerializeField]
//     private List<ItemType> _acceptableItems = null;

//     [SerializeField]
//     private List<InventoryAcceptancePair> _itemAcceptanceConditions = null;
    
//     [SerializeField]
//     private List<ItemDreamTransition> _itemDreamTransitions = null;


//     private SignalBus _signalBus;
//     private SaveableProfileManager _saveableProfileManager;

//     private ItemType? _currentItemType;

//     // TODO: add ItemDialogue class as serializefield here

//     [Inject]
//     public void Construct(SignalBus signalBus, SaveableProfileManager saveableProfileManager)
//     {
//         _saveableProfileManager = saveableProfileManager;
//         _signalBus = signalBus;
//     }

//     private void Start()
//     {
//         if(_acceptableItems == null)
//         {
//             _acceptableItems = new List<ItemType>();
//         }

//         Load();
//     }

//     public bool Accepts(ItemType itemType) =>
//         _acceptableItems.Contains(itemType);

//     public ItemDreamTransition CurrentItemDreamTransition() =>
//         _itemDreamTransitions.Where(i => 
//             _currentItemType != default(ItemType) && i.itemType == _currentItemType)
//         .FirstOrDefault();
        
//     private ItemDialoguePair CurrentItemDialoguePair() =>
//         _itemDialogueScreenPairs.Where(pair => 
//             pair.itemType == _currentItemType)
//         .FirstOrDefault();

//     // public List<string> GetCurrentDialogue()
//     public IDialogue GetCurrentDialogue(ItemType currentItem)
//     {
//         IDialogue dialogue = _initialDialogues;
        
//         ItemDialoguePair pair = CurrentItemDialoguePair();
        
//         if(pair != null)
//         {
//             dialogue = pair.dialogueScreens;
//         }
        
//         return dialogue;
//     }

//     public void UpdateItemAcceptance(ItemType newItem)
//     {
//         _currentItemType = newItem;

//         // make it so that CharacterInventory stops accepting items (unless more items are specified in _itemAcceptanceConditions)
//         _acceptableItems = new List<ItemType>();

//         // search _itemAcceptanceConditions for a tuple whose oldItem has been added. then, add the newItem property of that tuple to the list of acceptable items. 
//         _itemAcceptanceConditions.Where(cond => cond.oldItem == newItem)
//             .ToList()
//             .ForEach(cond => _acceptableItems.Add(cond.newItem));
//     }
    
//     public void Save()
//     {
//         Debug.Log("saving profile... ");
        
//         CharacterProfile profile =  new CharacterProfile() 
//         {
//             id = GetID(),
//             acceptableItems = _acceptableItems.Select(itemType => itemType).ToList(),
//             itemAcceptanceConditions = _itemAcceptanceConditions.Select(tuple => new InventoryAcceptancePair()
//                 {
//                    newItem = tuple.newItem,
//                    oldItem = tuple.oldItem 
//                 }).ToList(),
//             currentItemType = _currentItemType
//         };

//         _signalBus.Fire(new ProfileSavedSignal { profile = profile } );
//     }

//     public void Load()
//     {
//         ISaveableProfile profile = _saveableProfileManager.GetSavedProfile(this);

//         // skip loading if no saved profile was found
//         if(profile == default(ISaveableProfile))
//         {
//             return;
//         }

//         if(profile is CharacterProfile)
//         {
//             CharacterProfile invProfile = (CharacterProfile) profile;
//             _acceptableItems = invProfile.acceptableItems;
//             _itemAcceptanceConditions = invProfile.itemAcceptanceConditions;
//             _currentItemType = invProfile.currentItemType;
//         }
//         else
//         {
//             Debug.LogError("[CharacterConfiguration] Tried to load a profile that wasn't a CharacterProfile");
//         }
//     }

//     public string GetID() =>
//         "CharacterConfiguration" + gameObject.scene.name + transform.name;
// }
