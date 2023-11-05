using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager
{
    #region Constant Variables

    public const int MAX_NUM_PLAYERS = 12;
    public const int MIN_NUM_PLAYERS = 1;

    #endregion Constant Variables

    #region Variables

    private bool isInitialized = false;

    private List<RoomInfo> roomList = new List<RoomInfo>();

    public Action<List<RoomInfo>> RoomListAdded = null;
    public Action<List<RoomInfo>> RoomListRemoved = null;
    public Action<List<RoomInfo>> RoomListUpdated = null;

    [SerializeField] private PhotonObjectPool characterPrefabPool = null;

    private Dictionary<string, Player> playerDictionaryById = new();
    private Dictionary<int, Player> playerDictionaryByActorNum = new();

    #endregion Variables

    #region Properties

    public string NetworkStatus => PhotonNetwork.NetworkClientState.ToString();

    public List<RoomInfo> RoomList => roomList;

    public Dictionary<string, Player> PlayerDictionaryById => playerDictionaryById;
    public Dictionary<int, Player> PlayerDictionaryByActorNum => playerDictionaryByActorNum;
    public int PlayerCount => playerDictionaryByActorNum.Count;

    #endregion Properties

    #region Methods

    public void Init()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings();
    }

    public void Clear()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        PhotonNetwork.Disconnect();
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        var addedRoomList = new List<RoomInfo>();
        var removedRoomList = new List<RoomInfo>();
        var updatedRoomList = new List<RoomInfo>();

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                this.roomList.Remove(room);
                removedRoomList.Add(room);
                continue;
            }

            int idx = this.roomList.FindIndex(x => x.Name.Equals(room.Name));
            if (idx >= 0)
            {
                this.roomList[idx] = room;
                updatedRoomList.Add(room);
            }
            else
            {
                this.roomList.Add(room);
                addedRoomList.Add(room);
            }
        }

        RoomListAdded?.Invoke(addedRoomList);
        RoomListRemoved?.Invoke(removedRoomList);
        RoomListUpdated?.Invoke(updatedRoomList);
    }

    public void InitPlayerDictionary()
    {
        Dictionary<int, Player> playerDictionary = PhotonNetwork.CurrentRoom.Players;

        playerDictionaryById.Clear();
        playerDictionaryByActorNum.Clear();

        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            playerDictionaryById.Add(player.Value.UserId, player.Value);
            playerDictionaryByActorNum.Add(player.Key, player.Value);
        }
    }

    public void AddPlayer(Player newPlayer)
    {
        playerDictionaryById.Add(newPlayer.UserId, newPlayer);
        playerDictionaryByActorNum.Add(newPlayer.ActorNumber, newPlayer);
    }

    public void RemovePlayer(Player otherPlayer)
    {
        playerDictionaryById.Remove(otherPlayer.UserId);
        playerDictionaryByActorNum.Remove(otherPlayer.ActorNumber);
    }

    #endregion Methods

    #region Ready To Load

    public void InitReadyState()
    {
        foreach (KeyValuePair<int, Player> player in playerDictionaryByActorNum)
        {
            PhotonHashTable playerSetting = player.Value.CustomProperties;

            if (playerSetting.ContainsKey(PlayerProperties.READY_TO_LOAD))
            {
                playerSetting[PlayerProperties.READY_TO_LOAD] = false;
            }
            else
            {
                playerSetting.Add(PlayerProperties.READY_TO_LOAD, false);
            }

            player.Value.SetCustomProperties(playerSetting);
        }
    }

    public void ReadyToLoad()
    {
        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        playerSetting[PlayerProperties.READY_TO_LOAD] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);
    }

    private bool CheckAllPlayerReadyToLoad()
    {
        foreach (KeyValuePair<int, Player> player in playerDictionaryByActorNum)
        {
            if (!(bool)player.Value.CustomProperties[PlayerProperties.READY_TO_LOAD]) return false;
        }

        return true;
    }

    public IEnumerator CheckReadyCo()
    {
        while (!CheckAllPlayerReadyToLoad())
        {
            yield return Utilities.WaitForSeconds(0.5f);
        }

        foreach (KeyValuePair<int, Player> player in playerDictionaryByActorNum)
        {
            PhotonHashTable playerSetting = player.Value.CustomProperties;
            playerSetting[PlayerProperties.READY_TO_LOAD] = false;
            player.Value.SetCustomProperties(playerSetting);
        }
    }

    #endregion Ready To Load

    #region Event Methods

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        if (isInitialized) { return; }

        isInitialized = true;

        PhotonNetwork.LocalPlayer.NickName = "Test";

        characterPrefabPool = GameManager.Resource.Load<PhotonObjectPool>("Data/CharacterPrefabData");
        characterPrefabPool.InitPool();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.ApplicationQuit) return;

        PhotonNetwork.ConnectUsingSettings();
    }

    #endregion Photon Callbacks
}
