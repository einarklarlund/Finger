using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEditor;
using Zenject;
using Signals;

public class GameManager : MonoBehaviour
{
    //needs to know: current level, how to load/unload level, 
    //keep track of game state, generate other persistent systems

    public SceneAsset mainMenu;
    public SceneAsset beginning;
    public GameState CurrentGameState { get; private set; }

    [HideInInspector]
    public bool loading = false;
    [HideInInspector]
    public string currentStage = null;

    private UIManager _UIManager;
    private SignalBus _signalBus;

    private List<string> _loadedLevelNames;
    private List<AsyncOperation> _loadOperations;
    private List<AsyncOperation> _unloadOperations;

    private bool _transitioningToNewStage;
    private bool _isRestartingProgress;
    private string _nextTransitionStage;


    [Inject]
    public void Construct(UIManager UIManager, SignalBus signalBus)
    {
        _UIManager = UIManager;
        _signalBus = signalBus;
    }

    private void Start()
    {
        _loadedLevelNames = new List<string>();
        _loadOperations = new List<AsyncOperation>();
        _unloadOperations = new List<AsyncOperation>();

        Application.targetFrameRate = 20;
        CurrentGameState = GameState.MAINMENU;

        if(!beginning)
        {
            Debug.LogError("[GameManager] Beginning must be set in the inspector");
        }
        if(!mainMenu)
        {
            Debug.LogError("[GameManager] MainMenu must be set in the inspector");
        }

        if(SceneManager.GetActiveScene().name != "Boot")
        {
            Debug.LogError("[GameManager] Set the active scene to the \"Boot\" scene pls.");
        }
        else if(SceneManager.sceneCount == 1)
        {
            // load main menu scene if only boot scene is loaded
            currentStage = "Main Menu";
            LoadLevel(currentStage);   
        }
        else
        {
            // add current scenes to loaded level names except for boot scene
            for(int i = SceneManager.sceneCount - 1; i >= 0; --i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                
                if(scene.name != "Boot")
                {
                    SceneManager.SetActiveScene(scene);
                    currentStage = scene.name;
                    _loadedLevelNames.Add(currentStage);
                }
            }
            
            // update state to running, causes UIManager to execute OnGameStateChanged and fadeout menu
            UpdateState(GameState.RUNNING);
        }
    }

