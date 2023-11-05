using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AlertPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Text alertMessageText = null;
    [SerializeField] private Button confirmButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        confirmButton.onClick.AddListener(OnClickConfirmButton);

        panelRect.localScale = Vector3.zero;

        OnActive += () =>
        {
            confirmButton.interactable = true;
        };
    }

    public override Sequence ActiveAnimation()
    {
        Tween panelTween = panelRect.DOScale(1f, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.OutExpo)
            .OnStart(() =>
                {
                    panelRect.gameObject.SetActive(true);
                    panelRect.localScale = Vector3.zero;
                });

        return DOTween.Sequence().Append(panelTween);
    }

    public override Sequence DeactiveAnimation()
    {
        Tween panelTween = panelRect.DOScale(0f, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.OutExpo)
            .OnComplete(() =>
                {
                    panelRect.gameObject.SetActive(false);
                });

        return DOTween.Sequence().Append(panelTween);
    }

    #endregion Override Methods

    #region Methods

    public void SetAlertMessage(string message)
    {
        alertMessageText.text = message;
    } 

    #endregion Methods

    #region Event Methods

    public void OnClickConfirmButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        confirmButton.interactable = false;
        GameManager.UI.ClosePopupPanel<AlertPanel>();
    }

    #endregion Event Methods
}
