using System;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

using DoTween = DG.Tweening.Tween;

public class TextChattingPanel : UIPanel, IEnhancedScrollerDelegate
{
    #region Variables

    [SerializeField] private Button closeButton = null;
    [SerializeField] private RectTransform panelRect = null;
    
    [SerializeField] private InputField messageInputField = null;
    [SerializeField] private Button sendButton = null;

    private readonly SmallList<TextMessageData> textMessageDatas = new();

    [SerializeField] private EnhancedScroller scroller = null;

    [SerializeField] private TextMessageCellView myTextMessagePrefab = null;
    [SerializeField] private TextMessageCellView otherTextMessagePrefab = null;
    [SerializeField] private TextMessageCellView notificationTextMessagePrefab = null;

    // This will be the prefab of our first cell to push the other cells to the bottom
    [SerializeField] private EnhancedScrollerCellView spacerCellViewPrefab = null;

    private RectTransform scrollerRectTransform = null;

    private float totalCellSize = 0;
    private float oldScrollPosition = 0;

    private readonly TextGenerator textGenerator = new();
    private TextGenerationSettings myTextSetting, otherTextSetting, notificationTextSetting;

    [SerializeField] private float verticalPadding = 0f;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        // Init the enhanced scroller
        Text myText = myTextMessagePrefab.MessageText;
        myTextSetting = myText.GetGenerationSettings(myText.rectTransform.rect.size);
        Text otherText = otherTextMessagePrefab.MessageText;
        otherTextSetting = otherText.GetGenerationSettings(otherText.rectTransform.rect.size);
        Text notificationText = notificationTextMessagePrefab.MessageText;
        notificationTextSetting = notificationText.GetGenerationSettings(notificationText.rectTransform.rect.size);

        scroller.Delegate = this;
        scrollerRectTransform = scroller.GetComponent<RectTransform>();

        textMessageDatas.Add(new() { MessageType = EMessageType.SPACER });

        closeButton.onClick.AddListener(OnClickCloseButton);
        sendButton.onClick.AddListener(() => SendTextMessage(messageInputField.text));

        // Init Vivox
        VivoxManager vivoxManager = GameManager.Vivox;
        
        // Init the vivox events
        vivoxManager.ReceiveMessageEvent = null;

        // Set the vivox events
        vivoxManager.ReceiveMessageEvent += OnReceiveTextMessage;
        
