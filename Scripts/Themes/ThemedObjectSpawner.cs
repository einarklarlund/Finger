using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ThemedObjectSpawner : MonoBehaviour
{
    public bool canSpawnCharacters;
    public bool canSpawnItems;
    public List<Theme> themes = new List<Theme>();

    private ThemeContext _themeContext;
    private DiContainer _container;

    private bool _hasSpawned;

    [Inject]
    public void Construct(ThemeContext themeContext, DiContainer container)
    {
        _themeContext = themeContext;
        _container = container;
    }

    private void Start()
    {
        // I would normally do this from the construct method but Zenject is a FUCKING ASSHOLE TO ME
        _themeContext = GameObject.FindObjectOfType<ThemeContext>();
    }
    
    private void LateUpdate()
    {
        if(!_hasSpawned)
        {
            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        GameObject themedObject;

        if(themes == null || themes.Count < 1)
        {
            themedObject = _themeContext.GetObjectFromContextThemes(canSpawnCharacters, canSpawnItems);
        }
        else
        {
            themedObject = _themeContext.GetObjectFromThemesList(canSpawnCharacters, canSpawnItems, themes);
        }

        GameObject newInstantiation = _container.InstantiatePrefab(themedObject);

        newInstantiation.transform.position = this.transform.position;
        newInstantiation.SetActive(true);
        
        _hasSpawned = true;
    }
}
