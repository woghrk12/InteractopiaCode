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

    [SerializeField] private Text meetingStatusText = null;
    [SerializeField] private Image timerImage = null;
    [SerializeField] private Text timerText = null;

    [SerializeField] private PlayerButton assasinButton = null;

    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button minimapButton = null;
    [SerializeField] private Button voiceChattingButton = null;
    [SerializeField] private Button textChattingButton = null;
    [SerializeField] private Button skipButton = null;

    [SerializeField] private GameObject voiceBlockImageObject = null;

    [SerializeField] private RectTransform stateGroupParent = null;
    private Dictionary<int, PlayerStateGroup> playerStateDictionary = new();

    private PlayerStateGroup activeGroup = null;

    [SerializeField] private CanvasGroup[] npcImageList = null;

    #endregion Variables

    #region Properties

    public Text MeetingStatusText => meetingStatusText;

    #endregion Properties

    #region Override Methods

    public override void InitPanel()
    {
        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        EOpenVote openVoteMode = (EOpenVote)roomSetting[CustomProperties.OPEN_VOTE];

        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            PlayerStateGroup stateGroup = GameManager.Resource.Instantiate("UI/MeetingPanel/PlayerState", stateGroupParent).GetComponent<PlayerStateGroup>();
            stateGroup.InitGroup(this, player.Key, openVoteMode);
            playerStateDictionary.Add(player.Key, stateGroup);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(stateGroupParent);

        settingButton.onClick.AddListener(OnClickSettingButton);
        minimapButton.onClick.AddListener(OnClickMinimapButton);
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
            // set the player state
            List<int> aliveNonMafia = GameManager.InGame.AliveNonMafiaPlayerList;
            List<int> aliveMafia = GameManager.InGame.AliveMafiaPlayerList;
           
            foreach (KeyValuePair<int, PlayerStateGroup> stateGroup in playerStateDictionary)
            {
                if (aliveNonMafia.Contains(stateGroup.Key) || aliveMafia.Contains(stateGroup.Key))
                {
                    stateGroup.Value.DisableButton();
                }
                else 
                {
                    stateGroup.Value.DeactiveGroup();
                }
            }

            // Set the npc state
            Dictionary<ENPCRole, BaseNPC> npcDictionary = GameManager.InGame.NPCList;
            Dictionary<ENPCRole, BaseNPC> deadNPCDictionary = GameManager.InGame.DeadNPCList;

            foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcDictionary)
            {
                npcImageList[(int)npc.Key].alpha = 1f;
            }

            foreach (KeyValuePair<ENPCRole, BaseNPC> npc in deadNPCDictionary)
            {
                npcImageList[(int)npc.Key].alpha = 0.2f;
            }

            // Set the button interactable
            skipButton.interactable = false;
            if (assasinButton != null)
            {
                assasinButton.IsInteractable = true;
            }

            bool isDie = (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.IS_DIE];
            voiceChattingButton.interactable = !isDie;
            voiceBlockImageObject.SetActive(GameManager.Vivox.IsMute);

            activeGroup = null;
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

    public void SetActiveVoiceButton(bool isActive)
    {
        voiceChattingButton.gameObject.SetActive(isActive);
        voiceBlockImageObject.SetActive(isActive);
    }

    public void SetTimer(float maxTime, float curTime)
    {
        timerText.text = curTime.ToString("F0");
        timerImage.fillAmount = curTime / maxTime;
    }

    public void ChangeActiveGroup(PlayerStateGroup playerStateGroup)
    {
        if (activeGroup != null && activeGroup.StateOwner != playerStateGroup.StateOwner)
        {
            activeGroup.ResetGroup(); 
        }

        activeGroup = playerStateGroup;
    }

    public void EnableVote()
    {
        List<int> aliveCitizenList = GameManager.InGame.AliveCitizenPlayerList;
        List<int> aliveMafiaList = GameManager.InGame.AliveMafiaPlayerList;
        foreach (int player in aliveCitizenList)
        {
            playerStateDictionary[player].ResetGroup();
        }
        foreach (int player in aliveMafiaList)
        {
            playerStateDictionary[player].ResetGroup();
        }

        skipButton.interactable = true;
    }

    public void BlockVote()
    {
        List<int> aliveCitizenList = GameManager.InGame.AliveCitizenPlayerList;
        List<int> aliveMafiaList = GameManager.InGame.AliveMafiaPlayerList;
        foreach (int player in aliveCitizenList)
        {
            playerStateDictionary[player].DisableButton();
        }
        foreach (int player in aliveMafiaList)
        {
            playerStateDictionary[player].DisableButton();
        }

        skipButton.interactable = false;
    }

    public void DeactiveStateButton(int actorNumber)
    {
        playerStateDictionary[actorNumber].DeactiveGroup();
    }

    #endregion Methods

    #region Event Methods

    public void OnClickSettingButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.UI.PopupPanel<SettingPanel>();
    }

    public void OnClickMinimapButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.UI.GetPanel<MinimapPanel>().OpenCause = EMinimapOpenCause.NONE;
        GameManager.UI.PopupPanel<MinimapPanel>();
    }

    public void OnClickVoiceChattingButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

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

        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.UI.PopupPanel<TextChattingPanel>();
    }

    public void OnClickSkipButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        BlockVote();

        GameManager.Meeting.Vote(-1);
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerStateDictionary[otherPlayer.ActorNumber].DeactiveGroup();
    }

    #endregion Photon Events
}
