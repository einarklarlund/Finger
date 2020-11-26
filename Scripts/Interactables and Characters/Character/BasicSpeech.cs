using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicSpeech : Selectable
{

    [Header("Dialogue variables")]
    [SerializeField] private Text _dialogueText = null;
    [SerializeField] private string _dialogue = null;
    [SerializeField] private RectTransform _dialogueCanvasRect = null;
    [SerializeField] private RectTransform _backgroundRect = null;
    [SerializeField] private Image _backgroundImage = null;
    [SerializeField] private float _textSpeed = 30;
    
    private int _dialogueIndex = 0;

    private CharacterBase _characterBase;

    private Transform _playerCameraTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerCameraTransform = PlayerCharacterController.Instance.playerCamera.transform;

        _dialogueText.text = "";
        _dialogueText.enabled = false;
        _backgroundImage.enabled = false;

        _characterBase = GetComponent<CharacterBase>();
    }


    void FixedUpdate()
    {
        Quaternion newCanvasRotation = Quaternion.Euler(_playerCameraTransform.rotation.eulerAngles.x, _playerCameraTransform.rotation.eulerAngles.y, _playerCameraTransform.rotation.eulerAngles.z);
        _dialogueCanvasRect.rotation = newCanvasRotation;
    }
    
    public void Speak()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    { 
        for (int i = _dialogueIndex; i < (_dialogue.Length + 1) && selected; i++)
        {
            _dialogueText.text = _dialogue.Substring(0, i);

            _backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0.2f + _dialogueText.preferredHeight / 10);
            
            // sprite change speed is set in here so that talking speed is only set while text is being typed
            _characterBase.isTalking = true;

            _dialogueIndex = i;

            yield return new WaitForSeconds(1 / _textSpeed);
        }
        
        // sprite change speed is reset to idle speed
        _characterBase.isTalking = false;
    }

    public override void OnSelect()
    {
        _dialogueText.enabled = true;
        _backgroundImage.enabled = true;
        Speak();
    }

    public override void OnUnselect()
    {
        _dialogueText.enabled = false;
        _backgroundImage.enabled = false;
        _characterBase.isTalking = false;
    }
}
