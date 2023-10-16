using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Realtime;
using System.Collections.Generic;
using Photon.Pun;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MeetingPanel : UIPanel
{
    #region Variables

    [SerializeField] private PlayerButton assasinButton = null;

    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button voiceChattingButton = null;
    [SerializeField] private Button textChattingButton = null;
    [SerializeField] private Button skipButton = null;

    [SerializeField] private GameObject voiceBlockImageObject = null;

    [SerializeField] private RectTransform stateGroupParent = null;
    private Dictionary<int, PlayerStateGroup> playerStateDictionary = new();
    private Dictionary<int, PlayerStateGroup> alivePlayerStateDictionary = new();

    private PlayerStateGroup activeGroup = null;

    #endregion Variables

    #region Properties

    public Dictionary<int, PlayerStateGroup> AlivePlayerStateDictionary => alivePlayerStateDictionary;

    public Button VoiceChattingButton => voiceChattingButton;

    #endregion Properties

    #region Override Methods

    public override void InitPanel()
    {
        var playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            PlayerStateGroup stateGroup = GameManager.Resource.Instantiate("UI/MeetingPanel/PlayerState", stateGroupParent).GetComponent<PlayerStateGroup>();
            stateGroup.InitGroup(this, player.Key);
            playerStateDictionary.Add(player.Key, stateGroup);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(stateGroupParent);

        settingButton.onClick.AddListener(OnClickSettingButton);
        voiceChattingButton.onClick.AddListener(OnClickVoiceChattingButton);
        textChattingButton.onClick.AddListener(OnClickTextChattingButton);
        skipButton.onClick.AddListener(OnClickSkipButton);

        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        if ((ERoleType)playerSetting[PlayerProperties.PLAYER_TEAM] == ERoleType.MAFIA 
            && (EMafiaRole)playerSetting[PlayerProperties.PLAYER_ROLE] == EMafiaRole.ASSASSIN)
        {
            GameManager.Input.SkillButton = assasinButton;
        }
        else
        {
            Destroy(assasinButton.gameObject);
        }

        OnActive += () =>
        {
            alivePlayerStateDictionary.Clear();

            List<int> aliveCitizen = GameManager.InGame.AliveCitizenPlayerList;
            List<int> aliveMafia = GameManager.InGame.AliveMafiaPlayerList;

            bool isDie = (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.IS_DIE];

            foreach (KeyValuePair<int, PlayerStateGroup> stateGroup in playerStateDictionary)
            {
                if (aliveCitizen.Contains(stateGroup.Key) || aliveMafia.Contains(stateGroup.Key))
                {
                    alivePlayerStateDictionary.Add(stateGroup.Key, stateGroup.Value);

                    if (isDie)
                    {
                        stateGroup.Value.DisableButton();
                    }
                    else
                    {
                        stateGroup.Value.ActivePlayerListGroup();
                    }
                }
                else 
                {
                    stateGroup.Value.DeactiveGroup();
                }
            }

            skipButton.interactable = !isDie;
            if (assasinButton != null)
            {
                assasinButton.IsInteractable = true;
            }

            activeGroup = null;

            voiceBlockImageObject.SetActive(GameManager.Vivox.IsMute);
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

    public void ChangeActiveGroup(PlayerStateGroup playerStateGroup)
    {
        if (activeGroup != null && activeGroup.StateOwner != playerStateGroup.StateOwner)
        {
            activeGroup.ActivePlayerListGroup(); 
        }

        activeGroup = playerStateGroup;
    }

    public void DisableStateButtons()
    {
        foreach (KeyValuePair<int, PlayerStateGroup> stateGroup in alivePlayerStateDictionary)
        {
            stateGroup.Value.DisableButton();
        }

        skipButton.interactable = false;
    }

    public void DeactiveStateButton(int actorNumber)
    {
        alivePlayerStateDictionary[actorNumber].DeactiveGroup();
        alivePlayerStateDictionary.Remove(actorNumber);
    }

    #endregion Methods

    #region Event Methods

    public void OnClickSettingButton() => GameManager.UI.PopupPanel<SettingPanel>();

    public void OnClickVoiceChattingButton()
    {
        if (GameManager.Vivox.IsMute)
        {
            GameManager.Vivox.SetLocalUnmute();
            voiceBlockImageObject.SetActive(false);
        }
        else
        {
            GameManager.Vivox.SetLocalMute();
            voiceBlockImageObject.SetActive(true);
        }
    }

    public void OnClickTextChattingButton()
    {
        if (!GameManager.Vivox.IsConnected) return;

        GameManager.UI.PopupPanel<TextChattingPanel>();
    }

    public void OnClickSkipButton()
    {
        DisableStateButtons();
        skipButton.interactable = false;
        GameManager.Meeting.Vote(-1);
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int actorNumber = otherPlayer.ActorNumber;

        if (alivePlayerStateDictionary.ContainsKey(actorNumber))
        {
            playerStateDictionary[otherPlayer.ActorNumber].DeactiveGroup();
            alivePlayerStateDictionary.Remove(actorNumber);
        }
    }

    #endregion Photon Events
}
