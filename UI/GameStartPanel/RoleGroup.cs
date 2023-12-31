using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoleGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private UICharacter[] uiCharacters = null;

    #endregion Variables

    #region Methods

    public void SetAllPlayer()
    {
        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;

        // Set UI Characters by using the player list
        int index = 0;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            uiCharacters[index].gameObject.SetActive(true);
            uiCharacters[index].SetCharacter(player.Key, EUICharacterType.HEAD);
            index++;
        }

        for (int i = index; i < uiCharacters.Length; i++)
        {
            uiCharacters[i].gameObject.SetActive(false);
        }
    }

    public void SetCitizenGroup()
    {
        List<int> citizenPlayerList = GameManager.InGame.CitizenPlayerList;
        
        // Set UI Characters by using the player list
        int index = 0;
        foreach (int citizen in citizenPlayerList)
        {
            uiCharacters[index].gameObject.SetActive(true);
            uiCharacters[index].SetCharacter(citizen, EUICharacterType.HEAD);
            index++;
        }

        for (int i = index; i < uiCharacters.Length; i++)
        {
            uiCharacters[i].gameObject.SetActive(false);
        }
    }

    public void SetMafiaGroup(bool isBlind)
    {
        List<int> mafiaPlayerList = isBlind ? new() { PhotonNetwork.LocalPlayer.ActorNumber } : GameManager.InGame.MafiaPlayerList;

        // Set UI Characters by using the player list
        int index = 0;
        foreach (int mafia in mafiaPlayerList)
        {
            uiCharacters[index].gameObject.SetActive(true);
            uiCharacters[index].SetCharacter(mafia, EUICharacterType.HEAD, isColorNickname: false);
            index++;
        }

        for (int i = index; i < uiCharacters.Length; i++)
        {
            uiCharacters[i].gameObject.SetActive(false);
        }
    }

    public void SetNeutralGroup(int actorNum)
    {
        uiCharacters[0].gameObject.SetActive(true);
        uiCharacters[0].SetCharacter(actorNum, EUICharacterType.HEAD, isColorNickname: false);
    }

    #endregion Methods
}
