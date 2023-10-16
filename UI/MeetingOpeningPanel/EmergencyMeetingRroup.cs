using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmergencyMeetingRroup : MonoBehaviour
{
    #region Variables

    [Header("Component of canvas object")]
    [SerializeField] private Text reportText = null;
    [SerializeField] private UICharacter reporterCharacter = null;

    [Header("RectTransform for animation")]
    private RectTransform reporterRect = null;
    [SerializeField] private RectTransform alertIconRect = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        reporterRect = reporterCharacter.GetComponent<RectTransform>();
    }

    #endregion Unity Events

    #region Methods

    public void InitGroup(int reporter)
    {
        reporterCharacter.SetCharacter(reporter);
        
        reportText.text = "Emergency meeting called!!";

        reporterRect.anchoredPosition = new Vector2(-2000f, 0f);
        alertIconRect.localScale = Vector2.zero;
    }

    public Sequence ActiveAnimation()
    {
        Tween reporterTween = DoTweenUtil.DoAnchoredPos(
            reporterRect,
            new Vector2(-2000f, 0f),
            Vector2.zero,
            GlobalDefine.panelAnimationDuration,
            Ease.OutExpo);

        Tween alertTween = alertIconRect.DOScale(1f, GlobalDefine.panelAnimationDuration)
             .SetEase(Ease.OutExpo);

        return DOTween.Sequence()
            .Append(reporterTween)
            .Append(alertTween);
    }

    #endregion Methods
}
