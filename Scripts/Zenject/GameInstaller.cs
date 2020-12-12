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
        
        //      ---     PLAYER      ---

        // PlayerInputHandler bindings
        Container.BindSignal<GameStateChangedSignal>()
            .ToMethod<PlayerInputHandler>(handler => handler.OnGameStateChanged)
            .FromResolve();

        // PlayerCharacterController signal bindings
        Container.BindSignal<DialogueBeganSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnDialogueBegan)
            .FromResolve();

        Container.BindSignal<DialogueEndedSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnDialogueEnded)
            .FromResolve();
            
        Container.BindSignal<ItemThrownSignal>()
            .ToMethod<PlayerCharacterController>(controller => controller.OnItemThrown)
            .FromResolve();

        //  ---     INTERACTABLES      ---

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

        // character bindings
            
        // Container.Bind<CharacterConfiguration>()
        //     .FromComponentSibling()
        //     .NonLazy();

        // Container.Bind<CharacterDialogue>()
        //     .FromComponentSibling()
        //     .NonLazy();

        // Container.Bind<DialogueBubbleAnimator>()
        //     .FromComponentSibling()
        //     .NonLazy();

        // Container.Bind<CharacterInteractionHandler>()
        //     .FromComponentSibling()
        //     .NonLazy();
        
        // Container.Bind(typeof(CharacterDialogue), 
        //         typeof(CharacterConfiguration), 
        //         typeof(CharacterInteractionHandler),
        //         typeof(DialogueBubbleAnimator))
        //     .FromComponentSibling()
        //     .AsTransient()
        //     .NonLazy();

        // item bindings
        Container.Bind<Rigidbody>()
            .FromComponentSibling()
            .WhenInjectedInto<ThrowableItem>();

        // theme bindings
        Container.Bind<ThemeContext>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();
    }
}