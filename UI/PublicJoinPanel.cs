using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

public class PublicJoinPanel : UIPanel
{
    #region Variables

    private static readonly int MAX_ROOM_LIST = 30;

    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button cancelButton = null;

    private RoomInfo selectedRoomInfo = null;

    [SerializeField] private GameObject roomObjectDictionaryParent = null;
    [SerializeField] private GameObject roomPrefab = null;

    private Dictionary<string, GameObject> roomObjectDictionary = new();

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        joinButton.onClick.AddListener(OnClickJoinButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);

        OnActive += (() =>
        {
            joinButton.interactable = true;
            cancelButton.interactable = true;

            var networkManager = GameManager.Network;

            networkManager.RoomListAdded += AddRoomListObject;
            networkManager.RoomListRemoved += RemoveRoomlistObject;
            networkManager.RoomListUpdated += UpdateRoomListObject;

            selectedRoomInfo = null;

            AddRoomListObject(networkManager.RoomList);
        });
        OnDeactive += (() =>
        {
            var networkManager = GameManager.Network;

            networkManager.RoomListAdded -= AddRoomListObject;
            networkManager.RoomListRemoved -= RemoveRoomlistObject;
            networkManager.RoomListUpdated -= UpdateRoomListObject;

            foreach (KeyValuePair<string, GameObject> roomObj in roomObjectDictionary) 
            {
                Destroy(roomObj.Value); 
            }
            roomObjectDictionary.Clear();
        });
    }

    public override Sequence ActiveAnimation()
    {
        return DOTween.Sequence()
            .Append(GameManager.UI.FadeIn(GlobalDefine.fadeEffectDuration));
    }

    public override Sequence DeactiveAnimation()
    {
        return DOTween.Sequence()
            .Append(GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration));
    }

    #endregion Override Methods

    #region Methods

    private void AddRoomListObject(List<RoomInfo> addedList)
    {
        foreach (RoomInfo room in addedList)
        {
            if (roomObjectDictionary.Count > MAX_ROOM_LIST) break;

            var roomInstance = Instantiate(roomPrefab, roomObjectDictionaryParent.transform).GetComponent<RoomItemBtn>();
            roomInstance.SetRoomItem(room);
            roomInstance.SelectBtn.onClick.AddListener(() => OnClickRoomItem(room));
            roomObjectDictionary.Add(room.Name, roomInstance.gameObject);
        }
    }

    private void RemoveRoomlistObject(List<RoomInfo> removedList)
    {
        foreach (RoomInfo room in removedList)
        {
            if (roomObjectDictionary.TryGetValue(room.Name, out GameObject roomObj))
            {
                roomObjectDictionary.Remove(room.Name);
                Destroy(roomObj);
            }
        }
    }

    private void UpdateRoomListObject(List<RoomInfo> updatedList)
    {
        foreach (RoomInfo room in updatedList)
        {
            if (roomObjectDictionary.TryGetValue(room.Name, out GameObject roomObj))
            {
                roomObj.GetComponent<RoomItemBtn>().SetRoomItem(room);
            }
        }
    }

    #endregion Methods

    #region Event Methods

    public void OnClickJoinButton()
    {
        if (selectedRoomInfo == null)
        {
            GameManager.UI.Alert("Select the room item.");
            return;
        }
        
        joinButton.interactable = false;
        cancelButton.interactable = false;

        PhotonNetwork.JoinRoom(selectedRoomInfo.Name);
    }

    public void OnClickCancelButton() => GameManager.UI.OpenPanel<LobbyPanel>();

    public void OnClickRoomItem(RoomInfo roomInfo) { selectedRoomInfo = roomInfo; }

    #endregion Event Methods

    #region Photon Events

    public override void OnJoinedRoom() => GameManager.UI.OpenPanel<RoomPanel>();

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        joinButton.interactable = true;
        cancelButton.interactable = true;

        GameManager.UI.Alert("You cannot join the room!");
        PhotonNetwork.JoinLobby();
    }

    #endregion Photon Events
}
