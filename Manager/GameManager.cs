using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

public class GameManager : SingletonMonobehaviourPunCallback<GameManager>
{
    #region Variables

    private static bool isInitialized = false;

    [Header("Core Manager")]
    private NetworkManager networkManager = new();
    private PoolManager poolManager = new();
    private ResourceManager resourceManager = new();
    private InputManager inputManager = new();
    private UIManager uiManager = new();
    private VivoxManager vivoxManager = new();

    [Header("Content Manager")]
    private TitleManager titleManager = null;
    private InGameManager inGameManager = null;
    private MeetingManager meetingManager = null;

    [Header("Components for checking server status")]
    [SerializeField] private GameObject loadingPanel = null;
    [SerializeField] private Text networkStatusText = null;
    [SerializeField] private Text loginStatusText = null;
    [SerializeField] private Text channelStatusText = null;
    [SerializeField] private Text vivoxMuteStatusText = null;
    [SerializeField] private Text localMuteStatusText = null;

    [Header("Components for notification message")]
    [SerializeField] private Text notificationText = null;
    private Sequence notificationSequence = null;

    #endregion Variables

    #region Properties

    public static bool IsInitialized => isInitialized;

    public static NetworkManager Network => Instance.networkManager;
    public static PoolManager Pool => Instance.poolManager;
    public static ResourceManager Resource => Instance.resourceManager;
    public static InputManager Input => Instance.inputManager;
    public static UIManager UI => Instance.uiManager;
    public static VivoxManager Vivox => Instance.vivoxManager;

    public static TitleManager Title => Instance.titleManager;
    public static InGameManager InGame => Instance.inGameManager;
    public static MeetingManager Meeting => Instance.meetingManager;

    #endregion Properties

    #region Unity Events

    private void Start()
    {
        networkManager.Init();
        vivoxManager.Init();
        inputManager.Init();
        uiManager.Init();
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        titleManager = GameObject.FindObjectOfType<TitleManager>();
        titleManager.Init();

        uiManager.PremakeUIForTitle();

        inputManager.PlayerInputDevice = GetComponent<PlayerInput>();
        inputManager.PlayerArrowInput = GetComponent<PlayerArrowInput>();
        inputManager.PlayerScreenInput = GetComponent<PlayerScreenInput>();
    }

    private void Update()
    {
        networkStatusText.text = networkManager.NetworkStatus;
        loginStatusText.text = vivoxManager.LoginStatus;
        channelStatusText.text = vivoxManager.ChannelStatus;
        vivoxMuteStatusText.text = vivoxManager.IsVivoxMute.ToString();
        localMuteStatusText.text = vivoxManager.IsMute.ToString();
    }

    private void OnApplicationQuit()
    {
        vivoxManager.Clear();
        networkManager.Clear();
    }

    #endregion Unity Events

    #region Static Methods

    public static void SetLoadingPanel(bool isActive)
    {
        Instance.loadingPanel.SetActive(isActive);
    }

    #endregion Static Methods

    #region Event Methods

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == (int)EScene.TITLE)
        {
            titleManager = GameObject.FindObjectOfType<TitleManager>();
            titleManager.Init();

            uiManager.PremakeUIForTitle();

            titleManager.StartScene();
        }
        else if (scene.buildIndex == (int)EScene.INGAME)
        {
            if (PhotonNetwork.IsMasterClient)
            { 
                var playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
                playerSetting[PlayerProperties.PLAYER_TEAM] = ERoleType.MAFIA;
                playerSetting[PlayerProperties.PLAYER_ROLE] = EMafiaRole.ASSASSIN;
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);
            }


            inGameManager = GameObject.FindObjectOfType<InGameManager>();
            inGameManager.Init();
            meetingManager = GameObject.FindObjectOfType<MeetingManager>();
            meetingManager.Init();

            uiManager.PremakeUIForInGame();

            inGameManager.StartScene();
        }
    }

    #endregion Event Methods

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        networkManager.OnConnectedToMaster();
        vivoxManager.Login(PhotonNetwork.LocalPlayer.UserId);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.ApplicationQuit) return;

        SetLoadingPanel(true);
        networkManager.OnDisconnected(cause);
    }

    public override void OnJoinedRoom()
    {
        networkManager.InitPlayerDictionary();
        vivoxManager.JoinChannel();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        networkManager.UpdateRoomList(roomList);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        networkManager.AddPlayer(newPlayer);

        ShowNotificationMessage($"{newPlayer.NickName} has joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        networkManager.RemovePlayer(otherPlayer);

        ShowNotificationMessage($"{otherPlayer.NickName} has left the room.");
    }

    #endregion Photon Callbacks

    #region Helper Methods

    private void ShowNotificationMessage(string message)
    {
        if (notificationSequence != null)
        {
            notificationSequence.Kill();
            notificationSequence = null;
        }

        notificationText.gameObject.SetActive(true);

        Color color = notificationText.color;
        color.a = 1;
        notificationText.color = color;

        notificationText.text = message;

        Tween textTween = notificationText.DOFade(0f, 1f)
            .SetEase(Ease.OutExpo)
            .OnComplete(() =>
            {
                notificationText.gameObject.SetActive(false);
            });

        notificationSequence = DOTween.Sequence()
            .SetDelay(2f)
            .Append(textTween)
            .Play();
    }

    #endregion Helper Methods
}
