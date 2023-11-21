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
    }

    #endregion Unity Events

    #region Methods

    public void InitGroup(MeetingManager.ECauseMeeting cause, int reporter, int deadBody)
    {
        reporterRect.anchoredPosition = new Vector2(-2000f, 0f);
        deadCharacter.gameObject.SetActive(false);
        foreach (RectTransform rect in npcObjectRect)
        {
            rect.gameObject.SetActive(false);
        }

        alertIconRect.localScale = Vector2.zero;

        reporterCharacter.SetCharacter(reporter, EUICharacterType.BODY);

        if (cause == MeetingManager.ECauseMeeting.DEADBODYREPORT)
        {
            deadBodyRect = deadCharacter.GetComponent<RectTransform>();
            deadCharacter.SetCharacter(deadBody, EUICharacterType.DEAD);
        }
        else if (cause == MeetingManager.ECauseMeeting.DEADNPCREPORT)
        {
            deadBodyRect = npcObjectRect[deadBody];
        }

        deadBodyRect.gameObject.SetActive(true);
        reportText.text = "시체 발견";
    }

    public Sequence ActiveAnimation()
    {
        Tween deadTween = DoTweenUtil.DoAnchoredPosX(
           deadBodyRect,
           2000f,
           500f,
           GlobalDefine.panelAnimationDuration,
           Ease.OutExpo);

        Tween reporterTween = DoTweenUtil.DoAnchoredPosX(
            reporterRect,
            -2000f,
            -500f,
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
