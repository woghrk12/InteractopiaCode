using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MeetingOpeningPanel : UIPanel
{
    #region Variables

    [Header("Component of canvas object")]
    [SerializeField] private Image backgroundImage = null;

    [Header("Group object of each situation")]
    [SerializeField] private ReportGroup reportGroup = null;
    [SerializeField] private EmergencyMeetingRroup emergencyMeetingGroup = null;

    [Header("RectTransform for animation")]
    private RectTransform backgroundRect = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        backgroundRect = backgroundImage.GetComponent<RectTransform>();

        OnActive += () =>
        {
            GameManager.Input.StopMove();

            MeetingManager meetingManager = GameManager.Meeting;
            switch (meetingManager.CauseMeeting)
            {
                case MeetingManager.ECauseMeeting.DEADBODYREPORT:
                case MeetingManager.ECauseMeeting.DEADNPCREPORT:
                    reportGroup.gameObject.SetActive(true);
                    emergencyMeetingGroup.gameObject.SetActive(false);

                    reportGroup.InitGroup(meetingManager.CauseMeeting, meetingManager.Reporter, meetingManager.DeadBody);
                    break;

                case MeetingManager.ECauseMeeting.EMERGENCYMEETINGCALL:
                    reportGroup.gameObject.SetActive(false);
                    emergencyMeetingGroup.gameObject.SetActive(true);

                    emergencyMeetingGroup.InitGroup(meetingManager.Reporter);
                    break;

            }

            StartCoroutine(OpenMeetingPanel());
        };
    }

    public override Sequence ActiveAnimation()
    {
        Tween backgroundTween = backgroundRect.DOScaleY(1f, 0.1f)
            .SetEase(Ease.Linear)
            .OnStart(() => backgroundRect.localScale = new Vector3(1f, 0f, 1f));

        Sequence activeSequence = DOTween.Sequence()
            .Append(backgroundTween);

        switch (GameManager.Meeting.CauseMeeting)
        {
            case MeetingManager.ECauseMeeting.DEADBODYREPORT:
            case MeetingManager.ECauseMeeting.DEADNPCREPORT:
                activeSequence.Append(reportGroup.ActiveAnimation());
                break;

            case MeetingManager.ECauseMeeting.EMERGENCYMEETINGCALL:
                activeSequence.Append(emergencyMeetingGroup.ActiveAnimation());
                break;
        }

        return activeSequence;
    }

    public override Sequence DeactiveAnimation()
    {
        return DOTween.Sequence()
            .Append(GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration));
    }

    #endregion Override Methods

    #region Coroutine Methods

    private IEnumerator OpenMeetingPanel()
    {
        yield return Utilities.WaitForSeconds(5f);

        GameManager.Meeting.StartMeeting();
    }

    #endregion Coroutine Methods
}
