using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ThemeContext : MonoBehaviour
{
    public List<Theme> themes;
    private ThemeManager _themeManager;

    [Inject]
    public void Construct(ThemeManager themeManager)
    {
        _themeManager = themeManager;
    }

    public GameObject GetObjectFromContextThemes(bool canSpawnCharacters, bool canSpawnItems)
    {
        return _themeManager.GetThemedObject(canSpawnCharacters, canSpawnItems, themes);
    }

    public GameObject GetObjectFromThemesList(bool canSpawnCharacters, bool canSpawnItems, List<Theme> list)
    {
        return _themeManager.GetThemedObject(canSpawnCharacters, canSpawnItems, list);
    }
}
