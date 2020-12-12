using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ThemedObject : MonoBehaviour
{
    public bool isCharacter => _isCharacter;
    public bool isItem => !isCharacter;

    [SerializeField]
    private bool _isCharacter = false;
    
    [SerializeField]
    public List<Theme> themes;
}
