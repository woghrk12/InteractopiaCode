using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerStateGroup : MonoBehaviour
{
    #region Variables

    private MeetingPanel meetingPanel = null;

    private EOpenVote openVoteMode = EOpenVote.NONE;

    [Header("UI Character of the player and voters")]
    private int stateOwner = -1;
    [SerializeField] private UICharacter playerCharacter = null;
    [SerializeField] private UICharacter[] voterCharacters = null;

    private int voterIndex = 0;

    [Header("Buttons for voting")]
    [SerializeField] private Button playerStateButton = null;
    [SerializeField] private Button yesButton = null;
    [SerializeField] private Button noButton = null;

    [Header("State objects")]
    [SerializeField] private GameObject votePlayerList = null;
    [SerializeField] private GameObject selectGroup = null;

    [Header("UI components")]
    [SerializeField] private Image characterImage = null;
    [SerializeField] private Text nicknameText = null;
    [SerializeField] private Image speakingIconImage = null;

    #endregion Variables

    #region Properties

    public int StateOwner => stateOwner;

    #endregion Properties

    #region Methods

    public void InitGroup(MeetingPanel meetingPanel, int actorNum, EOpenVote mode)
    {
        this.meetingPanel = meetingPanel;

        playerStateButton.onClick.AddListener(OnClickPlayerStateButton);
        yesButton.onClick.AddListener(OnClickYesButton);
        noButton.onClick.AddListener(OnClickNoButton);

        GameManager.Meeting.VoteEvent += OnAddedVoter;

        stateOwner = actorNum;
        playerCharacter.SetCharacter(actorNum, EUICharacterType.HEAD, isColorNickname: false);

        voterIndex = 0;
        foreach (UICharacter voterCharacter in voterCharacters)
        {
            voterCharacter.gameObject.SetActive(false);
        }

        Player ownerPlayer = GameManager.Network.PlayerDictionaryByActorNum[actorNum];
        Player localPlayer = PhotonNetwork.LocalPlayer;
        ERoleType ownerTeam = (ERoleType)ownerPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM];
        ERoleType localTeam = (ERoleType)localPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM];
        if (localTeam == ERoleType.MAFIA)
        {
            playerCharacter.SetNicknameColor(ownerTeam == localTeam ? Color.red : Color.white);
        }

        openVoteMode = mode;
    }

    public void ResetGroup()
    {
        selectGroup.SetActive(false);
        votePlayerList.SetActive(true);

        playerStateButton.interactable = true;
    }

    public void DisableButton()
    {
        selectGroup.SetActive(false);
        votePlayerList.SetActive(true);

        playerStateButton.interactable = false;
    }

    public void DeactiveGroup()
    {
        selectGroup.SetActive(false);
        votePlayerList.SetActive(false);

        playerStateButton.interactable = false;

        Color deactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
        playerStateButton.GetComponent<Image>().color = deactiveColor;
        speakingIconImage.color = deactiveColor;
        nicknameText.color = deactiveColor;
        characterImage.color = deactiveColor;
    }

    #endregion Methods

    #region Event Methods

    private void OnAddedVoter(int voter, int target)
    {
        if (target != stateOwner) return;
        if (openVoteMode == EOpenVote.PRIVATE) return;

        voterCharacters[voterIndex].gameObject.SetActive(true);
        voterCharacters[voterIndex].SetCharacter(voter, EUICharacterType.HEAD, openVoteMode == EOpenVote.BLINDNESS);

        voterIndex++;
    }

    public void OnClickPlayerStateButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        selectGroup.SetActive(true);
        votePlayerList.SetActive(false);

        playerStateButton.interactable = false;

        meetingPanel.ChangeActiveGroup(this);
    }

    public void OnClickYesButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        meetingPanel.BlockVote();

        votePlayerList.SetActive(true);
        selectGroup.SetActive(false); 

        GameManager.Meeting.Vote(stateOwner);
    }

    public void OnClickNoButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        votePlayerList.SetActive(true);
        selectGroup.SetActive(false);
        playerStateButton.interactable = true;
    }

    #endregion Event Methods
}