    private void Update()
    {
        if((CurrentGameState == GameState.RUNNING || CurrentGameState == GameState.PAUSED)
            && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    //public method to start the game, for use in UIManager to start game from main menu
    public void StartGame()
    {
        Debug.Log("Starting game");

        _isRestartingProgress = true;

        //prevent loading  levels while async load operation is running
        if(loading)
            return;
        
        TransitionToStage(beginning.name);
        //UpdateState is called after levels are loaded
    }

    //public method to restart the game, for use in GameOverMenu to restart game from main menu
    public void RestartGame()
    {
        // UnloadAllLevels();

        StartGame();
        // LoadLevel("Main");
        //UpdateState is called after game over menu fades out completely in HandleGameOverFadeComplete or in restart
    }

    public void QuitGame()
    {
        Debug.Log("Quitting to main menu");

        _isRestartingProgress = true;

        TransitionToStage("Main Menu");
        
        // UpdateState is called in OnLoadComplete
        // Application.Quit();
    }

    public void TransitionToStage(string newStage)
    {
        // _nextTransitionStage is set before UnloadAllLevels is called, and will be loaded in OnUnloadOperationComplete
        _nextTransitionStage = newStage;

        _transitioningToNewStage = true;

        if(PlayerCharacterController.Instance)
        {
            PlayerCharacterController.Instance.disableMovement = true;
        }

        // UpdateState is called once again in OnTransitionFaded
        UpdateState(GameState.TRANSITIONING);

        // tell UI manager to fade in the backdrop. once it is faded in, the OnTransitionFadeComplete is invoked in UI Manager, and the handler in GameManager calls LoadLevel
        _UIManager.BeginStageTransition();
    }


    public void OnDreamTransition(DreamTransitionSignal signal)
    {
        TransitionToStage(signal.dreamScene.name);
    }

    //update the game's state
    void UpdateState(GameState state)
    {  
        GameState previousGameState = CurrentGameState;
        CurrentGameState = state;

        switch (CurrentGameState)
        {           
            case GameState.PAUSED:
                Time.timeScale = 0.0f;
                break;            
            default:
                Time.timeScale = 1.0f;
                break;
        }

        // OnGameStateChanged?.Invoke(CurrentGameState, previousGameState);
        _signalBus.Fire(
            new GameStateChangedSignal() 
            { 
                currentState = CurrentGameState, 
                previousState = previousGameState 
            });
    }

    // listener function that is called on the event ao.completed (where ao is a load operation)
    void OnLoadOperationComplete(AsyncOperation ao)
    {
        if(_loadOperations.Contains(ao))
        {
            _loadOperations.Remove(ao);

            if(_loadOperations.Count == 0)
            {
                Debug.Log("Load complete.");

                loading = false;

                if(_transitioningToNewStage)
                {
                    // the current stage will always be set to the scene most recently loaded thru TransitionToStage(). 
                    // this means that the mainmenu stage will be active scene as well.
                    SceneManager.SetActiveScene((SceneManager.GetSceneByName(currentStage)));
                    
                    // OnStageLoadComplete?.Invoke();
                }

                // tell listeners that a new stage was loaded (this will tell UIManager to initiate the transition fade out animation)
                _signalBus.Fire(new StageLoadedSignal() { isTransition = _transitioningToNewStage, isRestartingProgress = _isRestartingProgress});
                
                _isRestartingProgress = false;
            }
        }
    }

    // listener function that is called on the event ao.completed (where ao is an unload operation)
    void OnUnloadOperationComplete(AsyncOperation ao)
    {        
        if(_unloadOperations.Contains(ao))
        {
            _unloadOperations.Remove(ao);
            
            if(_loadOperations.Count == 0)
            {
                // special unloading behavior or some shit idk i dont think that i need it
                Debug.Log("Unload complete.");
            }
            
        }
    }

    // public method to load a level asynchronously using Unity's SceneManager
    public void LoadLevel(string levelName)
    {
        // load the scene asynchronously using ao object
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if(loadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        // we must add our listener function (OnLoadOperationComplete) to the ao object so that it is called when the ao completes
        loadOperation.completed += OnLoadOperationComplete;
        //main scene must be set as active scene after it is loaded so that projectiles & effects load in main scene
        // if(levelName == "Main")
        // {
        //     loadOperation.completed += OnMainSceneLoadComplete;
        // }
        
        // change class vars
        _loadOperations.Add(loadOperation);
        _loadedLevelNames.Add(levelName);
        loading = true;
    }

    // public method to unload a level asynchronously using Unity's SceneManager
    public void UnloadLevel(string levelName)
    {
        // unload the scene asynchronously using ao object
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(levelName);
        if(unloadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }

        // we must add our listener function (OnLoadOperationComplete) to the ao object so that it is called when the ao completes
        unloadOperation.completed += OnUnloadOperationComplete;

        //change class vars
        _unloadOperations.Add(unloadOperation);
        _loadedLevelNames.Remove(levelName);
    }

    public void UnloadAllLevels()
    {
        // Debug.Log("Unloading all levels...");
        int count = 0;
        
        for(int i = 0; i < _loadedLevelNames.Count; ++i, ++count)
        {
            // Debug.Log("Unloading " + _loadedLevelNames[i]);
            UnloadLevel(_loadedLevelNames[i]);
        }

        // Debug.Log($"Unloaded {count} levels");
    }

    // listens to UIManager.OnMainMenuFadeComplete
    public void OnMainMenuFaded(MainMenuFadedSignal signal)
    {
        if(!signal.isFadeOut)
        {
            //after the main menu fades in, make sure that all levels are unloaded            
            UnloadAllLevels();
        }
    }

    // listens to UIManager.OnTranstionFadeComplete
    public void OnTransitionFaded(TransitionFadedSignal signal)
    {
        if(!signal.isFadeOut)
        {
            // the transition is halfway done. it needs to fade out now, but before it does, unload the previous stage and load the next stage

            // unload all levels and load the new stage
            UnloadAllLevels();

            if(_nextTransitionStage != null)
            {
                // load the new stage
                currentStage = _nextTransitionStage;
                _nextTransitionStage = null;

                Debug.Log("Loading " + currentStage);
                
                UnloadAllLevels();

                LoadLevel(currentStage);
            }
            else
            {
                Debug.LogError("[GameManager] OnTransitionFaded was called but _nextTransitionStage was null.");
            }
        }
        else
        {
            // now that the transition is done, update the state and other vars

            // update GameState from TRANSITIONING to MAINMENU or RUNNING
            UpdateState(currentStage == "Main Menu" ? GameState.MAINMENU : GameState.RUNNING);

            // set bool to false now that we have finished loading the new stage
            _transitioningToNewStage = false;

            // let the player move again
            if(PlayerCharacterController.Instance)
            {
                PlayerCharacterController.Instance.disableMovement = false;
            }
        }
    }

    //change game state to paused/running
    public void TogglePause()
    {
        UpdateState(CurrentGameState == GameState.RUNNING ? GameState.PAUSED : GameState.RUNNING);
    }

    // tell ui manager to open settings menu
    public void ToggleSettingsMenu(bool toggleOn)
    {
        _UIManager?.ToggleSettingsMenu(toggleOn);
    }
}
