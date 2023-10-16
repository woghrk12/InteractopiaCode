using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ReportGroup : MonoBehaviour
{
    #region Variables

    [Header("Component of canvas object")]
    [SerializeField] private Text reportText = null;
    [SerializeField] private UICharacter reporterCharacter = null;
    [SerializeField] private UICharacter deadCharacter = null;

    [Header("Components for npc object")]
    [SerializeField] private RectTransform[] npcObjectRect = null;
    
    [Header("RectTransform for animation")]
    [SerializeField] private RectTransform alertIconRect = null;
    private RectTransform reporterRect = null;
    private RectTransform deadBodyRect = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        reporterRect = reporterCharacter.GetComponent<RectTransform>();
        deadBodyRect = deadCharacter.GetComponent<RectTransform>();
    }

    #endregion Unity Events

    #region Methods

    public void InitGroup(MeetingManager.ECauseMeeting cause, int reporter, int deadBody)
    {
        reporterRect.anchoredPosition = new Vector2(-2000f, 0f);
        deadCharacter.GetComponent<RectTransform>().anchoredPosition = new Vector2(2000f, 0f);
        foreach (RectTransform rect in npcObjectRect)
        {
            rect.anchoredPosition = new Vector2(2000f, 0f);
        }

        alertIconRect.localScale = Vector2.zero;

        reporterCharacter.SetCharacter(reporter);

        if (cause == MeetingManager.ECauseMeeting.DEADBODYREPORT)
        {
            deadBodyRect = deadCharacter.GetComponent<RectTransform>();
            deadCharacter.SetCharacter(deadBody);
        }
        else if (cause == MeetingManager.ECauseMeeting.DEADNPCREPORT)
        {
            deadBodyRect = npcObjectRect[deadBody];
        }

        reportText.text = "Body found!!";
    }

    public Sequence ActiveAnimation()
    {
        Tween deadTween = DoTweenUtil.DoAnchoredPos(
           deadBodyRect,
           new Vector2(2000f, 0f),
           new Vector2(500f, 0f),
           GlobalDefine.panelAnimationDuration,
           Ease.OutExpo);

        Tween reporterTween = DoTweenUtil.DoAnchoredPos(
            reporterRect,
            new Vector2(-2000f, 0f),
            new Vector2(-500f, 0f),
            GlobalDefine.panelAnimationDuration,
            Ease.OutExpo);

        Tween alertTween = alertIconRect.DOScale(1f, GlobalDefine.panelAnimationDuration)
             .SetEase(Ease.OutExpo);

        return DOTween.Sequence()
            .Append(deadTween)
            .Append(reporterTween)
            .Append(alertTween);
    }

    #endregion Methods
}
