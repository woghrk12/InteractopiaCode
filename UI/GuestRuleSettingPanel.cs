using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class GuestRuleSettingPanel : UIPanel
{
    #region Variables
    private readonly string[] OPEN_VOTE_STRING_KOR = { "비공개", "익명", "공개" };
    private readonly string[] SIGHT_RANGE_STRING_KOR = { "좁음", "보통", "넓음" };
    private readonly string[] MOVE_SPEED_STRING_KOR = { "느림", "보통", "빠름" };
    private readonly string[] MISSION_NUM_STRING_KOR = { "적음", "보통", "많음", "매우 많음" };

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

    [Header("Meeting and vote setting")]
    [SerializeField] private Text meetingTimeText = null;
    [SerializeField] private Text voteTimeText = null;
    [SerializeField] private Text openVoteText = null;
    [SerializeField] private Text emergencyMeetingCooldownText = null;

    [Header("Roles and NPC setting")]
    [SerializeField] private RoleData roleData = null;
    [SerializeField] private RectTransform citizenRoleParent = null;
    [SerializeField] private RectTransform mafiaRoleParent = null;
    [SerializeField] private RectTransform neutralRoleParent = null;
    [SerializeField] private GameObject roleGroupPrefab = null;

    private RoleOptionGroup[] citizenRoleGroup = new RoleOptionGroup[0];
    private RoleOptionGroup[] mafiaRoleGroup = new RoleOptionGroup[0];
    private RoleOptionGroup[] neutralRoleGroup = new RoleOptionGroup[0];

    [Header("Movement and sight setting")]
    [SerializeField] private Text sightRangeText = null;
    [SerializeField] private Text moveSpeedText = null;

    [Header("Cooldown and mission setting")]
    [SerializeField] private Text killCooldownText = null;
    [SerializeField] private Text missionCooldownText = null;
    [SerializeField] private Text numNPCMissionText = null;
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
            new Vector2(0f, -1500f),
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

        // Meeting and vote group setting
        meetingTimeText.text = ((int)roomProperties[CustomProperties.MEETING_TIME]).ToString();
        voteTimeText.text = ((int)roomProperties[CustomProperties.VOTE_TIME]).ToString();
        openVoteText.text = OPEN_VOTE_STRING_KOR[(int)roomProperties[CustomProperties.OPEN_VOTE]];
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

        // Movement and sight group setting
        sightRangeText.text = SIGHT_RANGE_STRING_KOR[(int)roomProperties[CustomProperties.SIGHT_RANGE]];
        moveSpeedText.text = MOVE_SPEED_STRING_KOR[(int)roomProperties[CustomProperties.MOVE_SPEED]];

        // Cooldown and mission group setting
        killCooldownText.text = ((int)roomProperties[CustomProperties.KILL_COOLDOWN]).ToString();
        missionCooldownText.text = ((int)roomProperties[CustomProperties.MISSION_COOLDOWN]).ToString();
        numNPCMissionText.text = MISSION_NUM_STRING_KOR[(int)roomProperties[CustomProperties.NUM_NPC_MISSION]];
        numSpecialMissionText.text = MISSION_NUM_STRING_KOR[(int)roomProperties[CustomProperties.NUM_SPECIAL_MISSION]];
    }

    #endregion Methods


    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        GameManager.UI.ClosePopupPanel<GuestRuleSettingPanel>();
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnRoomPropertiesUpdate(PhotonHashTable propertiesThatChanged)
        => SetOptionText(propertiesThatChanged);

    #endregion Photon Events
}
