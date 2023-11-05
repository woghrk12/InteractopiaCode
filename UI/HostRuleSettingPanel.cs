using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class HostRuleSettingPanel : UIPanel
{
    private enum EGroup { NUMOFPLAYER, PLAYRULE, ROLES, TIMES }

    #region Variables

    private PhotonHashTable roomSetting = null;

    [SerializeField] private RectTransform panelRect = null;

    [SerializeField] private Button closeButton = null;

    [Header("Buttons for opening groups")]
    [SerializeField] private Button commonButton = null;
    [SerializeField] private Button meetingAndVoteButton = null;
    [SerializeField] private Button rolesAndNPCButton = null;
    [SerializeField] private Button movementAndSightButton = null;
    [SerializeField] private Button cooltimeAndMissionButton = null;

    [Header("Setting groups")]
    [SerializeField] private CommonGroup commonGroup = null;
    [SerializeField] private MeetingAndVoteGroup meetingAndVoteGroup = null;
    [SerializeField] private RolesAndNPCGroup rolesAndNPCGroup = null;
    [SerializeField] private MovementAndSightGroup movementAndSightGroup = null;
    [SerializeField] private CooldownAndMissionGroup cooltimeAndMissionGroup = null;

    [Space]
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private Button confirmButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        commonButton.onClick.AddListener(OnClickCommonButton);
        meetingAndVoteButton.onClick.AddListener(OnClickMeetingAndVoteButton);
        rolesAndNPCButton.onClick.AddListener(OnClickRolesAndNPCButton);
        movementAndSightButton.onClick.AddListener(OnClickMovementAndSightButton);
        cooltimeAndMissionButton.onClick.AddListener(OnClickCooltimeAndMissionButton);

        cancelButton.onClick.AddListener(OnClickCancelButton);
        confirmButton.onClick.AddListener(OnClickConfirmButton);

        rolesAndNPCGroup.InitGroup();

        commonGroup.gameObject.SetActive(true);
        meetingAndVoteGroup.gameObject.SetActive(false);
        rolesAndNPCGroup.gameObject.SetActive(false);
        movementAndSightGroup.gameObject.SetActive(false);
        cooltimeAndMissionGroup.gameObject.SetActive(false);

        OnActive += (() =>
        {
            closeButton.interactable = true;
            confirmButton.interactable = true;

            roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

            commonGroup.SetGroup(roomSetting);
            meetingAndVoteGroup.SetGroup(roomSetting);
            rolesAndNPCGroup.SetGroup(roomSetting);
            movementAndSightGroup.SetGroup(roomSetting);
            cooltimeAndMissionGroup.SetGroup(roomSetting);
        });
        OnDeactive += (() =>
        {
            roomSetting = null;
        });
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

    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        confirmButton.interactable = false;

        GameManager.UI.ClosePopupPanel<HostRuleSettingPanel>();
    }

    public void OnClickCommonButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        commonGroup.gameObject.SetActive(true);

        meetingAndVoteGroup.gameObject.SetActive(false);
        rolesAndNPCGroup.gameObject.SetActive(false);
        movementAndSightGroup.gameObject.SetActive(false);
        cooltimeAndMissionGroup.gameObject.SetActive(false);
    }

    public void OnClickMeetingAndVoteButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        meetingAndVoteGroup.gameObject.SetActive(true);

        commonGroup.gameObject.SetActive(false);
        rolesAndNPCGroup.gameObject.SetActive(false);
        movementAndSightGroup.gameObject.SetActive(false);
        cooltimeAndMissionGroup.gameObject.SetActive(false);
    }

    public void OnClickRolesAndNPCButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        rolesAndNPCGroup.gameObject.SetActive(true);

        commonGroup.gameObject.SetActive(false);
        meetingAndVoteGroup.gameObject.SetActive(false);
        movementAndSightGroup.gameObject.SetActive(false);
        cooltimeAndMissionGroup.gameObject.SetActive(false);
    }

    public void OnClickMovementAndSightButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        movementAndSightGroup.gameObject.SetActive(true);

        commonGroup.gameObject.SetActive(false);
        meetingAndVoteGroup.gameObject.SetActive(false);
        rolesAndNPCGroup.gameObject.SetActive(false);
        cooltimeAndMissionGroup.gameObject.SetActive(false);
    }

    public void OnClickCooltimeAndMissionButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        cooltimeAndMissionGroup.gameObject.SetActive(true);

        commonGroup.gameObject.SetActive(false);
        meetingAndVoteGroup.gameObject.SetActive(false);
        rolesAndNPCGroup.gameObject.SetActive(false);
        movementAndSightGroup.gameObject.SetActive(false);
    }

    public void OnClickCancelButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.UI.ClosePopupPanel<HostRuleSettingPanel>();
    }

    public void OnClickConfirmButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        PhotonHashTable commonSetting = commonGroup.OptionSetting;
        foreach (var option in commonSetting)
        {
            if (!roomSetting.ContainsKey(option.Key))
            {
                roomSetting.Add(option.Key, option.Value);
                continue;
            }

            roomSetting[option.Key] = option.Value;
        }

        PhotonHashTable meetingAndVoteSetting = meetingAndVoteGroup.OptionSetting;
        foreach (var option in meetingAndVoteSetting)
        {
            if (!roomSetting.ContainsKey(option.Key))
            {
                roomSetting.Add(option.Key, option.Value);
                continue;
            }

            roomSetting[option.Key] = option.Value;
        }

        PhotonHashTable rolesAndNPCSetting = rolesAndNPCGroup.OptionSetting;
        foreach (var option in rolesAndNPCSetting)
        {
            if (!roomSetting.ContainsKey(option.Key))
            {
                roomSetting.Add(option.Key, option.Value);
                continue;
            }

            roomSetting[option.Key] = option.Value;
        }

        PhotonHashTable movementAndSightSetting = movementAndSightGroup.OptionSetting;
        foreach (var option in movementAndSightSetting)
        {
            if (!roomSetting.ContainsKey(option.Key))
            {
                roomSetting.Add(option.Key, option.Value);
                continue;
            }

            roomSetting[option.Key] = option.Value;
        }

        PhotonHashTable cooltimeAndMissionSetting = cooltimeAndMissionGroup.OptionSetting;
        foreach (var option in cooltimeAndMissionSetting)
        {
            if (!roomSetting.ContainsKey(option.Key))
            {
                roomSetting.Add(option.Key, option.Value);
                continue;
            }

            roomSetting[option.Key] = option.Value;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSetting);

        GameManager.UI.ClosePopupPanel<HostRuleSettingPanel>();
    }

    #endregion Event Methods
}