        OnActive += () =>
        {
            closeButton.interactable = true;
            sendButton.interactable = true;

            scroller.ClearAll();
            oldScrollPosition = scroller.ScrollPosition;
            scroller.ScrollPosition = 0;

            ResizeScroller();

            scroller.JumpToDataIndex(textMessageDatas.Count - 1, 1f, 1f, tweenType: EnhancedScroller.TweenType.easeInOutSine, tweenTime: 0.5f, jumpComplete: ResetSpacer);
        };
    }

    public override Sequence ActiveAnimation()
    {
        DoTween panelTween = DoTweenUtil.DoAnchoredPos(
           panelRect,
           new Vector2(0f, 1500f),
           Vector2.zero,
           GlobalDefine.panelAnimationDuration,
           Ease.OutExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    public override Sequence DeactiveAnimation()
    {
        DoTween panelTween = DoTweenUtil.DoAnchoredPos(
            panelRect,
            Vector2.zero,
            new Vector2(0f, 1500f),
            GlobalDefine.panelAnimationDuration,
            Ease.InExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    #endregion Override Methods

    #region Unity Events

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendTextMessage(messageInputField.text);
        }
#endif
    }

    #endregion Unity Events

    #region Methods

    public void SendTextMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        GameManager.Vivox.SendMessage(message);
        OnSendTextMessage(message);

        // Init the message inputfield
        messageInputField.text = "";
        messageInputField.ActivateInputField();
    }

    #endregion Methods

    #region Event Methods

    public void OnClickCloseButton()
    {
        closeButton.interactable = false;
        sendButton.interactable = false;
        
        GameManager.UI.ClosePopupPanel<TextChattingPanel>();
    }

    public void OnSendTextMessage(string message)
    {
        // Create the message data
        Player localPlayer = PhotonNetwork.LocalPlayer;
        TextMessageData textMessageData = new()
        {
            MessageType = EMessageType.MY,
            Color = CharacterColor.GetColor((ECharacterColor)localPlayer.CustomProperties[PlayerProperties.PLYAER_COLOR]),
            Message = message,
            IsDie = CheckLocalPlayerDie()
        };

        AddTextMessage(textMessageData);
    }

    public void OnReceiveTextMessage(string userId, string message)
    {
        if (userId == PhotonNetwork.LocalPlayer.UserId) return;

        // Find the other player by the user ID
        Player otherPlayer = GameManager.Network.PlayerDictionaryById[userId];

        // Check local and remote player state
        bool isLocalPlayerDie = CheckLocalPlayerDie();
        bool isRemotePlayerDie = CheckRemotePlayerDie(userId);

        if (!isLocalPlayerDie && isRemotePlayerDie) return;

        // Create the message data
        TextMessageData textMessageData = new()
        {
            MessageType = EMessageType.OTHER,
            Color = CharacterColor.GetColor((ECharacterColor)otherPlayer.CustomProperties[PlayerProperties.PLYAER_COLOR]),
            Nickname = otherPlayer.NickName,
            Message = message,
            IsDie = isRemotePlayerDie
        };

        AddTextMessage(textMessageData);
    }

    #endregion Event Methods

    #region Helpler Methods

    private void ResizeScroller()
    {
        float scrollRectSize = scroller.ScrollRectSize;
        float offset = oldScrollPosition - scroller.ScrollSize;
        Vector2 size = scrollerRectTransform.sizeDelta;

        scrollerRectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);

        totalCellSize = scroller.padding.top + scroller.padding.bottom;
        for (int i = 1; i < textMessageDatas.Count; i++)
        {
            totalCellSize += textMessageDatas[i].CellSize + (i < textMessageDatas.Count - 1 ? scroller.spacing : 0);
        }

        textMessageDatas[0].CellSize = scrollRectSize;
        scrollerRectTransform.sizeDelta = size;

        scroller.ReloadData();

        scroller.ScrollPosition = (totalCellSize - textMessageDatas[textMessageDatas.Count - 1].CellSize) + offset;
    }

    private void ResetSpacer()
    {
        textMessageDatas[0].CellSize = Mathf.Max(scroller.ScrollRectSize - totalCellSize, 0);

        scroller.ReloadData(1.0f);
    }

    private bool CheckLocalPlayerDie()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PlayerProperties.IS_DIE, out object isDie))
        {
            return (bool)isDie;
        }

        return false;
    }

    private bool CheckRemotePlayerDie(string otherUserId)
    {
        if (GameManager.Network.PlayerDictionaryById[otherUserId].CustomProperties.TryGetValue(PlayerProperties.IS_DIE, out object isDie))
        {
            return (bool)isDie;
        }

        return false;
    }

    private void AddTextMessage(TextMessageData textMessageData)
    {
        // Calculate the space needed for the text in the cell
        float height = 0;
        switch (textMessageData.MessageType)
        {
            case EMessageType.MY:
                height += textGenerator.GetPreferredHeight(textMessageData.Message, myTextSetting);
                break;

            case EMessageType.OTHER:
                height += textGenerator.GetPreferredHeight(textMessageData.Message, otherTextSetting);
                height += 90f;
                break;

            default:
                throw new Exception($"Unsupported message type. Input type : {textMessageData.MessageType}");
        }
        float cellSize = height + verticalPadding;
        textMessageData.CellSize = cellSize;

        textMessageDatas.Add(textMessageData);

        if (!gameObject.activeSelf) return;

        // Reset the scroller's position
        scroller.ClearAll();
        oldScrollPosition = scroller.ScrollPosition;
        scroller.ScrollPosition = 0;

        ResizeScroller();

        scroller.JumpToDataIndex(textMessageDatas.Count - 1, 1f, 1f, tweenType: EnhancedScroller.TweenType.easeInOutSine, tweenTime: 0.5f, jumpComplete: ResetSpacer);
    }

    #endregion Helper Methods

    #region IEnhancedScrollerDelegate

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return textMessageDatas.Count;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return textMessageDatas[dataIndex].CellSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        TextMessageCellView cellView;

        TextMessageData textMessageData = textMessageDatas[dataIndex];

        switch (textMessageData.MessageType)
        {
            case EMessageType.MY:
                cellView = scroller.GetCellView(myTextMessagePrefab) as TextMessageCellView;
                cellView.SetMyMessageData(textMessageData);
                cellView.name = "[MyTextMessage]";
                break;

            case EMessageType.OTHER:
                cellView = scroller.GetCellView(otherTextMessagePrefab) as TextMessageCellView;
                cellView.SetOtherMessageData(textMessageData);
                cellView.name = "[OtherTextMessage] " + textMessageData.Nickname;
                break;

            default:
                cellView = scroller.GetCellView(spacerCellViewPrefab) as TextMessageCellView;
                cellView.name = "[Spacer]";
                break;
        }

        return cellView;
    }

    #endregion IEnhancedScrollerDelegate

    #region Photon Events

    public override void OnLeftRoom()
    {
        textMessageDatas.Clear();
        textMessageDatas.Add(new() { MessageType = EMessageType.SPACER });
    }

    #endregion Photon Events
}
