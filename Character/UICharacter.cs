using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public enum EUICharacterType { NONE = -1, HEAD, BODY, DEAD, END }

public class UICharacter : MonoBehaviour
{
    #region Variables

    [SerializeField] private Image characterImage = null;
    [SerializeField] private Text playerNickname = null;

    private UICharacterData uiCharacterData = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        uiCharacterData = GameManager.Resource.Load<UICharacterData>("Data/UIColorData");
    }

    #endregion Unity Events

    #region Methods

    public void SetCharacter(int actorNum, EUICharacterType type, bool isBlind = false, bool isColorNickname = true)
    {
        Dictionary<int, Player> playerDictionary = PhotonNetwork.CurrentRoom.Players;

        if (!playerDictionary.ContainsKey(actorNum))
        {
            gameObject.SetActive(false);

            if (playerNickname != null)
            {
                playerNickname.gameObject.SetActive(false);
            }

            return;
        }

        PhotonHashTable playerSetting = playerDictionary[actorNum].CustomProperties;
        ECharacterColor color = (ECharacterColor)playerSetting[PlayerProperties.PLAYER_COLOR];
        switch (type)
        {
            case EUICharacterType.HEAD:
                characterImage.sprite = uiCharacterData.GetHeadSprite(isBlind ? ECharacterColor.WHITE : color);
                break;

            case EUICharacterType.BODY:
                characterImage.sprite = uiCharacterData.GetBodySprite(isBlind ? ECharacterColor.WHITE : color);
                break;

            case EUICharacterType.DEAD:
                characterImage.sprite = uiCharacterData.GetDeadSprite(isBlind ? ECharacterColor.WHITE : color);
                break;

            default:
                throw new System.Exception($"Unsupported type. Input type : {type}");
        }

        if (playerNickname != null)
        {
            playerNickname.text = playerDictionary[actorNum].NickName;
            playerNickname.color = isColorNickname ? CharacterColor.GetColor(color) : CharacterColor.White;
        }
    }

    public void SetNicknameColor(Color color)
    { 
        playerNickname.color = color;
    }
    #endregion Methods
}
