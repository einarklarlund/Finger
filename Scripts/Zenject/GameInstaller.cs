using UnityEngine;
using Zenject;
using Signals;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject _PlayerPrefab = null;

    public override void InstallBindings()
    {
        /*---------------- SIGNALS ----------------*/
        
        /*
        // Signal declarations
        // GameManager
        Container.DeclareSignal<GameStateChangedSignal>();
        Container.DeclareSignal<StageLoadedSignal>();
        // UI
        Container.DeclareSignal<TransitionFadedSignal>();
        Container.DeclareSignal<MainMenuFadedSignal>();
        // Interactables
        Container.DeclareSignal<InteractableSpeechBeganSignal>();
        Container.DeclareSignal<InteractableSpeechEndedSignal>();
        */

        //      ---     PLAYER      ---

        // PlayerInputHandler bindings
        Container.BindSignal<GameStateChangedSignal>()
            .ToMethod<PlayerInputHandler>(handler => handler.OnGameStateChanged)
            .FromResolve();

        // PlayerCharacterController signal bindings
        Container.BindSignal<InteractableSpeechBeganSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnInteractableSpeechBegan)
            .FromResolve();

        Container.BindSignal<InteractableSpeechEndedSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnInteractableSpeechEnded)
            .FromResolve();
            
        Container.BindSignal<ItemThrownSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnItemThrown)
            .FromResolve();

        //  ---     INTERACTABLES      ---

        // InteractableSpeech signal bindings
        Container.BindSignal<InteractableSpeechEndedSignal>()
            .ToMethod<InteractableSpeechInventoried>(speech => speech.OnInteractableSpeechEnded)
            .FromResolveAll();

        /*---------------- DEPENDENCY INJECTIONS ----------------*/

        // Player bindings
        Container.Bind(typeof(PlayerCharacterController), 
                typeof(PlayerInteractionHandler), 
                typeof(PlayerInputHandler),
                typeof(Camera),
                typeof(Transform))
            .FromComponentInNewPrefab(_PlayerPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<PlayerStartPosition>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();

        // Container.Bind<CharacterInventory>()
        //     .FromComponentSibling();

        Container.Bind<InteractableSpeechInventoried>()
            .FromComponentsInHierarchy()
            .AsTransient();
    }
}