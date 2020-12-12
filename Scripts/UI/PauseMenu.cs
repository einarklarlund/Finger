using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;
using Signals;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _resumeButton = null;
    [SerializeField] private Button _restartButton = null;
    [SerializeField] private Button _settingsButton = null;
    [SerializeField] private Button _quitButton = null;

    private GameManager _gameManager;
    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus, GameManager gameManager)
    {
        _gameManager = gameManager;
        _signalBus = signalBus;
    }

    private void Start()
    {
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _restartButton.onClick.AddListener(HandleRestartClicked);
        _settingsButton.onClick.AddListener(HandleSettingsClicked);
        _quitButton.onClick.AddListener(HandleQuitClicked);
    }
    
    public void HandleResumeClicked()
    {
        _gameManager.TogglePause();
    }
    
    public void HandleRestartClicked()
    {
        _signalBus.Fire(new DreamTransitionSignal()
        {
            clearProfiles = true,
            dreamScene = _gameManager.beginning
        });
    }

    public void HandleSettingsClicked()
    {
        _gameManager.ToggleSettingsMenu(true);
    }
    
    public void HandleQuitClicked()
    {
        _signalBus.Fire(new DreamTransitionSignal()
        {
            clearProfiles = true,
            dreamScene = _gameManager.mainMenu
        });
    }
}
