using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CharacterSettingPanel : UIPanel
{
    #region Variables
    private readonly string[] COLOR_STRING_KOR = { "»¡°­", "ÆÄ¶û", "ÃÊ·Ï", "ºÐÈ«", "ÁÖÈ²", "³ë¶û", "°ËÁ¤", "Èò»ö", "º¸¶ó", "°¥»ö", "ÇÏ´Ã", "¿¬µÎ" };

    [SerializeField] private Button closeButton = null;

    [Header("Character Color Group")]
    [SerializeField] private ColorButton[] colorButtons = null;

    [Header("Preview Group")]
    [SerializeField] private Image previewImage = null;
    [SerializeField] private Text valueText = null;
    [SerializeField] private Button confirmButton = null;

    private UICharacterData uiCharacterData = null;
    private ECharacterColor selectedColor = ECharacterColor.NONE;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        confirmButton.onClick.AddListener(OnClickConfirmButton);

        uiCharacterData = GameManager.Resource.Load<UICharacterData>("Data/UIColorData");

        foreach (ColorButton button in colorButtons)
        {
            button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);
                    confirmButton.interactable = button.ButtonState == ColorButton.EButtonState.NONE;
                    selectedColor = button.CharacterColor;
                    SetPreviewGroup(button.CharacterColor);
                });
        }

        OnActive += () =>
        {
            closeButton.interactable = true;
            confirmButton.interactable = false;

            int[] remainColorList = (int[])PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.REMAIN_COLOR_LIST];
            int[] usedColorList = (int[])PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.USED_COLOR_LIST];
            int playerColor = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.PLAYER_COLOR];

            foreach (int color in remainColorList)
            {
                colorButtons[color].SetButton(ColorButton.EButtonState.NONE);
            }

            foreach (int color in usedColorList)
            {
                colorButtons[color].SetButton(color == playerColor ? ColorButton.EButtonState.MINE : ColorButton.EButtonState.INUSE);
            }

            SetPreviewGroup((ECharacterColor)playerColor);
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

    private void SetPreviewGroup(ECharacterColor color)
    {
        previewImage.sprite = uiCharacterData.GetHeadSprite(color);
        //valueText.text = color.ToString();
        valueText.text = COLOR_STRING_KOR[(int)color];
    }

    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        confirmButton.interactable = false;
        GameManager.UI.OpenPanel<RoomPanel>();
    }

    public void OnClickConfirmButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;

        int[] remainColorList = (int[])roomSetting[CustomProperties.REMAIN_COLOR_LIST];
        int[] usedColorList = (int[])roomSetting[CustomProperties.USED_COLOR_LIST];
        int oldColor = (int)playerSetting[PlayerProperties.PLAYER_COLOR];

        remainColorList = ArrayHelper.Add(oldColor, remainColorList);
        remainColorList = ArrayHelper.Remove((int)selectedColor, remainColorList);
        usedColorList = ArrayHelper.Add((int)selectedColor, usedColorList);
        usedColorList = ArrayHelper.Remove(oldColor, usedColorList);

        roomSetting[CustomProperties.REMAIN_COLOR_LIST] = remainColorList;
        roomSetting[CustomProperties.USED_COLOR_LIST] = usedColorList;
        playerSetting[PlayerProperties.PLAYER_COLOR] = (int)selectedColor;

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnRoomPropertiesUpdate(PhotonHashTable propertiesThatChanged)
    {
        int[] remainColorList = (int[])propertiesThatChanged[CustomProperties.REMAIN_COLOR_LIST];
        int[] usedColorList = (int[])propertiesThatChanged[CustomProperties.USED_COLOR_LIST];
        int playerColor = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.PLAYER_COLOR];

        foreach (int color in remainColorList)
        {
            colorButtons[color].SetButton(ColorButton.EButtonState.NONE);
        }

        foreach (int color in usedColorList)
        {
            colorButtons[color].SetButton(color == playerColor ? ColorButton.EButtonState.MINE : ColorButton.EButtonState.INUSE);
        }

        if (selectedColor != ECharacterColor.NONE)
        {
            confirmButton.interactable = colorButtons[(int)selectedColor].ButtonState == ColorButton.EButtonState.NONE;
        }
    }

    #endregion Photon Events
}
