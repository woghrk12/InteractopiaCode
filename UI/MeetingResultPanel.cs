using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

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

            SoundManager.Instance.SpawnEffect(ESoundKey.SFX_DOOR_Sci_Fi_Heavy_Close_Lock_Reverb_stereo);

            ShowVoteResult();
        };
        OnDeactive += () => 
        {
            resultText.gameObject.SetActive(false);
            roleNameText.gameObject.SetActive(false);

            if (GameManager.InGame.CheckGameEndByVote())
            {
                GameManager.InGame.EndGame();
            }
            else if (GameManager.InGame.CheckGameEndByAllMafiaDead())
            {
                GameManager.InGame.EndGame();
            }
            else if (GameManager.InGame.CheckGameEndByNumber())
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
        Tween resultTween = resultText.DOText("아무도 추방되지 않았습니다.", 2f)
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

        Sequence sequence = DOTween.Sequence();

        Player kickedPlayer = GameManager.Network.PlayerDictionaryByActorNum[actorNum];
        
        string nickName = kickedPlayer.NickName;
        Tween resultTween = resultText.DOText(nickName + " 님이 추방되었습니다.", 2f)
            .OnStart(() => resultText.gameObject.SetActive(true));

        sequence.Append(resultTween);

        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!(bool)roomSetting[CustomProperties.HIDE_EMISSION_INFO])
        {
            string roleName = roleData.GetRoleInfo(
            (ERoleType)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_TEAM],
            (int)kickedPlayer.CustomProperties[PlayerProperties.PLAYER_ROLE]
            ).RoleName;

            Tween roleNameTween = roleNameText.DOText("그는 " + roleName + "였습니다.", 2f)
                .OnStart(() => roleNameText.gameObject.SetActive(true));

            sequence.Append(roleNameTween);
        }
        
        sequence.AppendInterval(5f)
            .OnComplete(() => GameManager.UI.ClosePanel<MeetingResultPanel>())
            .Play();

        resultCharacter.SetCharacter(actorNum, EUICharacterType.BODY);
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
