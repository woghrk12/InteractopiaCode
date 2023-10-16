using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class GameStartPanel : UIPanel
{
    #region Variables

    [Header("UI Object")]
    [SerializeField] private Text roleAndTeamText = null;
    [SerializeField] private Text descriptionText = null;
    [SerializeField] private Text numMafiaLeftText = null;

    [Header("Team Group Object")]
    [SerializeField] private Image spotLightImage = null;
    [SerializeField] private RoleGroup citizenGroup = null;
    [SerializeField] private RoleGroup mafiaGroup = null;
    [SerializeField] private RoleGroup neutralGroup = null;

    [Space]
    [SerializeField] private RoleData roleData = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        OnActive += () =>
        {
            int playerTeam = GameManager.InGame.LocalPlayer.PlayerTeam;
            int playerRole = GameManager.InGame.LocalPlayer.PlayerRole;

            // Set the player's team group object
            switch ((ERoleType)playerTeam)
            {
                case ERoleType.CITIZEN:
                    citizenGroup.gameObject.SetActive(true);
                    citizenGroup.SetAllPlayer();

                    mafiaGroup.gameObject.SetActive(false);
                    neutralGroup.gameObject.SetActive(false);
                    break;

                case ERoleType.MAFIA:
                    mafiaGroup.gameObject.SetActive(true);
                    mafiaGroup.SetMafiaGroup();

                    citizenGroup.gameObject.SetActive(false);
                    neutralGroup.gameObject.SetActive(false);
                    break;

                case ERoleType.NEUTRAL:
                    neutralGroup.gameObject.SetActive(true);
                    neutralGroup.SetNeutralGroup(PhotonNetwork.LocalPlayer.ActorNumber);

                    citizenGroup.gameObject.SetActive(false);
                    mafiaGroup.gameObject.SetActive(false);
                    break;

                default:
                    throw new System.Exception($"Cannot find the role type. Input player team index : {playerTeam}.");
            }

            // Get the player's role info
            RoleInfo playerRoleInfo = roleData.GetRoleInfo((ERoleType)playerTeam, playerRole);

            // Set text for indicating how many mafia members are remaining
            numMafiaLeftText.text = $"There are {GameManager.InGame.AliveMafiaPlayerList.Count} Mafia remaining";

            // Set UI based on the player's team and role
            roleAndTeamText.text = $"{playerRoleInfo.RoleName} [ {roleData.GetTeamName(playerTeam)} ]";
            descriptionText.text = playerRoleInfo.Description;
            spotLightImage.color = new Color(playerRoleInfo.RoleColor.r, playerRoleInfo.RoleColor.g, playerRoleInfo.RoleColor.b, spotLightImage.color.a);

            StartCoroutine(DisplayTeamInfo());
        };
        OnDeactive += () =>
        {
            GameManager.InGame.StartWork();
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

    private IEnumerator DisplayTeamInfo()
    {
        yield return Utilities.WaitForSeconds(4f);

        GameManager.UI.ClosePanel<GameStartPanel>();
    }

    #endregion Coroutine Methods
}
