using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InGamePanel : UIPanel
{
    #region Variables

    [SerializeField] private PlayerStickInput joystick = null;

    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button voiceChattingButton = null;
    [SerializeField] private Button minimapButton = null;

    [SerializeField] private GameObject voiceBlockImageObject = null;

    [SerializeField] private RectTransform buttonGroupParent = null;
    [SerializeField] private RoleData roleData = null;

    #endregion Variables

    #region Properties

    public Button VoiceChattingButton => voiceChattingButton;

    #endregion Properties

    #region Override Methods

    public override void InitPanel()
    {
        InputManager inputManager = GameManager.Input;
        GameManager.Input.PlayerStickInput = joystick;

        // Instantiate the player button according to player's role
        PlayerButton useButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
        useButton.InitButton(GameManager.Resource.Load<Sprite>("UI/Icon/UseIcon"), new Color(0.3f, 0.7f, 1f, 1f));
        GameManager.Input.UseButton = useButton;

        PlayerButton reportButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
        reportButton.InitButton(GameManager.Resource.Load<Sprite>("UI/Icon/ReportIcon"), new Color(0.3f, 0.7f, 1f, 1f));
        GameManager.Input.ReportButton = reportButton;

        int playerTeam = GameManager.InGame.LocalPlayer.PlayerTeam;
        int playerRole = GameManager.InGame.LocalPlayer.PlayerRole;
        RoleInfo playerRoleInfo = roleData.GetRoleInfo((ERoleType)playerTeam, playerRole);

        if (playerTeam == (int)ERoleType.CITIZEN)
        {
            if (playerRole != (int)ECitizenRole.NONE)
            {
                PlayerButton skillButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
                skillButton.InitButton(playerRoleInfo.RoleIconSprite, playerRoleInfo.RoleColor);
                GameManager.Input.SkillButton = skillButton;
            }
        }
        else if (playerTeam == (int)ERoleType.MAFIA)
        {
            if (playerRole != (int)EMafiaRole.MANIPULATOR)
            {
                PlayerButton killButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
                killButton.InitButton(GameManager.Resource.Load<Sprite>("UI/Icon/KillIcon"), new Color(1f, 0.4f, 0.3f, 1f));
                GameManager.Input.KillButton = killButton;
            }
            if (playerRole != (int)EMafiaRole.NONE && playerRole != (int)EMafiaRole.ASSASSIN)
            {
                PlayerButton skillButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
                skillButton.InitButton(playerRoleInfo.RoleIconSprite, playerRoleInfo.RoleColor);
                GameManager.Input.SkillButton = skillButton;
            }
        }
        else
        {
            if (playerRole != (int)ENeutralRole.ROGUE)
            {
                PlayerButton skillButton = GameManager.Resource.Instantiate("UI/InGamePanel/PlayerButton", buttonGroupParent).GetComponent<PlayerButton>();
                skillButton.InitButton(playerRoleInfo.RoleIconSprite, playerRoleInfo.RoleColor);
                GameManager.Input.SkillButton = skillButton;
            }
        }

        // Add event for player buttons
        settingButton.onClick.AddListener(OnClickSettingButton);
        voiceChattingButton.onClick.AddListener(OnClickVoiceChattingButton);
        minimapButton.onClick.AddListener(OnClickMinimapButton);

        if (inputManager.KillButton != null)
        {
            GameManager.InGame.LocalPlayer.killCooldownStartEvent += () =>
            {
                inputManager.KillButton.IsInteractable = false;
                inputManager.KillButton.SetActiveText(true);
            };
            GameManager.InGame.LocalPlayer.killCooldownTickEvent += inputManager.KillButton.SetCooldownText;
            GameManager.InGame.LocalPlayer.killCooldownEndEvent += (bool isInteractable) =>
            {
                inputManager.KillButton.SetActiveText(false);
                inputManager.KillButton.IsInteractable = isInteractable;
            };
        }
        if (inputManager.SkillButton != null)
        {
            GameManager.InGame.LocalPlayer.skillCooldownStartEvent += () =>
            {
                inputManager.SkillButton.IsInteractable = false;
                inputManager.SkillButton.SetActiveText(true);
            };
            GameManager.InGame.LocalPlayer.skillCooldownTickEvent += inputManager.SkillButton.SetCooldownText;
            GameManager.InGame.LocalPlayer.skillCooldownEndEvent += (bool isInteractable) =>
            {
                inputManager.SkillButton.SetActiveText(false);
                inputManager.SkillButton.IsInteractable = isInteractable;
            };
        }

        OnActive += () =>
        {
            // Check the player's input device
            joystick.gameObject.SetActive(GameManager.Input.InputMode == InputManager.EInputMode.GAMEPAD);

            voiceBlockImageObject.SetActive(GameManager.Vivox.IsMute);
        };
        OnDeactive += () =>
        {
            // Stop the character
            GameManager.Input.StopMove();
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

    public void OnClickMinimapButton()
    {
        GameManager.UI.GetPanel<MinimapPanel>().OpenCause = EMinimapOpenCause.NONE;
        GameManager.UI.PopupPanel<MinimapPanel>();
    }

    #endregion Event Methods
}
