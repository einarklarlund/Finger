using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SettingsMenu : MonoBehaviour
{
    public Slider sensitivitySlider = null;
    [SerializeField] private Text sensitivityText = null;
    [SerializeField] private Button _backButton = null;

    private UIManager m_UIManager;

    [Inject]
    public void Construct(UIManager UIManager)
    {
        m_UIManager = UIManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        sensitivitySlider.onValueChanged.AddListener(delegate {HandleSensitivitySliderChanged(); });

        sensitivityText.text = "Sensitivity: " + Math.Truncate(100 * (sensitivitySlider.value)) / 100;

        _backButton.onClick.AddListener(HandleBackClicked);
    }

    public void HandleSensitivitySliderChanged()
    {
        sensitivityText.text = "Sensitivity: " + Math.Truncate(100 * (sensitivitySlider.value)) / 100;
    }

    private void HandleBackClicked()
    {
        m_UIManager.ToggleSettingsMenu(false);
    }
}
