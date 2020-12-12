using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueConfiguration
{
    [SerializeField]
    private PlayerCharacterDialogue _initialDialogue = null;

    [SerializeField]
    private List<ItemDialogue> _itemDialogues = null;

    public IDialogue GetCurrentDialogue(ItemType? currentItem)
    {
        IDialogue dialogue = _initialDialogue;
        ItemDialogue itemDialogue = null;
        
        if(currentItem != null)
        {
            itemDialogue = GetItemDialogue((ItemType) currentItem);
        }
        
        if(itemDialogue != null)
        {
            dialogue = itemDialogue;
        }
        
        return dialogue;
    }

    private ItemDialogue GetItemDialogue(ItemType itemType)
    {
        return _itemDialogues.Where(itemDialogue => 
                itemDialogue.itemType == itemType)
            .FirstOrDefault();
    }
}
