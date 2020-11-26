using UnityEngine;
using Zenject;
using Signals;

public class PlayerInputHandler : Singleton<PlayerInputHandler>
{
    [Tooltip("Sensitivity multiplier for moving the camera around")]
    public float lookSensitivity = 1f;
    [Tooltip("Additional sensitivity multiplier for WebGL")]
    public float webglLookSensitivityMultiplier = 1f;
    [Tooltip("Limit to consider an input when using a trigger on a controller")]
    public float triggerAxisThreshold = 0.4f;
    [Tooltip("Used to flip the vertical input axis")]
    public bool invertYAxis = false;
    [Tooltip("Used to flip the horizontal input axis")]
    public bool invertXAxis = false;

    private GameManager m_GameManager;
    private UIManager m_UIManager;
    private PlayerCharacterController m_PlayerCharacterController;
    
    private bool m_FireInputWasHeld;
    private bool m_AltFireInputWasHeld;
    private bool m_CanProcessInput;

    [Inject]
    public void Construct(GameManager gameManager, UIManager UIManager)
    {
        m_GameManager = gameManager;
        m_UIManager = UIManager;
    }

    private void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnGameStateChanged(GameStateChangedSignal signal)
    {
        if(signal.currentState != GameState.RUNNING)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lookSensitivity = m_UIManager.GetMouseSensitivity();
        }
    }

    private void LateUpdate()
    {
        m_FireInputWasHeld = GetFireInputHeld();
        m_AltFireInputWasHeld = GetAltFireInputHeld();
    }

    public void SetCursorLock(bool locked)
    {
        if(!locked)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        Cursor.visible = !locked;
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked && (m_GameManager == null || m_GameManager.CurrentGameState.Equals(GameState.RUNNING));
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            Vector3 move = new Vector3(Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal), 0f, Input.GetAxisRaw(GameConstants.k_AxisNameVertical));

            // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameHorizontal, GameConstants.k_AxisNameJoystickLookHorizontal);
    }

    public float GetLookInputsVertical()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameVertical, GameConstants.k_AxisNameJoystickLookVertical);
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(GameConstants.k_ButtonNameJump);
        }

        return false;
    }

    public bool GetJumpInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(GameConstants.k_ButtonNameJump);
        }

        return false;
    }

    public bool GetFireInputDown()
    {
        return GetFireInputHeld() && !m_FireInputWasHeld;
    }

    public bool GetFireInputReleased()
    {
        return !GetFireInputHeld() && m_FireInputWasHeld;
    }

    public bool GetFireInputHeld()
    {
        if (CanProcessInput())
        {
            bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) != 0f;
            if (isGamepad)
            {
                return Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) >= triggerAxisThreshold;
            }
            else
            {
                return Input.GetButton(GameConstants.k_ButtonNameFire);
            }
        }

        return false;
    }

    public bool GetAltFireInputDown()
    {
        return GetAltFireInputHeld() && !m_AltFireInputWasHeld;
    }

    public bool GetAltFireInputReleased()
    {
        return !GetAltFireInputHeld() && m_AltFireInputWasHeld;
    }

    public bool GetAltFireInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(GameConstants.k_ButtonNameAltFire);
        }

        return false;
    }

    public bool GetAimInputHeld()
    {
        if (CanProcessInput())
        {
            bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) != 0f;
            bool i = isGamepad ? (Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) > 0f) : Input.GetButton(GameConstants.k_ButtonNameAim);
            return i;
        }

        return false;
    }

    public bool GetSprintInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(GameConstants.k_ButtonNameSprint);
        }

        return false;
    }

    public bool GetCrouchInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(GameConstants.k_ButtonNameCrouch);
        }

        return false;
    }

    public bool GetCrouchInputReleased()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonUp(GameConstants.k_ButtonNameCrouch);
        }

        return false;
    }

    public int GetSwitchWeaponInput()
    {
        if (CanProcessInput())
        {

            bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadSwitchWeapon) != 0f;
            string axisName = isGamepad ? GameConstants.k_ButtonNameGamepadSwitchWeapon : GameConstants.k_ButtonNameSwitchWeapon;

            if (Input.GetAxis(axisName) > 0f)
                return -1;
            else if (Input.GetAxis(axisName) < 0f)
                return 1;
            // else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) > 0f)
            //     return -1;
            // else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) < 0f)
            //     return 1;
        }

        return 0;
    }

    public int GetSelectWeaponInput()
    {
        if (CanProcessInput())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                return 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                return 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                return 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                return 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                return 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                return 6;
            else
                return 0;
        }

        return 0;
    }
    
    public bool GetUseInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(GameConstants.k_ButtonNameUse);
        }

        return false;
    }

    float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
    {
        if (CanProcessInput())
        {
            // Check if this look input is coming from the mouse
            bool isGamepad = Input.GetAxis(stickInputName) != 0f;
            float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);

            // handle inverting vertical input
            if (invertYAxis)
                i *= -1f;

            // apply sensitivity multiplier
            i *= lookSensitivity;

            if (isGamepad)
            {
                // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
                i *= Time.deltaTime;
            }
            else
            {
                // reduce mouse input amount to be equivalent to stick movement
                i *= 0.01f;
#if UNITY_WEBGL
                // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                i *= webglLookSensitivityMultiplier;
#endif
            }

            return i;
        }

        return 0f;
    }
}
