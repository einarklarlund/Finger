using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _resumeButton = null;
    [SerializeField] private Button _restartButton = null;
    [SerializeField] private Button _settingsButton = null;
    [SerializeField] private Button _quitButton = null;

    private GameManager _gameManager;

    [Inject]
    public void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
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
        _gameManager.RestartGame();
    }

    public void HandleSettingsClicked()
    {
        _gameManager.ToggleSettingsMenu(true);
    }
    
    public void HandleQuitClicked()
    {
        _gameManager.QuitGame();
    }
}
