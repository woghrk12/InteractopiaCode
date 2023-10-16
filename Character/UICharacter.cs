using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class UICharacter : MonoBehaviour
{
    #region Variables

    [SerializeField] private Image characterImage = null;
    [SerializeField] private Text playerNickname = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        Material material = Instantiate(characterImage.material);
        characterImage.material = material;
    }

    #endregion Unity Events

    #region Methods

    public void SetCharacter(int actorNum)
    {
        Dictionary<int, Player> playerDictionary = PhotonNetwork.CurrentRoom.Players;

        if (!playerDictionary.ContainsKey(actorNum))
        {
            gameObject.SetActive(false);
            return;
        }

        PhotonHashTable playerSetting = playerDictionary[actorNum].CustomProperties;

        characterImage.material.SetColor("_CharacterColor", CharacterColor.GetColor((ECharacterColor)playerSetting[PlayerProperties.PLYAER_COLOR]));

        if (playerNickname != null)
        {
            playerNickname.text = playerDictionary[actorNum].NickName;
        }
    }

    #endregion Methods
}
