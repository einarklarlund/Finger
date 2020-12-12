using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueScreen
{

    [SerializeField]
    public string text = "";

    [SerializeField]
    public bool isSkippable = true;

    [SerializeField]
    public bool isPlayerDialogue = false;

    [SerializeField]
    public float defaultSpeed = 30f;

    [SerializeField]
    public Font defaultFont = null;

    [SerializeField]
    public List<TextShakeParameter> textShakeParameters = null;

    [SerializeField]
    public List<TextFontParameter> textFontParameters = null;
    
    [SerializeField]
    public List<TextSpeedParameter> textSpeedParameters = null;

    public int Count() => 
        text.Length;
}
