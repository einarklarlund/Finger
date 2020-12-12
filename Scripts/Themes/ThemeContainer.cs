using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public class ThemedObjectContainer
{
    [SerializeField]
    public List<Theme> themes;
    
    public GameObject themedObject;
    
    public class Factory : PlaceholderFactory<UnityEngine.Object, ThemedObjectContainer>
    {

    }
}
