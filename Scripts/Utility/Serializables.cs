using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class ItemDialoguePair
{
    [SerializeField]
    public ItemType itemType;

    [SerializeField]
    public List<string> dialogueScreens;
}

[Serializable]
public class InventoryAcceptancePair
{
    [SerializeField]
    public ItemType oldItem;

    [SerializeField]
    public ItemType newItem;
}

[Serializable]
public class ItemDreamTransition
{
    [SerializeField]
    public ItemType itemType;
    
    [SerializeField]
    public SceneAsset dreamScene;

    [SerializeField]
    public Ending ending;
    
    [SerializeField]
    public bool clearProfiles;
}

[Serializable]
public class TextShakeParameter
{
    [SerializeField]
    public int startIndex;

    [SerializeField]
    public int endIndex;

    [SerializeField]
    public float xShake;

    [SerializeField]
    public float yShake;
}

[Serializable]
public class TextFontParameter
{
    [SerializeField]
    public int startIndex;

    [SerializeField]
    public int endIndex;

    [SerializeField]
    public Font font;
}

[Serializable]
public class TextSpeedParameter
{
    [SerializeField]
    public int startIndex;

    [SerializeField]
    public int endIndex;

    [SerializeField]
    public float speed;
}