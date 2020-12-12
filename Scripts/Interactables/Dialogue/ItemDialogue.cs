using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDialogue : PlayerCharacterDialogue
{
    [SerializeField]
    public ItemType itemType = default(ItemType);
}
