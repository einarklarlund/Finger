using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[ExecuteInEditMode]
public class CharacterBase : MonoBehaviour
{
    [Header("Texture/sprite variables")]
    [SerializeField] private Texture2D Texture1 = null;
    [SerializeField] private Texture2D Texture2 = null;
    [Tooltip("Interval in seconds at which the sprite changes from Texture 1 to Texture 2")]
    [SerializeField] private float spriteChangeSpeedIdle = 2f;
    [SerializeField] private float spriteChangeSpeedTalking = 3.5f;
    
    [HideInInspector]
    public bool isTalking;

    private Sprite _sprite1;
    private Sprite _sprite2;
    private SpriteRenderer _spriteRenderer;
    private Image _characterImage;

    private float _lastSpriteChangeTime;
    private bool _changeToSprite1;
    private Transform _playerCameraTransform;

    [Inject]
    public void Construct(Camera playerCamera)
    {
        _playerCameraTransform = playerCamera.transform;
    }

    void Start()
    {
        _sprite1 = Sprite.Create(Texture1, new Rect(0.0f, 0.0f, Texture1.width, Texture1.height), new Vector2(0.5f, 0.0f), 25.0f);
        _sprite2 = Sprite.Create(Texture2, new Rect(0.0f, 0.0f, Texture2.width, Texture2.height), new Vector2(0.5f, 0.0f), 25.0f);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = _sprite1;

        isTalking = false;

        // initialize last sprite change time so that sprites dont all change on the same interval
        _lastSpriteChangeTime = UnityEngine.Random.value * 2 / spriteChangeSpeedIdle;
        _changeToSprite1 = false;
    }

    void FixedUpdate()
    {
        float spriteChangeSpeed = isTalking ? spriteChangeSpeedTalking : spriteChangeSpeedIdle;

        if(Time.time - _lastSpriteChangeTime >= 1 / spriteChangeSpeed)
        {
            if(_changeToSprite1)
            {
                _spriteRenderer.sprite = _sprite1;
            }
            else
            {
                _spriteRenderer.sprite = _sprite2;
            }

            _changeToSprite1 = !_changeToSprite1;
            _lastSpriteChangeTime = Time.time;
        }

        // rotate to to face the camera, only in y rotation tho
        Quaternion newRotation = Quaternion.Euler(0f, _playerCameraTransform.rotation.eulerAngles.y, 0f);
        transform.rotation = newRotation;
    }
}
