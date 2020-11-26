using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ItemDialoguePair
{
    [SerializeField]
    public Item.ItemType itemType;

    [SerializeField]
    public List<string> dialogueScreens;
}

[System.Serializable]
public class InventoryAcceptancePair
{
    [SerializeField]
    public Item.ItemType oldItem;

    [SerializeField]
    public Item.ItemType newItem;
}

[System.Serializable]
public class ItemDreamTransition
{
    [SerializeField]
    public Item.ItemType itemType;
    
    [SerializeField]
    public SceneAsset dreamScene;

    [SerializeField]
    public bool isEnding;

    [SerializeField]
    public Ending ending;
    
    [SerializeField]
    public bool clearProfiles;
}
