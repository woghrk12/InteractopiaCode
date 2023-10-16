using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;
using ExitGames.Client.Photon;

public class RoomPanel : UIPanel
{
    #region Variables

    [SerializeField] private PlayerStickInput joystick = null;
    [SerializeField] private PlayerButton useButton = null;

    [SerializeField] private Text roomCodeText = null;
    [SerializeField] private Text maxPlayersText = null;
    [SerializeField] private Text curPlayersText = null;
    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button textChattingButton = null;
    [SerializeField] private Button voiceChattingButton = null;
    [SerializeField] private Toggle roomVisibleToggle = null;
    [SerializeField] private Button startButton = null;

    [SerializeField] private GameObject voiceBlockImageObject = null;

    [SerializeField] private Text countDownText = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        GameManager.Input.PlayerStickInput = joystick;
        GameManager.Input.UseButton = useButton;

        settingButton.onClick.AddListener(OnClickSettingButton);
        voiceChattingButton.onClick.AddListener(OnClickVoiceChattingButton);
        textChattingButton.onClick.AddListener(OnClickTextChattingButton);
        roomVisibleToggle.onValueChanged.AddListener(OnRoomPrivateOrPublicToggleValueChanged);
        startButton.onClick.AddListener(OnClickStartButton);

        GameManager.Title.CountDownEvent += SetCountDownText;

        OnActive += () =>
        {
            if (!PhotonNetwork.InRoom) { throw new Exception("Photon network state is not InRoom!!"); }

            countDownText.gameObject.SetActive(false);

            // Only master client can change the privacy mode
            roomVisibleToggle.interactable = PhotonNetwork.IsMasterClient;

            // Only master client can start the game
            startButton.interactable = GameManager.Network.PlayerCount >= NetworkManager.MIN_NUM_PLAYERS;
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

            Room currentRoom = PhotonNetwork.CurrentRoom;

            roomVisibleToggle.isOn = !currentRoom.IsVisible;

            roomCodeText.text = $"CODE\n{currentRoom.Name}";
            curPlayersText.text = currentRoom.PlayerCount.ToString();
            maxPlayersText.text = currentRoom.MaxPlayers.ToString();

            // Check the player's input device
            joystick.gameObject.SetActive(GameManager.Input.InputMode == InputManager.EInputMode.GAMEPAD);

            if (GameManager.Vivox.IsMute)
            {
                voiceBlockImageObject.SetActive(true);
            }
            else
            {
                voiceBlockImageObject.SetActive(false);
                GameManager.Vivox.SetPosition();
            }
        };
        OnDeactive += () =>
        {
            // Stop the character
            GameManager.Input.StopMove();
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

    public void SetCountDownText(int countDown)
    {
        if (!countDownText.gameObject.activeSelf)
        {
            countDownText.gameObject.SetActive(true);
        }

        countDownText.text = $"Starting in {countDown} seconds.";
    }

    #endregion Methods

    #region Event Methods

    public void OnClickSettingButton() => GameManager.UI.PopupPanel<SettingPanel>();

    public void OnClickTextChattingButton()
    {
        if (!GameManager.Vivox.IsConnected) return;

        GameManager.UI.PopupPanel<TextChattingPanel>();
    }

    public void OnClickVoiceChattingButton()
    {
        if (GameManager.Vivox.IsMute)
        {
            GameManager.Vivox.SetLocalUnmute();
            GameManager.Vivox.SetPosition();
            voiceBlockImageObject.SetActive(false);
        }
        else
        {
            GameManager.Vivox.SetLocalMute();
            voiceBlockImageObject.SetActive(true);
        }
    }

    public void OnClickCharacterSettingButton() => GameManager.UI.PopupPanel<CharacterSettingPanel>();

    public void OnClickStartButton()
    {
        if (!GameManager.Title.CheckCanStartGame())
        {
            return;
        }

        startButton.interactable = false;

        GameManager.Title.GameStart();
    }
    
    public void OnCurPlayerNumChanged(int value) => curPlayersText.text = value.ToString();

    public void OnMaxPlayerNumChanged(int value) => maxPlayersText.text = value.ToString();

    public void OnRoomPrivateOrPublicToggleValueChanged(bool isOn) => PhotonNetwork.CurrentRoom.IsVisible = !isOn;

    #endregion Event Methods

    #region Photon Events

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        countDownText.gameObject.SetActive(false);
        startButton.interactable = true;

        int curNumPlayers = GameManager.Network.PlayerCount;

        curPlayersText.text = curNumPlayers.ToString();
        startButton.interactable = curNumPlayers >= NetworkManager.MIN_NUM_PLAYERS;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        countDownText.gameObject.SetActive(false);
        startButton.interactable = true;

        int curNumPlayers = GameManager.Network.PlayerCount;

        curPlayersText.text = curNumPlayers.ToString();
        startButton.interactable = curNumPlayers >= NetworkManager.MIN_NUM_PLAYERS;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomVisibleToggle.interactable = PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient) return;

        roomVisibleToggle.isOn = !PhotonNetwork.CurrentRoom.IsVisible;
    }

    #endregion Photon Events
}
