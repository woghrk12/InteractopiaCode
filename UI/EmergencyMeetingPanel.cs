using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmergencyMeetingPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;

    [SerializeField] private Button closeButton = null;
    [SerializeField] private CanvasGroup emergencyCallButtonGroup = null;
    [SerializeField] private Text cooldownText = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        emergencyCallButtonGroup.GetComponent<Button>().onClick.AddListener(OnClickCallButton);

        OnActive += () =>
        {
            closeButton.interactable = true;
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

    public void SetCooldownText(float cooldown)
    {
        cooldownText.text = cooldown.ToString("F0");
    }

    public void EnableCall()
    {
        cooldownText.gameObject.SetActive(false);
        emergencyCallButtonGroup.interactable = true;
        emergencyCallButtonGroup.alpha = 1f;
    }
    
    public void BlockCall()
    {
        cooldownText.gameObject.SetActive(true);
        emergencyCallButtonGroup.interactable = false;
        emergencyCallButtonGroup.alpha = 0.2f;
    }

    #endregion Methods

    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;

        GameManager.UI.ClosePopupPanel<EmergencyMeetingPanel>();
    }

    public void OnClickCallButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        GameManager.Meeting.EmergencyCall();
    }

    #endregion Event Methods
}
