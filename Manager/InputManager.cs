using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    public enum EInputMode
    {
        NONE = -1,
        KEYBOARD,
        MOUSE,
        GAMEPAD,
        TOUCH
    }

    #region Variables

    public PlayerInput PlayerInputDevice = null;

    public PlayerArrowInput PlayerArrowInput = null;
    public PlayerScreenInput PlayerScreenInput = null;
    public PlayerStickInput PlayerStickInput = null;

    public PlayerButton UseButton = null;
    public PlayerButton KillButton = null;
    public PlayerButton ReportButton = null;
    public PlayerButton SkillButton = null;

    #endregion Variables

    #region Properties

    public IMoveDirection PlayerInput { private set; get; }
    public EInputMode InputMode { private set; get; } = EInputMode.NONE;

    #endregion Properties

    public void Init()
    {
        if (PlayerPrefs.HasKey("InputMode"))
        {
            //InputMode = (EInputMode)PlayerPrefs.GetInt("InputMode");
            InputMode = EInputMode.GAMEPAD;
        }
        else
        {
#if !UNITY_EDITOR

    // Mobile
    #if (UNITY_ANDROID || UNITY_IOS)
            InputMode =  EInputMode.GAMEPAD;
    // PC
    #elif (UNITY_STANDALONE)
            InputMode =  EInputMode.KEYBOARD;
    #endif

#else
            InputMode =  EInputMode.GAMEPAD;
#endif
        }
    }

    public void SetInputDevice()
    {
        SwitchInputMode(InputMode);
    }

    public void SetInputDevice(EInputMode inputMode)
    {
        SwitchInputMode(inputMode);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        PlayerScreenInput.SetPlayerTransform(playerTransform);
    }

    public void StopMove()
    {
        if (PlayerInput == null) return;

        PlayerInput.StopMove();
    }

    public void SetInteractableUseButton(bool isInteractable)
    {
        if (UseButton == null) return;

        UseButton.IsInteractable = isInteractable;
    }

    public void SetInteractableKillButton(bool isInteractable)
    {
        if (KillButton == null) return;

        KillButton.IsInteractable = isInteractable;
    }

    public void SetInteractableReportButton(bool isInteractable)
    {
        if (ReportButton == null) return;

        ReportButton.IsInteractable = isInteractable;
    }

    #region Helper Methods

    private void SwitchInputMode(EInputMode inputMode)
    {
        List<InputDevice> devices = new();

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.displayName.ToUpper().Contains(inputMode.ToString()))
            {
                devices.Add(device);
            }
        }

        InputDevice[] deviceArray = devices.ToArray();

        InputMode = inputMode;

        switch (InputMode)
        {
            case EInputMode.KEYBOARD:
                PlayerInput = PlayerArrowInput;
                PlayerStickInput.gameObject.SetActive(false);
                PlayerInputDevice.SwitchCurrentControlScheme("Keyboard", deviceArray);
                break;

            case EInputMode.MOUSE:
                PlayerInput = PlayerScreenInput;
                PlayerStickInput.gameObject.SetActive(false);
                PlayerInputDevice.SwitchCurrentControlScheme("Mouse", deviceArray);
                break;

            case EInputMode.GAMEPAD:
                PlayerInput = PlayerStickInput;
                PlayerStickInput.gameObject.SetActive(true);
                PlayerInputDevice.SwitchCurrentControlScheme("Gamepad", deviceArray);
                break;

            case EInputMode.TOUCH:
                PlayerInput = PlayerScreenInput;
                PlayerStickInput.gameObject.SetActive(false);
                PlayerInputDevice.SwitchCurrentControlScheme("Touch", deviceArray);
                break;
        }

        PlayerPrefs.SetInt("InputMode", (int)InputMode);
    }

    #endregion Helper Methods
}
