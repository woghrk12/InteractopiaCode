using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class RoomPlayer : MonoBehaviourPunCallbacks
{
    #region Variables

    private InputManager inputManager = null;

    private CameraFollow cameraFollow = null;
    private RoomCharacter localCharacter = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void FixedUpdate()
    {
        if (localCharacter == null) return;
        if (inputManager.PlayerInput == null) return;

        localCharacter.Move(inputManager.PlayerInput.MoveDirection);
    }

    #endregion Unity Events

    #region Methods

    public void InitPlayer(RoomCharacter character)
    {
        // Set the manager components
        inputManager = GameManager.Input;

        // Set the vivox position
        GameManager.Vivox.SetPosition();

        // Set the local character
        localCharacter = character;

        // Set the camera focus
        cameraFollow.SetTarget(localCharacter.transform);

        // Init the input device
        inputManager.SetPlayerTransform(localCharacter.transform);
        inputManager.SetInputDevice();

        // Add events for the player's button
        inputManager.UseButton.Button.onClick.AddListener(Use);
        localCharacter.AddInteractionEvent(
            EObjectType.OBJECT,
            () => inputManager.UseButton.IsInteractable = true,
            () => inputManager.UseButton.IsInteractable = false
            );
    }

    public void Use() => localCharacter.Use();

    #endregion Methods
}
