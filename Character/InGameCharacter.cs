using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class InGameCharacter : MonoBehaviourPunCallbacks
{
    #region Variables

    private Transform characterTransform = null;

    [Header("Character function components")]
    [SerializeField] private CharacterSprite characterSprite = null;
    [SerializeField] private CharacterAnimation characterAnimation = null;
    [SerializeField] private CharacterMovement characterMovement = null;
    [SerializeField] private CharacterInteraction characterInteraction = null;
    [SerializeField] private CharacterSight characterSight = null;
    [SerializeField] private CharacterNickname characterNickname = null;

    public Action<int> dieEvent = null;

    [Header("UI components for alert")]
    [SerializeField] private GameObject canvas = null;
    [SerializeField] private UnityEngine.UI.Text alertText = null;
    private Coroutine alertCo = null;

    #endregion Variables

    #region Properties

    public Transform CharacterTransform => characterTransform;

    public int characterCount => characterInteraction.GetCountObjects((int)EObjectType.CHARACTER);
    public int bodyCount => characterInteraction.GetCountObjects((int)(EObjectType.BODY | EObjectType.NPCBODY));
    public int objectCount => characterInteraction.GetCountObjects((int)(EObjectType.OBJECT | EObjectType.NPC));
    public int npcCount => characterInteraction.GetCountObjects((int)EObjectType.NPC);
    public int npcBodyCount => characterInteraction.GetCountObjects((int)EObjectType.NPCBODY);

    #endregion Properties

    #region Unity Events

    private void Awake()
    {
        characterTransform = GetComponent<Transform>();
    }

    private void Start()
    {
        int actorNumber = photonView.Owner.ActorNumber;
        Player player = GameManager.Network.PlayerDictionaryByActorNum[actorNumber];
        PhotonHashTable playerSetting = player.CustomProperties;

        // Add player's character
        GameManager.InGame.CharacterObjectDictionary.Add(actorNumber, this);

        // Set player nickname
        characterNickname.SetText(player.NickName);

        // Set player character's color
        characterSprite.SetColor(
            "_CharacterColor",
            CharacterColor.GetColor((ECharacterColor)playerSetting[PlayerProperties.PLYAER_COLOR])
            );

        // Set the nickname color
        ERoleType localPlayerTeam = (ERoleType)GameManager.InGame.LocalPlayer.PlayerTeam;
        if (localPlayerTeam == ERoleType.CITIZEN)
        {
            characterNickname.SetColor(Color.white);
        }
        else if (localPlayerTeam == ERoleType.MAFIA)
        {
            characterNickname.SetColor((ERoleType)playerSetting[PlayerProperties.PLAYER_TEAM] == ERoleType.MAFIA ? Color.red : Color.white);
        }
        else if (localPlayerTeam == ERoleType.NEUTRAL)
        {
            if (photonView.IsMine)
            {
                characterNickname.SetColor(GameManager.Resource.Load<RoleData>("Data/RoleData").GetRoleInfo(localPlayerTeam, GameManager.InGame.LocalPlayer.PlayerRole).RoleColor);
            }
            else
            {
                characterNickname.SetColor(Color.white);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (photonView.IsMine)
            {
                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                GameManager.InGame.NPCList[ENPCRole.INSPECTION].Die(actorNumber);
            }
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

    public void StopMove()
    {
        characterAnimation.SetBool("IsWalk", false);
        characterMovement.MoveCharacter(Vector2.zero);
    }

    public void See(Vector2 direction)
    {
        characterSight.DrawSight(direction);
        characterSight.DrawCircleSight();
    }

    public int? Kill()
    {
        int killer = PhotonNetwork.LocalPlayer.ActorNumber;

        GameObject characterObject = GetNearestObject((int)EObjectType.CHARACTER);

        if (characterObject == null) return null;

        characterObject.GetComponent<InGameCharacter>().Die(killer);

        return characterObject.GetComponent<PhotonView>().Owner.ActorNumber;
    }

    public void Use()
    {
        GameObject interactableObject = GetNearestObject((int)(EObjectType.OBJECT | EObjectType.NPC));

        if (interactableObject == null) return;

        interactableObject.GetComponent<InteractableObject>().OnInteract();
    }

    public void Report()
    {
        GameObject bodyObject = GetNearestObject((int)EObjectType.BODY);

        if (bodyObject == null) return;

        if (bodyObject.CompareTag("Body"))
        {
            GameManager.Meeting.Report(bodyObject.GetComponent<PhotonView>().Owner.ActorNumber);
        }
        else if (bodyObject.CompareTag("NPCBody"))
        {
            // TODO : how to classify the actorNumber of npc (actorNubmer >= 0)
            // Need the logic for encoding and decoding the parameter
            GameManager.Meeting.Report(bodyObject.GetComponent<DeadNPC>().NPCRole);
        }
    }

    /// <summary>
    /// Greater than or equal to 0 : killed by other player.
    /// Equal to -1 : died by meeting result (kicked).
    /// Less than -1 : killed by the npc.
    /// </summary>
    /// <param name="killer"></param>
    public void Die(int killer = -1)
    {
        characterInteraction.DisableTrigger();

        if (killer >= 0 || killer < -1) // Killed by the other player or the npc
        {
            photonView.RPC(nameof(KilledRPC), RpcTarget.AllViaServer, killer);
        }
        else if (killer == -1) // Died by meeting result
        {
            photonView.RPC(nameof(KickedRPC), RpcTarget.AllViaServer);
        }
    }

    public GameObject GetNearestObject(int flag)
        => characterInteraction.GetNearestObject(flag);

    #endregion Character Action

    #region Character UI

    public void ShowMissionCount()
    {
        int totalMissionCount = (int)GameManager.Network.PlayerDictionaryByActorNum[photonView.Owner.ActorNumber].CustomProperties[PlayerProperties.TOTAL_MISSION_COUNT];
        canvas.SetActive(true);
        alertText.text = totalMissionCount.ToString();

        alertCo = StartCoroutine(ShowMissionCountCo());   
    }

    private IEnumerator ShowMissionCountCo()
    {
        yield return Utilities.WaitForSeconds(5f);

        canvas.SetActive(false);
    }

    #endregion Character UI

    #region RPC Methods

    [PunRPC]
    private void KilledRPC(int subject)
    {
        int deadPlayerActorNumber = photonView.Owner.ActorNumber;

        // Release npcs
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in GameManager.InGame.NPCList)
        {
            npc.Value.EndInteract(deadPlayerActorNumber);
        }

        dieEvent?.Invoke(deadPlayerActorNumber);

        // Trun off the canvas if it is opend
        if (alertCo != null)
        {
            StopCoroutine(alertCo);
            alertCo = null;
            canvas.SetActive(false);
        }

        // Spawn the body object if killed and set the position of the body object 
        DeadCharacter deadCharacter = GameManager.InGame.DeadCharacterObjectDictionary[deadPlayerActorNumber];
        deadCharacter.transform.position = transform.position;
        deadCharacter.gameObject.SetActive(true);

        if (photonView.IsMine) // If the local player ide
        {
            // Block the voice channel
            GameManager.Vivox.SetLocalMute();
            GameManager.UI.GetPanel<InGamePanel>().VoiceChattingButton.gameObject.SetActive(false);
            GameManager.UI.GetPanel<MeetingPanel>().VoiceChattingButton.gameObject.SetActive(false);

            // Close mission popup panel
            if (GameManager.InGame.LocalPlayer.CurTask != null)
            {
                GameManager.InGame.LocalPlayer.CurTask.OpenInGamePanel();
            }

            // Set the UI panel
            GameManager.UI.CloseAllPopupPanel();
            if (GameManager.Input.KillButton != null)
            {
                GameManager.Input.KillButton.gameObject.SetActive(false);
            }
            if (GameManager.Input.SkillButton != null)
            {
                GameManager.Input.SkillButton.gameObject.SetActive(false);
            }

            // Set player properties to indicate that the player has died
            Player player = GameManager.Network.PlayerDictionaryByActorNum[deadPlayerActorNumber];
            PhotonHashTable playerSetting = player.CustomProperties;
            playerSetting[PlayerProperties.IS_DIE] = true;
            player.SetCustomProperties(playerSetting);

            // Set animation of the dead player
            characterAnimation.SetBool("IsDead", true);

            // Set the characters of dead players to be semi-transparent
            characterSprite.SetFloat("_Alpha", 0.2f);
            foreach (int deadPlayer in GameManager.InGame.DeadPlayerList)
            {
                InGameCharacter character = GameManager.InGame.CharacterObjectDictionary[deadPlayer];

                character.characterSprite.SetFloat("_Alpha", 0.5f);
                GameManager.InGame.CharacterObjectDictionary[deadPlayer].characterSprite.SetFloat("_Alpha", 0.5f);
                GameManager.InGame.CharacterObjectDictionary[deadPlayer].characterNickname.NicknameText.gameObject.SetActive(true);
            }
        }
        else // If the other player die
        {
            bool isDie = (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.IS_DIE];

            characterSprite.SetFloat("_Alpha", isDie ? 0.5f : 0f);
            characterNickname.NicknameText.gameObject.SetActive(isDie);
        }

        if (GameManager.InGame.AliveCitizenPlayerList.Contains(deadPlayerActorNumber))
        {
            GameManager.InGame.AliveCitizenPlayerList.Remove(deadPlayerActorNumber);
        }
        else
        {
            GameManager.InGame.AliveMafiaPlayerList.Remove(deadPlayerActorNumber);
        }

        GameManager.InGame.DeadPlayerList.Add(deadPlayerActorNumber);
        GameManager.InGame.CurrentDeadPlayerList.Add(deadPlayerActorNumber);

        if (GameManager.InGame.CheckGameEnd())
        {
            GameManager.InGame.EndGame();
        }
    }

    [PunRPC]
    private void KickedRPC()
    {
        int deadPlayerActorNumber = photonView.Owner.ActorNumber;

        if (photonView.IsMine) // If the local player ide
        {
            // Block the voice channel
            GameManager.Vivox.SetLocalMute();
            GameManager.UI.GetPanel<InGamePanel>().VoiceChattingButton.gameObject.SetActive(false);
            GameManager.UI.GetPanel<MeetingPanel>().VoiceChattingButton.gameObject.SetActive(false);

            // Set player properties to indicate that the player has died
            Player player = PhotonNetwork.CurrentRoom.Players[deadPlayerActorNumber];
            PhotonHashTable playerSetting = player.CustomProperties;
            playerSetting[PlayerProperties.IS_DIE] = true;
            player.SetCustomProperties(playerSetting);

            // Set animation of the dead player
            characterAnimation.SetBool("IsDead", true);

            // Set the characters of dead players to be semi-transparent
            characterSprite.SetFloat("_Alpha", 0.2f);
            foreach (int deadPlayer in GameManager.InGame.DeadPlayerList)
            {
                GameManager.InGame.CharacterObjectDictionary[deadPlayer].characterSprite.SetFloat("_Alpha", 0.5f);
            }
        }
        else // If the other player die
        {
            characterSprite.SetFloat("_Alpha", (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.IS_DIE] ? 0.5f : 0f);
        }

        if (GameManager.InGame.AliveCitizenPlayerList.Contains(deadPlayerActorNumber))
        {
            GameManager.InGame.AliveCitizenPlayerList.Remove(deadPlayerActorNumber);
        }
        else
        {
            GameManager.InGame.AliveMafiaPlayerList.Remove(deadPlayerActorNumber);
        }

        GameManager.InGame.DeadPlayerList.Add(deadPlayerActorNumber);
        GameManager.InGame.CurrentDeadPlayerList.Add(deadPlayerActorNumber);
    }

    #endregion RPC Methods
}
