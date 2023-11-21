using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public enum EMessageType
{ 
    NONE = -1,
    MY,
    OTHER,
    SPACER
}

public class TextMessageData
{
    public EMessageType MessageType = EMessageType.NONE;
    public int ActorNumber = -1;
    public string Message = string.Empty;
    public bool IsDie = false;
    public float CellSize = 0f;
}

public class TextMessageCellView : EnhancedScrollerCellView
{
    #region Variables

    [SerializeField] private UICharacter uiCharacter = null;
    [SerializeField] private Text messageText = null;

    [SerializeField] private Image messagePanelImage = null;

    #endregion Variables

    #region Properties

    public Text MessageText => messageText;

    #endregion Properties

    public void SetNotificationMessageData(TextMessageData messageData)
    {
        messageText.text = messageData.Message;
    }

    public void SetMessageData(TextMessageData messageData)
    {
        messageText.text = messageData.Message;
        uiCharacter.SetCharacter(messageData.ActorNumber, EUICharacterType.HEAD);

        if (messageData.IsDie) SetGhostMessage();
    }

    private void SetGhostMessage()
    {
        Color panelColor = messagePanelImage.color;

        panelColor.r *= 0.5f;
        panelColor.g *= 0.5f;
        panelColor.b *= 0.5f;

        messagePanelImage.color = panelColor;
    }
}
