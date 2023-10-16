using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

public class PrivateJoinPanel : UIPanel
{
    #region Variables
    
    [SerializeField] private InputField roomCodeInputField = null;
    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button enterButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        enterButton.onClick.AddListener(OnClickEnterButton);

        OnActive += () =>
        {
            closeButton.interactable = true;
            enterButton.interactable = true;

            roomCodeInputField.text = string.Empty;
        };
    }

    public override Sequence ActiveAnimation()
    {
        Tween panelTween = DoTweenUtil.DoAnchoredPos(
            panelRect,
            new Vector2(0f, 1500f),
            Vector2.zero,
            GlobalDefine.panelAnimationDuration,
            Ease.OutExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    public override Sequence DeactiveAnimation()
    {
        Tween panelTween = DoTweenUtil.DoAnchoredPos(
            panelRect,
            Vector2.zero,
            new Vector2(0f, 1500f),
            GlobalDefine.panelAnimationDuration,
            Ease.InExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    #endregion Override Methods

    #region Event Methods

    public void OnClickCloseButton()
    {
        enterButton.interactable = false;
        closeButton.interactable = false;

        GameManager.UI.ClosePopupPanel<PrivateJoinPanel>();
    }

    public void OnClickEnterButton()
    {
        if (roomCodeInputField.text == string.Empty)
        {
            GameManager.UI.Alert("Please write the room code!");
            return;
        }

        enterButton.interactable = false;
        closeButton.interactable = false;

        PhotonNetwork.JoinRoom(roomCodeInputField.text);
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnJoinedRoom()
    {
        GameManager.UI.ClosePopupPanel<PrivateJoinPanel>();
        GameManager.UI.OpenPanel<RoomPanel>();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        enterButton.interactable = true;
        closeButton.interactable = true;

        roomCodeInputField.text = string.Empty;

        GameManager.UI.Alert("You cannot join the room!");
        PhotonNetwork.JoinLobby();
    }

    #endregion Photon Events
}
