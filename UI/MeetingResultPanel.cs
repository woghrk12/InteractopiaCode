using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

public class MeetingResultPanel : UIPanel
{
    #region Variables

    [SerializeField] private UICharacter resultCharacter = null;

    [Header("Text component")]
    [SerializeField] private Text resultText = null;
    [SerializeField] private Text roleNameText = null;

    [Space]
    [SerializeField] private RoleData roleData = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        resultText.gameObject.SetActive(false);
        roleNameText.gameObject.SetActive(false);

        OnActive += () =>
        {
            resultText.text = string.Empty;
            roleNameText.text = string.Empty;
            ShowVoteResult();
        };
        OnDeactive += () => 
        {
            resultText.gameObject.SetActive(false);
            roleNameText.gameObject.SetActive(false);

            if (GameManager.InGame.CheckGameEnd())
            {
                GameManager.InGame.EndGame();
            }
            else 
            {
                GameManager.InGame.StartWork();
            }
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

    private void ShowVoteResult()
    {
        if (GameManager.Meeting.IsSkipMeeting)
        {
            SkipMeeting();
        }
        else
        {
            KickPlayer(GameManager.Meeting.KickedPlayerActorNum);
        }
    }

    private void SkipMeeting()
    {
        Tween resultTween = resultText.DOText("No one has been kicked.", 2f)
            .OnStart(() => resultText.gameObject.SetActive(true));

        DOTween.Sequence()
            .Append(resultTween)
            .AppendInterval(6f)
            .OnComplete(() => GameManager.UI.ClosePanel<MeetingResultPanel>())
            .Play();
    }

    private void KickPlayer(int actorNum)
    {
        GameManager.InGame.CharacterObjectDictionary[actorNum].Die();
        
        Player kickedPlayer = GameManager.Network.PlayerDictionaryByActorNum[actorNum];
        string nickName = kickedPlayer.NickName;
        string roleName = roleData.GetRoleInfo(
            (ERoleType)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM], 
            (int)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_ROLE]
            ).RoleName;
        
        Tween resultTween = resultText.DOText(nickName + " has been kicked.", 2f)
            .OnStart(() => resultText.gameObject.SetActive(true));

        Tween roleNameTween = roleNameText.DOText("He was the " + roleName + ".", 2f)
            .OnStart(() => roleNameText.gameObject.SetActive(true));

        DOTween.Sequence()
            .Append(resultTween)
            .Append(roleNameTween)
            .AppendInterval(5f)
            .OnComplete(() => GameManager.UI.ClosePanel<MeetingResultPanel>())
            .Play();

        resultCharacter.SetCharacter(actorNum);
        RectTransform resultCharacterRect = resultCharacter.GetComponent<RectTransform>();

        Tween characterMoveTween = DoTweenUtil.DoAnchoredPos(
            resultCharacterRect,
            new Vector2(-2000f, -150f),
            new Vector2(2000f, -150f),
            4f,
            Ease.Linear)
            .Play();

        Tween characterRotateTween = resultCharacterRect.DORotate(new Vector3(0f, 0f, 360f), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .Play();
    }

    #endregion Methods
}
