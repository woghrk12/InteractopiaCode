using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class TitleManager : MonoBehaviourPunCallbacks
{
    private readonly int COUNT_DOWN = 5;

    #region Variables

    private PhotonView managerPhotonView = null;
    private Camera mainCamera = null;

    private RoomPlayer localPlayer = null;

    private SpawnPositionData spawnPositions = null;

    private Dictionary<int, RoomCharacter> characterObjectDicionary = new();

    private Coroutine countDownCo = null;

    #endregion Variables

    #region Properties

    public RoomPlayer LocalPlayer => localPlayer;

    public Dictionary<int, RoomCharacter> CharacterObjectDicionary => characterObjectDicionary;

    public Action<int> CountDownEvent = null;

    #endregion Properties

    #region Methods

    public void Init()
    {
        managerPhotonView = GetComponent<PhotonView>();
        mainCamera = Camera.main;

        spawnPositions = GameManager.Resource.Load<SpawnPositionData>("Data/RoomSpawnPositionData");

        localPlayer = GameManager.Resource.Instantiate("PlayerPrefabs/RoomPlayer").GetOrAddComponent<RoomPlayer>();
    }

    public void StartScene()
    {
        if (PhotonNetwork.InRoom)
        {
            EnterRoomEffect();
            GameManager.Vivox.ReleaseVoice();
            GameManager.UI.OpenPanel<RoomPanel>();
        }
        else
        {
            GameManager.UI.OpenPanel<StartPanel>();
        }
    }

    private void SpawnPlayerCharacter()
    {
        // Get
        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        int spawnIndex = (int)roomSetting[CustomProperties.SPAWN_POSITION];
        
        // Set
        roomSetting[CustomProperties.SPAWN_POSITION] = (spawnIndex + 1) % spawnPositions.GetLength();
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);

        RoomCharacter character = PhotonNetwork.Instantiate(CharacterPrefabName.roomCharacterPrefabName, spawnPositions.GetPosition(spawnIndex), Quaternion.identity).GetComponent<RoomCharacter>();
        if (character.photonView.IsMine)
        {
            localPlayer.InitPlayer(character);
        }
    }

    public void LeaveRoom()
    {
        LeaveRoomEffect();
        characterObjectDicionary.Clear();

        if (countDownCo != null)
        {
            StopCoroutine(countDownCo);
            countDownCo = null;
        }

        GameManager.Vivox.SetLocalMute();

        GameManager.UI.OpenPanel<StartPanel>();
    }

    #endregion Methods

    #region Game Start

    public bool CheckCanStartGame()
    {
        if (GameManager.Network.PlayerCount > characterObjectDicionary.Count)
        {
            GameManager.UI.Alert("Not all players are ready yet.");
            return false;
        }

        return true;
    }

    public void GameStart()
    {
        GameManager.Network.InitReadyState();

        countDownCo = StartCoroutine(CountDownCo());
    }

    [PunRPC]
    private void CountDownRPC(int countDown)
    {
        CountDownEvent?.Invoke(countDown);
    }

    private IEnumerator CountDownCo()
    {
        int countDown = COUNT_DOWN;

        while (countDown > 0)
        {
            managerPhotonView.RPC(nameof(CountDownRPC), RpcTarget.AllViaServer, countDown);
            yield return Utilities.WaitForSeconds(1f);
            countDown--;
        }

        managerPhotonView.RPC(nameof(GameStartRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void GameStartRPC()
    {
        GameManager.Vivox.BlockVoice();

        Tween fadeOutTween = GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration);
        fadeOutTween.onComplete += () =>
        {
            GameManager.Network.ReadyToLoad();

            if (PhotonNetwork.IsMasterClient) { StartCoroutine(GameStartCo()); }
        };

        fadeOutTween.Play();
    }

    private IEnumerator GameStartCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        SetRoles();

        PhotonNetwork.LoadLevel((int)EScene.INGAME);
    }

    #endregion Game Start

    #region Set Roles

    private void SetRoles()
    {
        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

        // Set random seed
        int randomSeed = (int)System.DateTime.UtcNow.ToFileTime();
        UnityEngine.Random.InitState(randomSeed);

        // Init the player list
        List<int> actorList = new();
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            actorList.Add(player.Key);
        }
        int totalNumPlayers = actorList.Count;
        
        // Check num of mafia players
        int numMafia = (int)roomSetting[CustomProperties.NUM_MAFIAS];
        if (numMafia > totalNumPlayers / 4)
        {
            numMafia = totalNumPlayers / 4;
        }
        roomSetting[CustomProperties.NUM_MAFIAS] = numMafia;

        // Select mafia players
        List<int> mafiaActorList = new();
        for (int count = 0; count < numMafia; count++)
        {
            int actorIndex = UnityEngine.Random.Range(0, actorList.Count - 1);
            mafiaActorList.Add(actorList[actorIndex]);
            actorList.RemoveAt(actorIndex);
        }

        // Set mafia players' role
        DistributeRoles(roomSetting, ERoleType.MAFIA, mafiaActorList);

        // Check num of neutral players
        int numNeutral = (int)roomSetting[CustomProperties.NUM_NEUTRALS];
        if (numNeutral + numMafia >= totalNumPlayers / 2)
        {
            numNeutral = (totalNumPlayers / 2) - numMafia - 1;
        }
        int numNeutralRoles = ((int[])roomSetting[CustomProperties.ESSENTIAL_NEUTRAL_ROLES]).Length + ((int[])roomSetting[CustomProperties.OPTIONAL_NEUTRAL_ROLES]).Length;
        if (numNeutral > numNeutralRoles)
        {
            numNeutral = numNeutralRoles;
        }
        if (numNeutral < 0) { numNeutral = 0; }
        roomSetting[CustomProperties.NUM_NEUTRALS] = numNeutral;

        // Select neutral players
        List<int> neutralActorList = new();
        for (int count = 0; count < numNeutral; count++)
        {
            int actorIndex = UnityEngine.Random.Range(0, actorList.Count - 1);
            neutralActorList.Add(actorList[actorIndex]);
            actorList.RemoveAt(actorIndex);
        }

        // Set neutral players' role
        DistributeRoles(roomSetting, ERoleType.NEUTRAL, neutralActorList);

        // Set citizen players' role
        DistributeRoles(roomSetting, ERoleType.CITIZEN, actorList);

        // Save room properties
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);
    }

    private void DistributeRoles(PhotonHashTable roomSetting, ERoleType playersTeam, List<int> players)
    {
        if (players.Count <= 0) { return; }

        // Get essential and optional roles
        int[] essentialRoles;
        int[] optionalRoles;

        switch (playersTeam)
        {
            case ERoleType.CITIZEN:
                essentialRoles = (int[])roomSetting[CustomProperties.ESSENTIAL_CITIZEN_ROLES];
                optionalRoles = (int[])roomSetting[CustomProperties.OPTIONAL_CITIZEN_ROLES];
                break;

            case ERoleType.MAFIA:
                essentialRoles = (int[])roomSetting[CustomProperties.ESSENTIAL_MAFIA_ROLES];
                optionalRoles = (int[])roomSetting[CustomProperties.OPTIONAL_MAFIA_ROLES];
                break;

            case ERoleType.NEUTRAL:
                essentialRoles = (int[])roomSetting[CustomProperties.ESSENTIAL_NEUTRAL_ROLES];
                optionalRoles = (int[])roomSetting[CustomProperties.OPTIONAL_NEUTRAL_ROLES];
                break;

            default:
                throw new System.Exception("ERoleType.NONE and ERoleType.NPC is currently not supported!!");
        }

        PhotonHashTable playerSetting;
        for (int index = 0; index < players.Count; index++)
        {
            playerSetting = PhotonNetwork.CurrentRoom.Players[players[index]].CustomProperties;

            // Set the player's team
            if (playerSetting.ContainsKey(PlayerProperties.PLAYER_TEAM))
            {
                playerSetting[PlayerProperties.PLAYER_TEAM] = (int)playersTeam;
            }
            else
            {
                playerSetting.Add(PlayerProperties.PLAYER_TEAM, (int)playersTeam);
            }

            // Select the player's role
            int playerRole = -1;
            if (essentialRoles.Length > 0)
            {
                int roleIndex = UnityEngine.Random.Range(0, essentialRoles.Length - 1);
                playerRole = essentialRoles[roleIndex];
                essentialRoles = ArrayHelper.RemoveAt(roleIndex, essentialRoles);
            }
            else if (optionalRoles.Length > 0)
            {
                int roleIndex = UnityEngine.Random.Range(0, optionalRoles.Length - 1);
                playerRole = optionalRoles[roleIndex];
                optionalRoles = ArrayHelper.RemoveAt(roleIndex, optionalRoles);
            }

            // Set the player's role
            if (playerSetting.ContainsKey(PlayerProperties.PLAYER_ROLE))
            {
                playerSetting[PlayerProperties.PLAYER_ROLE] = playerRole;
            }
            else
            {
                playerSetting.Add(PlayerProperties.PLAYER_ROLE, playerRole);
            }

            // Set the player's custom properties
            PhotonNetwork.CurrentRoom.Players[players[index]].SetCustomProperties(playerSetting);
        }
    }

    #endregion Set Roles

    #region Effect Methods

    private void EnterRoomEffect()
    {
        mainCamera.transform.DOMoveY(0f, 1f)
            .SetEase(Ease.OutExpo)
            .OnComplete(() => SpawnPlayerCharacter())
            .Play();
    }

    private void LeaveRoomEffect()
    {
        mainCamera.transform.DOMove(new Vector3(0f, 15f, mainCamera.transform.position.z), 1f)
                    .SetEase(Ease.OutExpo)
                    .Play();
    }

    #endregion Effect Methods

    #region Photon Events

    public override void OnJoinedRoom()
    {
        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;

        int[] remainColorList = (int[])roomSetting[CustomProperties.REMAIN_COLOR_LIST];
        int[] usedColorList = (int[])roomSetting[CustomProperties.USED_COLOR_LIST];
        int playerColor = remainColorList[UnityEngine.Random.Range(0, remainColorList.Length)];

        remainColorList = ArrayHelper.Remove(playerColor, remainColorList);
        usedColorList = ArrayHelper.Add(playerColor, usedColorList);

        roomSetting[CustomProperties.REMAIN_COLOR_LIST] = remainColorList;
        roomSetting[CustomProperties.USED_COLOR_LIST] = usedColorList;

        if (playerSetting.ContainsKey(PlayerProperties.PLYAER_COLOR))
        {
            playerSetting[PlayerProperties.PLYAER_COLOR] = playerColor;
        }
        else
        {
            playerSetting.Add(PlayerProperties.PLYAER_COLOR, playerColor);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);

        EnterRoomEffect();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (countDownCo != null)
        {
            StopCoroutine(countDownCo);
            countDownCo = null;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (countDownCo != null)
        {
            StopCoroutine(countDownCo);
            countDownCo = null;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

            int[] remainColorList = (int[])roomSetting[CustomProperties.REMAIN_COLOR_LIST];
            int[] usedColorList = (int[])roomSetting[CustomProperties.USED_COLOR_LIST];
            int leftPlayerColor = (int)otherPlayer.CustomProperties[PlayerProperties.PLYAER_COLOR];

            remainColorList = ArrayHelper.Add(leftPlayerColor, remainColorList);
            usedColorList = ArrayHelper.Remove(leftPlayerColor, usedColorList);

            roomSetting[CustomProperties.REMAIN_COLOR_LIST] = remainColorList;
            roomSetting[CustomProperties.USED_COLOR_LIST] = usedColorList;

            PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);
        }

        // Remove other player from the list
        CharacterObjectDicionary.Remove(otherPlayer.ActorNumber);
    }

    #endregion Photon Events
}