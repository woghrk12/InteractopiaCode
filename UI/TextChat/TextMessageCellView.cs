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
    public Color Color = Color.white;
    public string Nickname = string.Empty;
    public string Message = string.Empty;
    public bool IsDie = false;
    public float CellSize = 0f;
}

public class TextMessageCellView : EnhancedScrollerCellView
{
    #region Variables

    [SerializeField] private Image characterImage = null;
    [SerializeField] private Text nicknameText = null;
    [SerializeField] private Text messageText = null;

    [SerializeField] private Image messagePanelImage = null;
    
    #endregion Variables

    #region Properties

    public Text MessageText => messageText;

    #endregion Properties

    #region Unity Events

    private void Awake()
    {
        if (characterImage != null)
        {
            Material instance = Instantiate(characterImage.material);
            characterImage.material = instance;
        }
    }

    #endregion Unity Events

    public void SetNotificationMessageData(TextMessageData messageData)
    {
        messageText.text = messageData.Message;
    }

    public void SetMyMessageData(TextMessageData messageData)
    {
        messageText.text = messageData.Message;
        characterImage.material.SetColor("_CharacterColor", messageData.Color);

        if (messageData.IsDie) SetGhostMessage();
    }

    public void SetOtherMessageData(TextMessageData messageData)
    {
        nicknameText.text = messageData.Nickname;
        nicknameText.color = messageData.Color;
        messageText.text = messageData.Message;
        characterImage.material.SetColor("_CharacterColor", messageData.Color);

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
