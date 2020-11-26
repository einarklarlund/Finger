using UnityEngine;
using Zenject;
using Signals;
using SaveableProfile;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject _GameManagerPrefab = null;

    [SerializeField]
    private GameObject _UIManagerPrefab = null;

    public override void InstallBindings()
    {
        /*---------------- SIGNALS ----------------*/

        // Use the signal bus
        SignalBusInstaller.Install(Container);

        // Signal declarations
        // GameManager
        Container.DeclareSignal<GameStateChangedSignal>();
        Container.DeclareSignal<StageLoadedSignal>();
        Container.DeclareSignal<DreamTransitionSignal>();
        // UI
        Container.DeclareSignal<TransitionFadedSignal>();
        Container.DeclareSignal<MainMenuFadedSignal>();
        Container.DeclareSignal<PopupRequestedSignal>();
        Container.DeclareSignal<ClearPopupRequestedSignal>();
        // Interactables
        Container.DeclareSignal<InteractableSpeechBeganSignal>();
        Container.DeclareSignal<InteractableSpeechEndedSignal>();
        Container.DeclareSignal<ItemCollectedSignal>();
        Container.DeclareSignal<ItemThrownSignal>();
        // SaveableProfiles
        Container.DeclareSignal<ProfileSavedSignal>();

        //  ---     GAMEMANAGER     ---

        Container.BindSignal<TransitionFadedSignal>()
            .ToMethod<GameManager>(manager => manager.OnTransitionFaded)
            .FromResolve();
            
        Container.BindSignal<MainMenuFadedSignal>()
            .ToMethod<GameManager>(manager => manager.OnMainMenuFaded)
            .FromResolve();

        Container.BindSignal<DreamTransitionSignal>()
            .ToMethod<GameManager>(manager => manager.OnDreamTransition)
            .FromResolve();

        //  ---     UI      ---

        // UIManager signal bindings
        Container.BindSignal<GameStateChangedSignal>()
            .ToMethod<UIManager>(manager => manager.OnGameStateChanged)
            .FromResolve();

        Container.BindSignal<StageLoadedSignal>()
            .ToMethod<UIManager>(manager => manager.OnStageLoaded)
            .FromResolve();

        Container.BindSignal<InteractableSpeechBeganSignal>()
            .ToMethod<UIManager>(manager => manager.OnInteractableSpeechBegan)
            .FromResolve();

        Container.BindSignal<PopupRequestedSignal>()
            .ToMethod<UIManager>(manager => manager.OnPopupRequested)   
            .FromResolve();

        Container.BindSignal<ClearPopupRequestedSignal>()
            .ToMethod<UIManager>(manager => manager.OnClearPopupRequested)
            .FromResolve();

        //  ---     INTERACTABLES      ---

        // PlayerInteractionManager signal bindings
        Container.BindSignal<InteractableSpeechBeganSignal>()
            .ToMethod<InteractionManager>(manager => manager.OnInteractableSpeechBegan)
            .FromResolve();

        Container.BindSignal<InteractableSpeechEndedSignal>()
            .ToMethod<InteractionManager>(manager => manager.OnInteractableSpeechEnded)
            .FromResolve();
            
        // PlayerInventoryManager signal bindings
        Container.BindSignal<ItemCollectedSignal>()
            .ToMethod<PlayerInventoryManager>(manager => manager.OnItemCollected)
            .FromResolve();

        Container.BindSignal<ItemThrownSignal>()
            .ToMethod<PlayerInventoryManager>(manager => manager.OnItemThrown)
            .FromResolve();

        Container.BindSignal<StageLoadedSignal>()
            .ToMethod<PlayerInventoryManager>(manager => manager.OnStageLoaded)
            .FromResolve();

        //  ---     SAVEABLES       ---
        
        // SaveableProfileManager
        Container.BindSignal<ProfileSavedSignal>()
            .ToMethod<SaveableProfileManager>(manager => manager.OnProfileSaved)
            .FromResolve();
            
        Container.BindSignal<DreamTransitionSignal>()
            .ToMethod<SaveableProfileManager>(manager => manager.OnDreamTransition)
            .FromResolve();

        /*---------------- DEPENDENCY INJECTIONS ----------------*/

        // Global manager bindings
        Container.Bind<GameManager>()
            .FromComponentInNewPrefab(_GameManagerPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<UIManager>()
            .FromComponentInNewPrefab(_UIManagerPrefab)
            .AsSingle()
            .NonLazy();
            
        Container.Bind<InteractionManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
            
        Container.Bind<SaveableProfileManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        Container.Bind<PlayerInventoryManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
    }
}