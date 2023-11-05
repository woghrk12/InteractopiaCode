using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;  
using DG.Tweening;

public class EndingPanel : UIPanel
{
    #region Variables

    [Header("UI Object")]
    [SerializeField] private Text winnerTeamText = null;

    [Header("Team Group Object")]
    [SerializeField] private Image spotLightImage = null;
    [SerializeField] private RoleGroup citizenGroup = null;
    [SerializeField] private RoleGroup mafiaGroup = null;
    [SerializeField] private RoleGroup neutralGroup = null;

    private RoleData roleData = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        roleData = GameManager.Resource.Load<RoleData>("Data/RoleData");

        OnActive += () =>
        {
            // Set winner team info
            ERoleType winnerTeam = GameManager.InGame.WinnerTeam;
            int winnerActorNumber = GameManager.InGame.WinnerActorNumber;

            switch (winnerTeam)
            {
                case ERoleType.CITIZEN:
                    citizenGroup.gameObject.SetActive(true);
                    citizenGroup.SetCitizenGroup();

                    mafiaGroup.gameObject.SetActive(false);
                    neutralGroup.gameObject.SetActive(false);
                    break;

                case ERoleType.MAFIA:
                    mafiaGroup.gameObject.SetActive(true);
                    mafiaGroup.SetMafiaGroup(false);

                    citizenGroup.gameObject.SetActive(false);
                    neutralGroup.gameObject.SetActive(false);
                    break;

                case ERoleType.NEUTRAL:
                    neutralGroup.gameObject.SetActive(true);
                    neutralGroup.SetNeutralGroup(winnerActorNumber);

                    citizenGroup.gameObject.SetActive(false);
                    mafiaGroup.gameObject.SetActive(false);
                    break;

                default:
                    throw new System.Exception($"Cannot find the role type. Input player team index : {winnerTeam}.");
            }

            // Get the winner's role info
            if (winnerTeam != ERoleType.NEUTRAL)
            {
                RoleInfo roleInfo = roleData.GetRoleInfo(winnerTeam, -1);
                winnerTeamText.text = $"[ {roleData.GetTeamName((int)winnerTeam)} ] ½Â¸®";
                spotLightImage.color = new Color(roleInfo.RoleColor.r, roleInfo.RoleColor.g, roleInfo.RoleColor.b, spotLightImage.color.a);
            }
            else
            {
                Player winnerPlayer = GameManager.Network.PlayerDictionaryByActorNum[winnerActorNumber];
                RoleInfo roleInfo = roleData.GetRoleInfo((ERoleType)winnerPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM], (int)winnerPlayer.CustomProperties[PlayerProperties.PLAYER_ROLE]);
                winnerTeamText.text = $"[ {roleInfo.RoleName} ] ½Â¸®";
                spotLightImage.color = new Color(roleInfo.RoleColor.r, roleInfo.RoleColor.g, roleInfo.RoleColor.b, spotLightImage.color.a);
            }

            StartCoroutine(DisplayResult());
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

    #region Coroutine Methods

    private IEnumerator DisplayResult()
    {
        yield return Utilities.WaitForSeconds(3f);
        
        Tween fadeOutTween = GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration);
        fadeOutTween.onComplete += () =>
            {
                GameManager.UI.ClosePanel<EndingPanel>(false);
                GameManager.InGame.ReturnToRoom();
            };

        fadeOutTween.Play();
    }

    #endregion Coroutine Methods
}
