using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class GuestRuleSettingPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private RectTransform optionContentRect = null;

    [SerializeField] private Button closeButton = null;
    
    [Header("Common setting")]
    [SerializeField] private Toggle shortDistanceVoiceToggle = null;
    [SerializeField] private Toggle randomStartPointToggle = null;
    [SerializeField] private Text numMafiaText = null;
    [SerializeField] private Text numNeutralText = null;
    [SerializeField] private Toggle hideEmissionInfoToggle = null;
    [SerializeField] private Toggle blindMafiaModeToggle = null;
    [SerializeField] private Toggle ghostCanSeeRoleToggle = null;

    [Header("Meeting and vote setting")]
    [SerializeField] private Toggle openingAddressToggle = null;
    [SerializeField] private Text meetingTimeText = null;
    [SerializeField] private Text voteTimeText = null;
    [SerializeField] private Text openVoteText = null;
    [SerializeField] private Toggle openVoteResultToggle = null;
    [SerializeField] private Text emergencyMeetingCooldownText = null;

    [Header("Roles and NPC setting")]
    [SerializeField] private RoleData roleData = null;
    [SerializeField] private RectTransform citizenRoleParent = null;
    [SerializeField] private RectTransform mafiaRoleParent = null;
    [SerializeField] private RectTransform neutralRoleParent = null;
    [SerializeField] private RectTransform npcRoleParent = null;
    [SerializeField] private GameObject roleGroupPrefab = null;

    private RoleOptionGroup[] citizenRoleGroup = new RoleOptionGroup[0];
    private RoleOptionGroup[] mafiaRoleGroup = new RoleOptionGroup[0];
    private RoleOptionGroup[] neutralRoleGroup = new RoleOptionGroup[0];
    private RoleOptionGroup[] npcRoleGroup = new RoleOptionGroup[0];

    [Header("Movement and sight setting")]
    [SerializeField] private Text citizenSightText = null;
    [SerializeField] private Text mafiaSightText = null;
    [SerializeField] private Text neutralSightText = null;
    [SerializeField] private Text moveSpeedText = null;

    [Header("Cooldown and mission setting")]
    [SerializeField] private Text killCooldownText = null;
    [SerializeField] private Text sabotageCooldownText = null;
    [SerializeField] private Text numMissionText = null;
    [SerializeField] private Text minMissionCooldownText = null;
    [SerializeField] private Text maxMissionCooldownText = null;
    [SerializeField] private Text numSpecialMissionText = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        // Create citizen role option group prefab
        RoleInfo[] citizenRoles = roleData.GetAllRoleInfos(ERoleType.CITIZEN);
        foreach (RoleInfo roleInfo in citizenRoles)
        {
            RoleOptionGroup roleGroup = Instantiate(roleGroupPrefab, citizenRoleParent).GetComponent<RoleOptionGroup>();
            roleGroup.InitGroup(roleInfo);
            citizenRoleGroup = ArrayHelper.Add(roleGroup, citizenRoleGroup);
        }

        // Create mafia role option group prefab
        RoleInfo[] mafiaRoles = roleData.GetAllRoleInfos(ERoleType.MAFIA);
        foreach (RoleInfo roleInfo in mafiaRoles)
        {
            RoleOptionGroup roleGroup = Instantiate(roleGroupPrefab, mafiaRoleParent).GetComponent<RoleOptionGroup>();
            roleGroup.InitGroup(roleInfo);
            mafiaRoleGroup = ArrayHelper.Add(roleGroup, mafiaRoleGroup);
        }

        // Create neutral role option group prefab
        RoleInfo[] neutralRoles = roleData.GetAllRoleInfos(ERoleType.NEUTRAL);
        foreach (RoleInfo roleInfo in neutralRoles)
        {
            RoleOptionGroup roleGroup = Instantiate(roleGroupPrefab, neutralRoleParent).GetComponent<RoleOptionGroup>();
            roleGroup.InitGroup(roleInfo);
            neutralRoleGroup = ArrayHelper.Add(roleGroup, neutralRoleGroup);
        }

        // Create npc role option group prefab
        RoleInfo[] npcRoles = roleData.GetAllRoleInfos(ERoleType.NPC);
        foreach (RoleInfo roleInfo in npcRoles)
        {
            RoleOptionGroup roleGroup = Instantiate(roleGroupPrefab, npcRoleParent).GetComponent<RoleOptionGroup>();
            roleGroup.InitGroup(roleInfo);
            npcRoleGroup = ArrayHelper.Add(roleGroup, npcRoleGroup);
        }

        OnActive += () =>
        {
            closeButton.interactable = true;
            SetOptionText(PhotonNetwork.CurrentRoom.CustomProperties);
        };

        LayoutRebuilder.ForceRebuildLayoutImmediate(optionContentRect);
    }
    public override Sequence ActiveAnimation()
    {
        Tween panelTween = DoTweenUtil.DoAnchoredPos(
            panelRect,
            new Vector2(0f, 1500f),
            Vector2.zero,
            GlobalDefine.panelAnimationDuration,
            Ease.OutExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    public override Sequence DeactiveAnimation()
    {
        Tween panelTween = DoTweenUtil.DoAnchoredPos(
            panelRect,
            Vector2.zero,
            new Vector2(0f, 1500f),
            GlobalDefine.panelAnimationDuration,
            Ease.InExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    #endregion Override Methods

    #region Methods

    private void SetOptionText(PhotonHashTable roomProperties)
    {
        // Common group setting
        shortDistanceVoiceToggle.isOn = (bool)roomProperties[CustomProperties.SHORT_DISTANCE_VOICE];
        randomStartPointToggle.isOn = (bool)roomProperties[CustomProperties.RANDOM_START_POINT];
        numMafiaText.text = ((int)roomProperties[CustomProperties.NUM_MAFIAS]).ToString();
        numNeutralText.text = ((int)roomProperties[CustomProperties.NUM_NEUTRALS]).ToString();
        hideEmissionInfoToggle.isOn = (bool)roomProperties[CustomProperties.HIDE_EMISSION_INFO];
        blindMafiaModeToggle.isOn = (bool)roomProperties[CustomProperties.BLIND_MAFIA_MODE];
        ghostCanSeeRoleToggle.isOn = (bool)roomProperties[CustomProperties.GOAST_CAN_SEE_ROLE];

        // Meeting and vote group setting
        openingAddressToggle.isOn = (bool)roomProperties[CustomProperties.OPENING_ADDRESS];
        meetingTimeText.text = ((int)roomProperties[CustomProperties.MEETING_TIME]).ToString();
        voteTimeText.text = ((int)roomProperties[CustomProperties.VOTE_TIME]).ToString();
        openVoteText.text = ((EOpenVote)roomProperties[CustomProperties.OPEN_VOTE]).ToString();
        openVoteResultToggle.isOn = (bool)roomProperties[CustomProperties.OPEN_VOTE_RESULT];
        emergencyMeetingCooldownText.text = ((int)roomProperties[CustomProperties.EMERGENCY_MEETING_COOLDOWN]).ToString();

        int[] roleList;
        // Roles and NPC group setting
        // Citizen group setting
        foreach (RoleOptionGroup option in citizenRoleGroup)
        {
            option.SetGroup(RoleOption.EOptionState.EXCLUDE);
        }

        roleList = (int[])roomProperties[CustomProperties.ESSENTIAL_CITIZEN_ROLES];
        foreach (int role in roleList)
        {
            citizenRoleGroup[role].SetGroup(RoleOption.EOptionState.ESSENTIAL);
        }

        roleList = (int[])roomProperties[CustomProperties.OPTIONAL_CITIZEN_ROLES];
        foreach (int role in roleList)
        {
            citizenRoleGroup[role].SetGroup(RoleOption.EOptionState.OPTIONAL);
        }

        // Mafia group setting
        foreach (RoleOptionGroup option in mafiaRoleGroup)
        {
            option.SetGroup(RoleOption.EOptionState.EXCLUDE);
        }

        roleList = (int[])roomProperties[CustomProperties.ESSENTIAL_MAFIA_ROLES];
        foreach (int role in roleList)
        {
            mafiaRoleGroup[role].SetGroup(RoleOption.EOptionState.ESSENTIAL);
        }

        roleList = (int[])roomProperties[CustomProperties.OPTIONAL_MAFIA_ROLES];
        foreach (int role in roleList)
        {
            mafiaRoleGroup[role].SetGroup(RoleOption.EOptionState.OPTIONAL);
        }

        // Neutral group setting
        foreach (RoleOptionGroup option in neutralRoleGroup)
        {
            option.SetGroup(RoleOption.EOptionState.EXCLUDE);
        }

        roleList = (int[])roomProperties[CustomProperties.ESSENTIAL_NEUTRAL_ROLES];
        foreach (int role in roleList)
        {
            neutralRoleGroup[role].SetGroup(RoleOption.EOptionState.ESSENTIAL);
        }

        roleList = (int[])roomProperties[CustomProperties.OPTIONAL_NEUTRAL_ROLES];
        foreach (int role in roleList)
        {
            neutralRoleGroup[role].SetGroup(RoleOption.EOptionState.OPTIONAL);
        }

        // NPC group setting
        foreach (RoleOptionGroup option in npcRoleGroup)
        {
            option.SetGroup(RoleOption.EOptionState.EXCLUDE);
        }
        
        roleList = (int[])roomProperties[CustomProperties.NPC_ROLES];
        foreach (int role in roleList)
        {
            npcRoleGroup[role].SetGroup(RoleOption.EOptionState.ESSENTIAL);
        }

        // Movement and sight group setting
        citizenSightText.text = ((ESightRange)roomProperties[CustomProperties.CITIZEN_SIGHT]).ToString();
        mafiaSightText.text = ((ESightRange)roomProperties[CustomProperties.MAFIA_SIGHT]).ToString();
        neutralSightText.text = ((ESightRange)roomProperties[CustomProperties.NEUTRAL_SIGHT]).ToString();
        moveSpeedText.text = ((EMoveSpeed)roomProperties[CustomProperties.MOVE_SPEED]).ToString();

        // Cooldown and mission group setting
        killCooldownText.text = ((int)roomProperties[CustomProperties.KILL_COOLDOWN]).ToString();
        sabotageCooldownText.text = ((int)roomProperties[CustomProperties.SABOTAGE_COOLDOWN]).ToString();
        numMissionText.text = ((int)roomProperties[CustomProperties.NUM_MISSION]).ToString();
        minMissionCooldownText.text = ((int)roomProperties[CustomProperties.MIN_MISSION_COOLDOWN]).ToString();
        maxMissionCooldownText.text = ((int)roomProperties[CustomProperties.MAX_MISSION_COOLDOWN]).ToString();
        numSpecialMissionText.text = ((ENumMission)roomProperties[CustomProperties.NUM_MISSION]).ToString();
    }

    #endregion Methods


    #region Event Methods

    public void OnClickCloseButton()
    {
        closeButton.interactable = false;
        GameManager.UI.ClosePopupPanel<GuestRuleSettingPanel>();
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnRoomPropertiesUpdate(PhotonHashTable propertiesThatChanged)
        => SetOptionText(propertiesThatChanged);

    #endregion Photon Events
}
