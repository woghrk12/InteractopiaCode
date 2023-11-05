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
    private List<int> aliveNonMafiaPlayerList = new();
    private List<int> aliveCitizenPlayerList = new();
    private List<int> aliveMafiaPlayerList = new();
    private List<int> deadPlayerList = new();
    private List<int> currentDeadPlayerList = new();
    private int collectorActorNumber = -1;

    [Header("Dictionary for npcs")]
    private Dictionary<ENPCRole, BaseNPC> npcList = new();
    private Dictionary<ENPCRole, BaseNPC> deadNPCList = new();
    private List<GameObject> deadNPCObjectList = new();

    [Header("Variables for missions")]
    private Dictionary<EMissionType, List<MissionObject>> missionObjectDictionary = new();
    private int curTransmitterMissionCount = 0;
    private int transmitterMissionCount = 0;

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

    public List<int> AliveNonMafiaPlayerList => aliveNonMafiaPlayerList;
    public List<int> AliveCitizenPlayerList => aliveCitizenPlayerList;
    public List<int> AliveMafiaPlayerList => aliveMafiaPlayerList;
    public List<int> DeadPlayerList => deadPlayerList;
    public List<int> CurrentDeadPlayerList => currentDeadPlayerList;

    public Dictionary<ENPCRole, BaseNPC> NPCList => npcList;
    public Dictionary<ENPCRole, BaseNPC> DeadNPCList => deadNPCList;

    public Dictionary<EMissionType, List<MissionObject>> MissionObjectDictionary => missionObjectDictionary;

    public ERoleType WinnerTeam => winnerTeam;
    public int WinnerActorNumber => winnerActorNumber;

    #endregion Properties

    #region Methods

    public void Init()
    {
        managerPhotonView = GetComponent<PhotonView>();

        spawnPositions = GameManager.Resource.Load<SpawnPositionData>("Data/InGameSpawnPositionData");
        randomSpawnPositions = GameManager.Resource.Load<SpawnPositionData>("Data/RandomSpawnPositionData");

        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

        // Init the player
        foreach (KeyValuePair<int, Player> player in GameManager.Network.PlayerDictionaryByActorNum)
        {
            ERoleType playerTeam = (ERoleType)player.Value.CustomProperties[PlayerProperties.PLAYER_TEAM];

            if (playerTeam == ERoleType.CITIZEN)
            {
                aliveNonMafiaPlayerList.Add(player.Key);
                citizenPlayerList.Add(player.Key);
                aliveCitizenPlayerList.Add(player.Key);
            }
            else if (playerTeam == ERoleType.MAFIA)
            {
                mafiaPlayerList.Add(player.Key);
                aliveMafiaPlayerList.Add(player.Key);
            }
            else
            {
                aliveNonMafiaPlayerList.Add(player.Key);

                ENeutralRole playerRole = (ENeutralRole)player.Value.CustomProperties[PlayerProperties.PLAYER_ROLE];
                if (playerRole == ENeutralRole.COLLECTOR)
                {
                    collectorActorNumber = player.Value.ActorNumber;
                }
            }
        }

        // Init the npc
        BaseNPC[] npcs = GameObject.FindObjectsOfType<BaseNPC>();
        foreach (BaseNPC npc in npcs)
        {
            npcList.Add(npc.NPCType, npc);
        }

        // Find the mission object
        MissionObject[] missionObjects = GameObject.FindObjectsOfType<MissionObject>();
        Array.Sort(missionObjects, (a, b) => { return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()); });
        Dictionary<EMissionType, List<MissionObject>> missionObjectDictionary = new();
        foreach (MissionObject missionObject in missionObjects)
        {
            if (missionObjectDictionary.ContainsKey(missionObject.MissionType))
            {
                missionObjectDictionary[missionObject.MissionType].Add(missionObject);
            }
            else
            {
                missionObjectDictionary.Add(missionObject.MissionType, new() { missionObject });
            }

            missionObject.gameObject.SetActive(false);
        }

        // Set the mission object by using the room setting
        int[] missionIndexList = (int[])roomSetting[CustomProperties.RANDOM_MISSION_INDEX];
        foreach (KeyValuePair<EMissionType, List<MissionObject>> missionList in missionObjectDictionary)
        {
            foreach (int index in missionIndexList)
            {
                missionList.Value[index].gameObject.SetActive(true);
                if (this.missionObjectDictionary.ContainsKey(missionList.Key))
                {
                    this.missionObjectDictionary[missionList.Key].Add(missionList.Value[index]);
                }
                else
                {
                    this.MissionObjectDictionary.Add(missionList.Key, new() { missionList.Value[index] });
                }
                
            }
        }

        // Init the count of the transmitter mission
        ENumMission numTranmitterMission = (ENumMission)roomSetting[CustomProperties.NUM_SPECIAL_MISSION];
        switch (numTranmitterMission)
        {
            case ENumMission.LITTLE:
                transmitterMissionCount = 3 * PhotonNetwork.CurrentRoom.PlayerCount;
                break;

            case ENumMission.MIDDLE:
                transmitterMissionCount = 5 * PhotonNetwork.CurrentRoom.PlayerCount;
                break;

            case ENumMission.MANY:
                transmitterMissionCount = 7 * PhotonNetwork.CurrentRoom.PlayerCount;
                break;

            case ENumMission.VERYMANY:
                transmitterMissionCount = 9 * PhotonNetwork.CurrentRoom.PlayerCount;
                break;
        }

        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        RoleData roleData = GameManager.Resource.Load<RoleData>("Data/RoleData");
        ERoleType team = (ERoleType)playerSetting[PlayerProperties.PLAYER_TEAM];
        int role = (int)playerSetting[PlayerProperties.PLAYER_ROLE];

        localPlayer = Instantiate(roleData.GetRoleInfo(team, role).PlayerObject).GetComponent<InGamePlayer>();
    }

    public void ClearTransmitterMission(int addCount)
    {
        photonView.RPC(nameof(ClearTransmitterMissionRPC), RpcTarget.AllViaServer, addCount);
    }

    [PunRPC]
    public void ClearTransmitterMissionRPC(int addCount)
    {
        curTransmitterMissionCount += addCount;

        GameManager.UI.GetPanel<InGamePanel>().SetMissionProgress(curTransmitterMissionCount, transmitterMissionCount);

        if (CheckGameEndByTransmitter())
        {
            EndGame();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        GameManager.Vivox.SetLocalMute();
        GameManager.Vivox.LeaveChannel();
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
        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        if ((bool)roomSetting[CustomProperties.SHORT_DISTANCE_VOICE])
        {
            GameManager.Vivox.ReleaseVoice();
            GameManager.InGame.LocalPlayer.IsSetPosition = true;
        }
        else
        {
            GameManager.Vivox.BlockVoice();
        }

        // Open the InGame Panel
        GameManager.UI.OpenPanel<InGamePanel>();

        // Start the cooldown of player
        GameManager.InGame.LocalPlayer.StartCooldown();

        // Start the cooldown of mission
        foreach (KeyValuePair<EMissionType, List<MissionObject>> missionList in missionObjectDictionary)
        {
            foreach (MissionObject missionObject in missionList.Value)
            {
                missionObject.StartCooldown();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Start the cooldown of emergency meeting
            GameManager.Meeting.StartEmergencyCooldown();
        }

        // Start the cooldown of npcs
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        {
            npc.Value.EnterWork();
        }
    }

    #endregion Start Work

    #region Game Ending

    /// <summary>
    /// Check whether the game is end because of the transmitter mission
    /// </summary>
    /// <returns>whether the transmitter mission count is greater or equal to the goal</returns>
    public bool CheckGameEndByTransmitter()
    {
        if (transmitterMissionCount <= curTransmitterMissionCount)
        {
            winnerTeam = ERoleType.CITIZEN;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check whether the game is end because of the voting
    /// </summary>
    /// <returns>whether the kicked player's role is the Rogue of the neutral team</returns>
    public bool CheckGameEndByVote()
    {
        int actorNumber = GameManager.Meeting.KickedPlayerActorNum;
        
        if (actorNumber < 0) return false;

        Player kickedPlayer = GameManager.Network.PlayerDictionaryByActorNum[actorNumber];
        if ((ERoleType)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM] == ERoleType.NEUTRAL
            && (int)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_ROLE] == (int)ENeutralRole.ROGUE)
        {
            winnerTeam = ERoleType.NEUTRAL;
            winnerActorNumber = kickedPlayer.ActorNumber;
            GameManager.Meeting.KickedPlayerActorNum = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check whether the game is end because the number of mafias is 
    /// greater or equals to the number of citizens and neutrals
    /// </summary>
    /// <returns>the number of mafias >= the number of citizens and neutrals </returns>
    public bool CheckGameEndByNumber()
    {
        if (aliveNonMafiaPlayerList.Count <= aliveMafiaPlayerList.Count)
        {
            winnerTeam = ERoleType.MAFIA;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check whether the game is end because all maifas are dead
    /// </summary>
    /// <returns>the number of mafias <= 0</returns>
    public bool CheckGameEndByAllMafiaDead()
    {
        if (aliveMafiaPlayerList.Count <= 0)
        {
            winnerTeam = ERoleType.CITIZEN;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>the number of dead npcs >= the number of npcs</returns>
    public bool CheckGameEndByAllNPCDead()
    {
        if (deadNPCList.Count >= npcList.Count && collectorActorNumber >= 0)
        {
            Player collectorPlayer = GameManager.Network.PlayerDictionaryByActorNum[collectorActorNumber];
            if ((bool)collectorPlayer.CustomProperties[PlayerProperties.IS_DIE])
            {
                return true;
            }
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

    public override void OnLeftRoom()
    {
        GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration)
            .OnComplete(() => UnityEngine.SceneManagement.SceneManager.LoadScene((int)EScene.TITLE))
            .Play();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Remove other player's color
            PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

            int[] remainColorList = (int[])roomSetting[CustomProperties.REMAIN_COLOR_LIST];
            int[] usedColorList = (int[])roomSetting[CustomProperties.USED_COLOR_LIST];
            int leftPlayerColor = (int)otherPlayer.CustomProperties[PlayerProperties.PLAYER_COLOR];

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

        if (aliveNonMafiaPlayerList.Contains(otherPlayer.ActorNumber))
        {
            aliveNonMafiaPlayerList.Remove(otherPlayer.ActorNumber);
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

        if (collectorActorNumber >= 0 && collectorActorNumber == otherPlayer.ActorNumber)
        {
            collectorActorNumber = -1;
        }

        // Release npcs
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        {
            npc.Value.EndInteract(otherPlayer.ActorNumber);
        }

        if (CheckGameEndByAllMafiaDead())
        {
            EndGame();
        }
        else if (CheckGameEndByNumber())
        {
            EndGame();
        }
    }

    #endregion Photon Events
}
