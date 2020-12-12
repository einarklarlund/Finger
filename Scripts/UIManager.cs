using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Signals;

public class UIManager : MonoBehaviour
{
    [Header("Menu variables")]
    [SerializeField] private Camera _dummyCamera = null;
    [SerializeField] private MainMenu _mainMenu = null;
    [SerializeField] private PauseMenu _pauseMenu = null;
    public SettingsMenu _settingsMenu = null;

    [Header("Transition backdrop variables")]
    [SerializeField] private StageTransitionBackdrop _transitionBackdrop = null;

    [Header("Popup text variables")]
    [SerializeField] private Text _popupText = null;
    [SerializeField] private Image _popupTextBackground = null;

    [Header("Dialogue variables")]
    [SerializeField] private Text _interactableDialogueText = null;
    [SerializeField] private GameObject _interactableDialogueMenu = null;    
    [SerializeField] private Text _playerDialogueText = null;
    [SerializeField] private GameObject _playerDialogueMenu = null;
    [SerializeField] private float _textSpeed = 30;
    
    private GameManager _gameManager;
    private InteractionManager _interactionManager;
    private SignalBus _signalBus;

    private bool _skipDialogue = false;
    
    public float GetMouseSensitivity() => _settingsMenu.sensitivitySlider.value;
    
    [Inject]
    public void Construct(GameManager gameManager, InteractionManager interactionManager,
        SignalBus signalBus)
    {
        _gameManager = gameManager;
        _interactionManager = interactionManager;
        _signalBus = signalBus;
    }

    private void Start()
    {
        _interactableDialogueText.text = "";

        // hide dialogue elements
        _interactableDialogueMenu.SetActive(false);
        _playerDialogueMenu.SetActive(false);
    }

    private void Update()
    {
        if(_gameManager.loading)
        {
            return;
        }

        //start game and fade out menu if currently in MAINMENU state and space is pressed
        if(_gameManager.CurrentGameState == GameState.MAINMENU 
            && Input.GetKeyDown(KeyCode.Space))
        {
            _gameManager.StartGame();
            _mainMenu.AnimateOut();
            _dummyCamera.enabled = false; 
        }

        if(_gameManager.CurrentGameState == GameState.RUNNING
            && _interactionManager.isInteracting
            && (PlayerInputHandler.Instance.GetFireInputDown()
            || PlayerInputHandler.Instance.GetUseInputDown()))
        {
            _skipDialogue = true;
        }
    }

    public void OnGameStateChanged(GameStateChangedSignal signal)
    {
        //disable dialogue menu thing
        _interactableDialogueMenu.SetActive(false);
        _skipDialogue = false;

        //turn on pause menu if in paused state
        _pauseMenu.gameObject.SetActive(signal.currentState == GameState.PAUSED);

        //fade the main menu in if game is entering mainmenu
        if(signal.currentState == GameState.MAINMENU)
        {
            _mainMenu.AnimateIn();
            _dummyCamera.enabled = true;
        }
        else if (signal.currentState == GameState.RUNNING 
            && signal.previousState == GameState.MAINMENU)
        {
            _mainMenu.SetTransparent();
            _dummyCamera.enabled = false;
        }
    }

    // fade out the stage transition backdrop after the stage has loaded
    public void OnStageLoaded(StageLoadedSignal signal)
    {
        if(signal.isTransition)
        {
            // I would rather have this in StageTransitionBackdrop but Zenject has trouble resolving that object when the signal is called 
            _transitionBackdrop.FadeOutTransition();
            _dummyCamera.enabled = false; 
        }
    }

    // begin the stage transition animation (fade in the backdrop)
    public void BeginStageTransition()
    {
        _transitionBackdrop.FadeInTransition();
    }

    public void ToggleSettingsMenu(bool toggleOn)
    {
        if(toggleOn)
        {
            _pauseMenu.gameObject.SetActive(false);
            _settingsMenu.gameObject.SetActive(true);
        }
        else
        {
            _pauseMenu.gameObject.SetActive(true);
            _settingsMenu.gameObject.SetActive(false);
        }
    }

    public void OnPopupRequested(PopupRequestedSignal signal)
    {
        _popupText.enabled = true;
        _popupText.text = signal.text;

        if(signal.background != null)
        {
            _popupTextBackground.enabled = true;
            _popupTextBackground.sprite = signal.background;
        }
    }

    public void OnClearPopupRequested(ClearPopupRequestedSignal signal)
    {
        _popupTextBackground.enabled = false;
        _popupText.enabled = false;
    }

    public void OnDialogueBegan(DialogueBeganSignal signal)
    {
        // begin dialogue and change appropriate vars
        StartCoroutine(BeginDialogue(signal.dialogue));
    }

    IEnumerator BeginDialogue(IDialogue dialogue)
    {
        DialogueScreen dialogueScreen;
        while((dialogueScreen = dialogue.GetNextDialogueScreen()) != null)
        {
            _skipDialogue = false;

            Text dialogueText = dialogueScreen.isPlayerDialogue ? 
                _playerDialogueText : _interactableDialogueText;

            // set dialogue menus active/inactive
            _playerDialogueMenu.SetActive(dialogueScreen.isPlayerDialogue);
            _interactableDialogueMenu.SetActive(!dialogueScreen.isPlayerDialogue);

            // animate the text and wait for animation to complete
            yield return AnimateDialogueScreen(dialogueText, dialogueScreen);

            // wait for user to click until continuing to next dialogue screen
            while (!PlayerInputHandler.Instance.GetFireInputDown() &&
                !PlayerInputHandler.Instance.GetUseInputDown())
            {
                yield return null;
            }
        }

        _playerDialogueMenu.SetActive(false);
        _interactableDialogueMenu.SetActive(false);

        _signalBus.Fire(new DialogueEndedSignal());

        yield break;
    }
    
    IEnumerator AnimateDialogueScreen(Text text, DialogueScreen dialogueScreen)
    { 
        for (int dialogueIndex = 0; !_skipDialogue && dialogueIndex < dialogueScreen.text.Length; dialogueIndex++)
        {
            // set the on screen text
            text.text = dialogueScreen.text.Substring(0, dialogueIndex);
            yield return new WaitForSeconds(1 / _textSpeed);
        }

        // wait for a moment because idk it causes problems with detecting inputs if you dont
        yield return new WaitForSeconds(1 / _textSpeed);
        
        // finish the dialogue in case player has buffered skip
        text.text = dialogueScreen.text;

        yield break;
    }
}
