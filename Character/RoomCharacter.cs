using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class RoomCharacter : MonoBehaviourPunCallbacks
{
    #region Variables

    [SerializeField] private CharacterAnimation characterAnimation = null;
    [SerializeField] private CharacterSprite characterSprite = null;
    [SerializeField] private CharacterMovement characterMovement = null;
    [SerializeField] private CharacterInteraction characterInteraction = null;
    [SerializeField] private CharacterNickname characterNickname = null;

    #endregion Variables

    #region Unity Events

    private void Start()
    {
        int actorNumber = photonView.Owner.ActorNumber;
        Player player = GameManager.Network.PlayerDictionaryByActorNum[actorNumber];
        PhotonHashTable playerSetting = player.CustomProperties;

        // Add player's character
        GameManager.Title.CharacterObjectDicionary.Add(actorNumber, this);

        characterSprite.gameObject.SetActive(true);
        characterNickname.SetText(player.NickName);

        if (playerSetting.TryGetValue(PlayerProperties.PLYAER_COLOR, out object value))
        {
            characterSprite.SetColor("_CharacterColor", CharacterColor.GetColor((ECharacterColor)value));
        }

        if (photonView.IsMine)
        {
            characterAnimation.SetTrigger("DoSpawn");
        }
    }

    #endregion Unity Events

    #region Methods

    public void AddInteractionEvent(EObjectType type, Action addEvent, Action removeEvent)
    {
        characterInteraction.AddObjectEvents[type] = addEvent;
        characterInteraction.RemoveObjectEvents[type] = removeEvent;
    }

    #endregion Methods

    #region Character Action

    public void Move(Vector2 moveDirection)
    {
        characterAnimation.SetBool("IsWalk", moveDirection != Vector2.zero);
        characterMovement.MoveCharacter(moveDirection);
    }

    public void Use()
    {
        GameObject interactableObject = GetNearestObject((int)EObjectType.OBJECT);

        if (interactableObject == null) return;

        interactableObject.GetComponent<InteractableObject>().OnInteract();
    }

    #endregion Character Action

    #region Helper Methods

    private GameObject GetNearestObject(int flag)
        => characterInteraction.GetNearestObject(flag);

    #endregion Helper Methods

    #region Photon Events

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashTable changedProps)
    {
        if (targetPlayer.ActorNumber != photonView.Owner.ActorNumber) { return; }

        characterSprite.SetColor("_CharacterColor", CharacterColor.GetColor((ECharacterColor)changedProps[PlayerProperties.PLYAER_COLOR]));
    }

    #endregion Photon Events
}
