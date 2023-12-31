using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CreateRoomPanel : UIPanel
{
    #region Variables

    [Range(NetworkManager.MIN_NUM_PLAYERS, NetworkManager.MAX_NUM_PLAYERS)] private int maxPlayer = 8;

    [SerializeField] private Button createButton = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private Slider maxPlayerSlider = null;
    [SerializeField] private Text maxPlayerText = null;
    [SerializeField] private Toggle privacyModeToggle = null;

    [SerializeField] private MapList mapList = null;

    private PhotonHashTable roomSetting = null;
    private string[] roomSettingForLobby = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        createButton.onClick.AddListener(OnClickCreateButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);

        maxPlayerSlider.value = maxPlayer;
        maxPlayerSlider.onValueChanged.AddListener(OnMaxPlayerChanged);

        privacyModeToggle.isOn = false;

        roomSetting = new();

        // Custom Room Properties
        roomSetting.Add(CustomProperties.ROOM_NAME, string.Empty);
        roomSetting.Add(CustomProperties.MAP, 0);
        roomSetting.Add(CustomProperties.SPAWN_POSITION, 0);
        roomSetting.Add(CustomProperties.IS_VISIBLE, false);

        // Common group setting
        roomSetting.Add(CustomProperties.SHORT_DISTANCE_VOICE, true);
        roomSetting.Add(CustomProperties.RANDOM_START_POINT, false);
        roomSetting.Add(CustomProperties.MAX_MAFIAS, 0);
        roomSetting.Add(CustomProperties.NUM_MAFIAS, 0);
        roomSetting.Add(CustomProperties.MAX_NEUTRALS, 0);
        roomSetting.Add(CustomProperties.NUM_NEUTRALS, 0);
        roomSetting.Add(CustomProperties.HIDE_EMISSION_INFO, false);
        roomSetting.Add(CustomProperties.BLIND_MAFIA_MODE, false);

        // Meeting and vote group setting
        roomSetting.Add(CustomProperties.MEETING_TIME, 30);
        roomSetting.Add(CustomProperties.VOTE_TIME, 60);
        roomSetting.Add(CustomProperties.OPEN_VOTE, (int)EOpenVote.OPEN);
        roomSetting.Add(CustomProperties.EMERGENCY_MEETING_COOLDOWN, 20);

        // Roles and NPC group setting
        roomSetting.Add(CustomProperties.ESSENTIAL_CITIZEN_ROLES, new int[0]);
        roomSetting.Add(CustomProperties.OPTIONAL_CITIZEN_ROLES, new int[0]);
        roomSetting.Add(CustomProperties.ESSENTIAL_MAFIA_ROLES, new int[0]);
        roomSetting.Add(CustomProperties.OPTIONAL_MAFIA_ROLES, new int[0]);
        roomSetting.Add(CustomProperties.ESSENTIAL_NEUTRAL_ROLES, new int[0]);
        roomSetting.Add(CustomProperties.OPTIONAL_NEUTRAL_ROLES, new int[0]);

        // Movement and sight group setting
        roomSetting.Add(CustomProperties.SIGHT_RANGE, (int)ESightRange.MIDDLE);
        roomSetting.Add(CustomProperties.MOVE_SPEED, (int)EMoveSpeed.MIDDLE);

        // Cooltime and mission group setting
        roomSetting.Add(CustomProperties.KILL_COOLDOWN, 25);
        roomSetting.Add(CustomProperties.MISSION_COOLDOWN, 40);
        roomSetting.Add(CustomProperties.NUM_NPC_MISSION, (int)ENumMission.MIDDLE);
        roomSetting.Add(CustomProperties.NUM_SPECIAL_MISSION, (int)ENumMission.MIDDLE);

        // Character colors
        List<int> colorList = new();
        for (int index = 0; index < (int)ECharacterColor.END; index++)
        {
            colorList.Add(index);
        }
        roomSetting.Add(CustomProperties.REMAIN_COLOR_LIST, ArrayHelper.ListToArray(colorList));
        roomSetting.Add(CustomProperties.USED_COLOR_LIST, new int[0]);

        // Custom Room Properties for Lobby
        roomSettingForLobby = new string[0];
        roomSettingForLobby = ArrayHelper.Add(CustomProperties.ROOM_NAME, roomSettingForLobby);
        roomSettingForLobby = ArrayHelper.Add(CustomProperties.MAP, roomSettingForLobby);
        roomSettingForLobby = ArrayHelper.Add(CustomProperties.MAX_MAFIAS, roomSettingForLobby);

        OnActive += () =>
        {
            createButton.interactable = true;
            maxPlayer = 8;
            maxPlayerSlider.value = 8;
            maxPlayerText.text = maxPlayer.ToString();
            privacyModeToggle.isOn = false;
            mapList.InitList();
        };
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

    private void CreateRoom()
    {
        // Set the room name
        roomSetting[CustomProperties.ROOM_NAME] = PhotonNetwork.LocalPlayer.NickName + "'s room";

        // Set num of mafias
        roomSetting[CustomProperties.MAX_MAFIAS] = maxPlayer / 4;
        roomSetting[CustomProperties.NUM_MAFIAS] = maxPlayer / 4;

        // Set num of neutrals
        roomSetting[CustomProperties.MAX_NEUTRALS] = maxPlayer / 4;
        roomSetting[CustomProperties.NUM_NEUTRALS] = maxPlayer / 5;

        // Set a selected map
        roomSetting[CustomProperties.MAP] = mapList.SelectedIdx;

        // Set visible
        roomSetting[CustomProperties.IS_VISIBLE] = !privacyModeToggle.isOn;

        string roomName = Utilities.ComputeMD5(PhotonNetwork.LocalPlayer.UserId + "_" + System.DateTime.UtcNow.ToFileTime().ToString(), 3);
        RoomOptions roomOption = new RoomOptions
        {
            MaxPlayers = maxPlayer,
            IsVisible = !privacyModeToggle.isOn,
            IsOpen = true,
            CustomRoomProperties = roomSetting,
            CustomRoomPropertiesForLobby = roomSettingForLobby,
            PublishUserId = true
        };

        PhotonNetwork.CreateRoom(roomName, roomOption);
    }

    #endregion Methods

    #region Event Methods

    public void OnClickCreateButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        createButton.interactable = false;
        CreateRoom();
    }

    public void OnClickCancelButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.UI.OpenPanel<LobbyPanel>();
    }

    public void OnMaxPlayerChanged(float value) 
    {
        maxPlayer = (int)value;
        maxPlayerText.text = maxPlayer.ToString(); 
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnCreatedRoom() => GameManager.UI.OpenPanel<RoomPanel>();

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        createButton.interactable = true;

        switch (returnCode)
        {
            case ErrorCode.GameIdAlreadyExists:
                {
                    CreateRoom();
                    break;
                }
        }
    }

    #endregion Photon Events
}
