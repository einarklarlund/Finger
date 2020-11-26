using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//where T : Singleton<T> ensures that Singleton is only created with type T that extend singleton
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get { return instance; }
    }

    public static bool isInitialized
    {
        get { return instance != null; }
    }

    protected virtual void Awake()
    {
        if(instance == null) 
        {
            instance = (T) this;
        }
        else
        {
            // Destroy(gameObject);
            Debug.LogError("[Singleton] tried to instantiate a second instance of a singleton class.");
        }
    }

    protected virtual void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }
}
