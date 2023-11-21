using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DescriptionPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;

    [SerializeField] private GameObject[] descriptions = null;

    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button previousButton = null;
    [SerializeField] private Button nextButton = null;

    private int descriptionIndex = 0;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        previousButton.onClick.AddListener(OnClickPreviousButton);
        nextButton.onClick.AddListener(OnClickNextButton);

        OnActive += () =>
        {
            closeButton.interactable = true;
            previousButton.interactable = false;
            nextButton.interactable = true;

            descriptionIndex = 0;

            foreach (GameObject description in descriptions)
            {
                description.SetActive(false);
            }

            descriptions[descriptionIndex].SetActive(true);
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

    #region Event Methods

    public void OnClickCloseButton()
    {
        closeButton.interactable = false;
        previousButton.interactable = false;
        nextButton.interactable = false;

        GameManager.UI.ClosePopupPanel<DescriptionPanel>();
    }

    public void OnClickPreviousButton()
    {
        descriptions[descriptionIndex].SetActive(false);
        descriptions[--descriptionIndex].SetActive(true);

        if (descriptionIndex <= 0)
        {
            previousButton.interactable = false;
        }

        nextButton.interactable = true;
    }

    public void OnClickNextButton()
    {
        descriptions[descriptionIndex].SetActive(false);
        descriptions[++descriptionIndex].SetActive(true);

        if (descriptionIndex >= descriptions.Length - 1)
        {
            nextButton.interactable = false;
        }

        previousButton.interactable = true;
    }

    #endregion Event Methods
}
