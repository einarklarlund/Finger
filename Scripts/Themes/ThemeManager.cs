using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Signals;

public class ThemeManager : MonoBehaviour
{
    [SerializeField]
    private List<ThemedObject> _themedObjects = null;

    private List<ThemedObject> _objectsForCurrentScene;

    public void OnStageLoaded(StageLoadedSignal signal)
    {
        _objectsForCurrentScene = new List<ThemedObject>(_themedObjects);
    }

    public void Start()
    {
        _objectsForCurrentScene = new List<ThemedObject>(_themedObjects);
    }
    
    public GameObject GetThemedObject(bool canSpawnCharacters, bool canSpawnItems, List<Theme> themes)
    {
        if(_objectsForCurrentScene.Count < 1)
        {
            Debug.LogError("[ThemeManager] Tried to get a themed object from ThemeManager, but no objects were left in _objectsForCurrentScene");
            return null;
        }

        List<ThemedObject> objects = _objectsForCurrentScene.Where(o =>
                themes.Any(t => o.themes.Contains(t)) && 
                ((o.isItem && canSpawnItems) || (o.isCharacter && canSpawnCharacters)))
            .ToList();

        int index = (int) (UnityEngine.Random.value * _objectsForCurrentScene.Count);

        ThemedObject choice = _objectsForCurrentScene[index];

        _objectsForCurrentScene.Remove(choice);

        return choice.gameObject;
    }
}
