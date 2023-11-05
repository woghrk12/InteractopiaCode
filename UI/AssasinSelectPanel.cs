using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

public class AssasinSelectPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;

    [Header("Components for selecting the player")]
    [SerializeField] private Button playerSelectButton = null;
    [SerializeField] private GameObject playerSelectGroup = null;
    [SerializeField] private RectTransform playerButtonParent = null;
    private Dictionary<int, PlayerSelectButton> playerSelectDictionary = new();

    [Header("Components for selecting the role")]
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private Button roleSelectButton = null;
    [SerializeField] private UICharacter selectedPlayer = null;
    [SerializeField] private GameObject roleSelectGroup = null;
    [SerializeField] private RectTransform roleButtonParent = null;
    
    [Header("Selected buttons")]
    private PlayerSelectButton playerSelectedButton = null;
    private RoleSelectButton roleSelectedButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        playerSelectButton.onClick.AddListener(OnClickPlayerSelectButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
        roleSelectButton.onClick.AddListener(OnClickRoleSelectButton);

        // Instantiate the button of players
        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            if (player.Key == PhotonNetwork.LocalPlayer.ActorNumber) continue;
            
            PlayerSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/PlayerSelectButton", playerButtonParent).GetComponent<PlayerSelectButton>();
            button.InitButton(this, player.Key);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(playerButtonParent);

        // Instantiate the button of all roles
        RoleData roleData = GameManager.Resource.Load<RoleData>("Data/RoleData");
        // Citizen roles
        RoleInfo defaultCitizenRoleInfo = roleData.GetRoleInfo(ERoleType.CITIZEN, -1);
        {
            RoleSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/RoleSelectButton", roleButtonParent).GetComponent<RoleSelectButton>();
            button.InitButton(this, defaultCitizenRoleInfo);
        }
        RoleInfo[] citizenRoleInfos = roleData.GetAllRoleInfos(ERoleType.CITIZEN);
        foreach (RoleInfo role in citizenRoleInfos)
        {
            RoleSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/RoleSelectButton", roleButtonParent).GetComponent<RoleSelectButton>();
            button.InitButton(this, role);
        }
        // Mafia roles
        RoleInfo defaultMafiaRoleInfo = roleData.GetRoleInfo(ERoleType.MAFIA, -1);
        {
            RoleSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/RoleSelectButton", roleButtonParent).GetComponent<RoleSelectButton>();
            button.InitButton(this, defaultMafiaRoleInfo);
        }
        RoleInfo[] mafiaRoleInfos = roleData.GetAllRoleInfos(ERoleType.MAFIA);
        foreach (RoleInfo role in mafiaRoleInfos)
        {
            RoleSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/RoleSelectButton", roleButtonParent).GetComponent<RoleSelectButton>();
            button.InitButton(this, role);
        }
        // Neutral roles
        RoleInfo[] neutralRoleInfos = roleData.GetAllRoleInfos(ERoleType.NEUTRAL);
        foreach (RoleInfo role in neutralRoleInfos)
        {
            RoleSelectButton button = GameManager.Resource.Instantiate("UI/AssasinSelectPanel/RoleSelectButton", roleButtonParent).GetComponent<RoleSelectButton>();
            button.InitButton(this, role);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(roleButtonParent);

        OnActive += () =>
        {
            closeButton.interactable = true;
            playerSelectButton.interactable = false;
            cancelButton.interactable = true;
            roleSelectButton.interactable = false;

            // Initialize the player select group
            playerSelectGroup.SetActive(true);
            List<int> deadPlayerList = GameManager.InGame.DeadPlayerList;
            foreach (int player in deadPlayerList)
            {
                playerSelectDictionary[player].SetDisableButton();
            }

            // Initialize the role select group
            roleSelectGroup.SetActive(false);
        };
        OnDeactive += () =>
        {
            // Deactivate the player select button
            if (playerSelectedButton != null)
            {
                playerSelectedButton.SetActiveButton(false);
                playerSelectedButton = null;
            }

            // Deactivate the role select button
            if (roleSelectedButton != null)
            {
                roleSelectedButton.SetActiveButton(false);
                roleSelectedButton = null;
            }
        };
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

    public void SelectPlayer(PlayerSelectButton selectedButton)
    {
        if (playerSelectedButton != null)
        {
            playerSelectedButton.SetActiveButton(false);
        }

        playerSelectedButton = selectedButton;
        playerSelectedButton.SetActiveButton(true);

        playerSelectButton.interactable = true;
    }

    public void SelectRole(RoleSelectButton selectedButton)
    {
        if (roleSelectedButton != null)
        {
            roleSelectedButton.SetActiveButton(false);
        }

        roleSelectedButton = selectedButton;
        roleSelectedButton.SetActiveButton(true);

        roleSelectButton.interactable = true;
    } 

    #endregion Methods

    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        playerSelectButton.interactable = false;
        cancelButton.interactable = false;
        roleSelectButton.interactable = false;

        GameManager.UI.ClosePopupPanel<AssasinSelectPanel>();
    }

    public void OnClickPlayerSelectButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        playerSelectGroup.SetActive(false);
        roleSelectGroup.SetActive(true);

        selectedPlayer.SetCharacter(playerSelectedButton.ActorNumber, EUICharacterType.HEAD);
    }

    public void OnClickCancelButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        playerSelectGroup.SetActive(true);
        roleSelectGroup.SetActive(false);
    }

    public void OnClickRoleSelectButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        (GameManager.InGame.LocalPlayer as AssasinPlayer).Assasinate(
            playerSelectedButton.ActorNumber,
            roleSelectedButton.Team,
            roleSelectedButton.Role
            );

        GameManager.UI.ClosePopupPanel<AssasinSelectPanel>();
    }

    #endregion Event Methods

    #region Photon Events

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int actorNumber = otherPlayer.ActorNumber;

        if (roleSelectGroup.activeSelf)
        {
            if (roleSelectedButton != null)
            {
                roleSelectedButton.SetActiveButton(false);
                roleSelectedButton = null;

                roleSelectButton.interactable = false;
            }

            roleSelectGroup.SetActive(false);
            playerSelectGroup.SetActive(true);
        }

        if (playerSelectedButton != null && playerSelectedButton.ActorNumber == actorNumber)
        {
            playerSelectedButton.SetActiveButton(false);
            playerSelectedButton = null;

            playerSelectButton.interactable = false;
        }

        playerSelectDictionary[otherPlayer.ActorNumber].SetDisableButton();
    }

    #endregion Photon Events
}
