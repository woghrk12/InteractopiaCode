using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class InGameManager : MonoBehaviourPunCallbacks
{
    #region Variables

    private PhotonView managerPhotonView = null;

    private InGamePlayer localPlayer = null;

    [Header("Dictionary for character objects")]
    private Dictionary<int, InGameCharacter> characterObjectDictionary = new();
    private Dictionary<int, DeadCharacter> deadCharacterObjectDictionary = new();

    [Header("List for players")]
    private List<int> citizenPlayerList = new();
    private List<int> mafiaPlayerList = new();

    [Header("Dictionary for npcs")]
    [SerializeField] private List<GameObject> npcObjectList = new();
    private Dictionary<ENPCRole, BaseNPC> npcList = new();
    private Dictionary<ENPCRole, BaseNPC> deadNPCList = new();
    private List<GameObject> deadNPCObjectList = new();

    private List<int> aliveCitizenPlayerList = new();
    private List<int> aliveMafiaPlayerList = new();
    private List<int> deadPlayerList = new();
    private List<int> currentDeadPlayerList = new();

    private SpawnPositionData spawnPositions = null;
    private SpawnPositionData randomSpawnPositions = null;

    private ERoleType winnerTeam = ERoleType.NONE;
    private int winnerActorNumber = -1;

    #endregion Variables

    #region Properties

    public InGamePlayer LocalPlayer => localPlayer;

    public Dictionary<int, InGameCharacter> CharacterObjectDictionary => characterObjectDictionary;
    public Dictionary<int, DeadCharacter> DeadCharacterObjectDictionary => deadCharacterObjectDictionary;

    public List<int> CitizenPlayerList => citizenPlayerList;
    public List<int> MafiaPlayerList => mafiaPlayerList;

    public List<int> AliveCitizenPlayerList => aliveCitizenPlayerList;
    public List<int> AliveMafiaPlayerList => aliveMafiaPlayerList;
    public List<int> DeadPlayerList => deadPlayerList;
    public List<int> CurrentDeadPlayerList => currentDeadPlayerList;

    public ERoleType WinnerTeam => winnerTeam;
    public int WinnerActorNumber => winnerActorNumber;

    public Dictionary<ENPCRole, BaseNPC> NPCList => npcList;
    public Dictionary<ENPCRole, BaseNPC> DeadNPCList => deadNPCList;

    #endregion Properties

    #region Methods

    public void Init()
    {
        managerPhotonView = GetComponent<PhotonView>();

        spawnPositions = GameManager.Resource.Load<SpawnPositionData>("Data/InGameSpawnPositionData");
        randomSpawnPositions = GameManager.Resource.Load<SpawnPositionData>("Data/RandomSpawnPositionData");

        foreach (KeyValuePair<int, Player> player in GameManager.Network.PlayerDictionaryByActorNum)
        {
            ERoleType playerTeam = (ERoleType)player.Value.CustomProperties[PlayerProperties.PLAYER_TEAM];

            if (playerTeam != ERoleType.MAFIA)
            {
                citizenPlayerList.Add(player.Key);
                aliveCitizenPlayerList.Add(player.Key);
            }
            else
            {
                mafiaPlayerList.Add(player.Key);
                aliveMafiaPlayerList.Add(player.Key);
            }
        }

        int[] npcList = (int[])PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.NPC_ROLES];
        foreach (int npcIndex in npcList)
        {
            BaseNPC npc = npcObjectList[npcIndex].GetComponent<BaseNPC>();
            this.npcList.Add((ENPCRole)npcIndex, npc);
            npc.gameObject.SetActive(true);
        }

        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        RoleData roleData = GameManager.Resource.Load<RoleData>("Data/RoleData");
        ERoleType team = (ERoleType)playerSetting[PlayerProperties.PLAYER_TEAM];
        int role = (int)playerSetting[PlayerProperties.PLAYER_ROLE];

        localPlayer = Instantiate(roleData.GetRoleInfo(team, role).PlayerObject).GetComponent<InGamePlayer>();
    }

    #endregion Methods

    #region Start Scene

    public void StartScene()
    {
        // Spawn character objects
        InGameCharacter character = PhotonNetwork.Instantiate(
            CharacterPrefabName.inGameCharacterPrefabName,
            Vector2.zero,
            Quaternion.identity
            ).GetComponent<InGameCharacter>();

        localPlayer.InitPlayer(character);

        // Spawn dead character objects
        PhotonNetwork.Instantiate(
            CharacterPrefabName.deadCharacterPrefabName,
            Vector2.zero,
            Quaternion.identity
            );

        GameManager.Network.ReadyToLoad();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartSceneCo());
        }
    }

    private IEnumerator StartSceneCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        managerPhotonView.RPC(nameof(StartSceneRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void StartSceneRPC()
    {
        GameManager.UI.OpenPanel<GameStartPanel>();
    }

    #endregion Start Scene

    #region Start Work

    public void StartWork()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SetPositionIndex();

        managerPhotonView.RPC(nameof(SetPositionRPC), RpcTarget.AllViaServer);
    }

    private void SetPositionIndex()
    {
        // Initialize the list used for shuffling the spawn positions
        List<int> spawnIndices = new();
        for (int index = 0; index < spawnPositions.GetLength(); index++)
        {
            spawnIndices.Add(index);
        }

        // Set the spawn index to each player's customproperty
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PhotonHashTable playerSetting = player.Value.CustomProperties;
            int index = UnityEngine.Random.Range(0, spawnIndices.Count);

            if (playerSetting.ContainsKey(PlayerProperties.SPAWN_POSITION_INDEX))
            {
                playerSetting[PlayerProperties.SPAWN_POSITION_INDEX] = index;
            }
            else
            {
                playerSetting.Add(PlayerProperties.SPAWN_POSITION_INDEX, index);
            }

            spawnIndices.Remove(index);
            player.Value.SetCustomProperties(playerSetting);
        }
    }

    [PunRPC]
    private void SetPositionRPC()
    {
        // Deactive previously created dead character objects
        foreach (int currentDeadPlayer in currentDeadPlayerList)
        {
            deadCharacterObjectDictionary[currentDeadPlayer].gameObject.SetActive(false);
        }
        currentDeadPlayerList.Clear();

        // Deactive previously created dead npc objects
        foreach (GameObject deadNPCObject in deadNPCObjectList)
        {
            Destroy(deadNPCObject);
        }
        deadNPCObjectList.Clear();

        // Set the local character's position
        int spawnIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.SPAWN_POSITION_INDEX];
        bool isRandomStart = (bool)PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.RANDOM_START_POINT];
        GameManager.InGame.LocalPlayer.LocalCharacter.transform.position = isRandomStart ? randomSpawnPositions.GetPosition(spawnIndex) : spawnPositions.GetPosition(spawnIndex);

        GameManager.Network.ReadyToLoad();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartWorkCo());
        }
    }

    private IEnumerator StartWorkCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        managerPhotonView.RPC(nameof(StartWorkRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void StartWorkRPC()
    {
        // Release the vivox input device
        GameManager.Vivox.ReleaseVoice();
        GameManager.InGame.LocalPlayer.IsSetPosition = true;

        // Open the InGame Panel
        GameManager.UI.OpenPanel<InGamePanel>();

        // Start the cooldown of player
        GameManager.InGame.LocalPlayer.StartCooldown();

        // Start the cooldown of npcs
        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach(KeyValuePair<ENPCRole, BaseNPC> npc in NPCList)
        {
            npc.Value.EnterWork();
        }
    }

    #endregion Start Work

    #region Game Ending

    public bool CheckGameEnd()
    {
        // Neutral rogue player win
        if (GameManager.Meeting.KickedPlayerActorNum >= 0)
        {
            // Neutral rogue player win
            Player kickedPlayer = GameManager.Network.PlayerDictionaryByActorNum[GameManager.Meeting.KickedPlayerActorNum];
            if ((ERoleType)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM] == ERoleType.NEUTRAL
                && (int)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_ROLE] == (int)ENeutralRole.ROGUE)
            {
                winnerTeam = ERoleType.NEUTRAL;
                winnerActorNumber = kickedPlayer.ActorNumber;
                GameManager.Meeting.KickedPlayerActorNum = -1;
                return true;
            }
        }

        // Citizen win; All mafia is dead
        if (aliveMafiaPlayerList.Count == 0)
        {
            winnerTeam = ERoleType.CITIZEN;
            return true;
        }

        // Mafia win
        if (aliveCitizenPlayerList.Count <= aliveMafiaPlayerList.Count)
        {
            winnerTeam = ERoleType.MAFIA;
            return true;
        }

        return false;
    }

    public void EndGame()
    {
        if (GameManager.InGame.LocalPlayer.CurTask != null)
        {
            GameManager.InGame.LocalPlayer.CurTask.CloseTaskPanel();
        }

        GameManager.Input.StopMove();

        GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration)
            .OnComplete(() =>
                {
                    GameManager.UI.CloseAllPanel(false);
                    GameManager.Network.ReadyToLoad();

                    if (PhotonNetwork.IsMasterClient)
                    {
                        StartCoroutine(EndGameCo());
                    }
                });
    }

    private IEnumerator EndGameCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        managerPhotonView.RPC(nameof(EndGameRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void EndGameRPC()
    {
        GameManager.InGame.LocalPlayer.IsSetPosition = false;

        GameManager.UI.OpenPanel<EndingPanel>();
    }

    public void ReturnToRoom()
    {
        DOTween.KillAll();
        GameManager.Vivox.BlockVoice();
        GameManager.Network.ReadyToLoad();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ReturnToRoomCo());
        }
    }

    private IEnumerator ReturnToRoomCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        PhotonNetwork.LoadLevel((int)EScene.TITLE);
    }

    #endregion Game Ending

    #region Photon Events

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Remove other player's color
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

        // Remove other player from the lists
        characterObjectDictionary.Remove(otherPlayer.ActorNumber);
        deadCharacterObjectDictionary.Remove(otherPlayer.ActorNumber);

        if (citizenPlayerList.Contains(otherPlayer.ActorNumber))
        {
            citizenPlayerList.Remove(otherPlayer.ActorNumber);
        }
        else if (mafiaPlayerList.Contains(otherPlayer.ActorNumber))
        {
            mafiaPlayerList.Remove(otherPlayer.ActorNumber);
        }

        if (aliveCitizenPlayerList.Contains(otherPlayer.ActorNumber))
        {
            aliveCitizenPlayerList.Remove(otherPlayer.ActorNumber);
        }
        else if (aliveMafiaPlayerList.Contains(otherPlayer.ActorNumber))
        {
            aliveMafiaPlayerList.Remove(otherPlayer.ActorNumber);
        }
        else if (deadPlayerList.Contains(otherPlayer.ActorNumber))
        {
            deadPlayerList.Remove(otherPlayer.ActorNumber);

            if (currentDeadPlayerList.Contains(otherPlayer.ActorNumber))
            {
                currentDeadPlayerList.Remove(otherPlayer.ActorNumber);
            }
        }

        // Release npcs
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        {
            npc.Value.EndInteract(otherPlayer.ActorNumber);
        }

        if (CheckGameEnd())
        {
            EndGame();
        }
    }

    #endregion Photon Events
}
