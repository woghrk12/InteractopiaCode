using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MapList : MonoBehaviour
{
    #region Variables

    private int selectedIdx = 0;

    [SerializeField] private Button previousMapButton = null;
    [SerializeField] private Button postMapButton = null;

    [SerializeField] private float rotateDuration = 1.5f;
    [SerializeField] private Image[] mapImageArray = null;

    #endregion Variables

    #region Properties

    public int SelectedIdx => selectedIdx;

    #endregion Properties

    #region Unity Events

    private void Awake()
    {
        previousMapButton.onClick.AddListener(OnClickPreviousMapButton);
        postMapButton.onClick.AddListener(OnClickPostMapButton);
    }

    #endregion Unity Events

    #region Methods

    public void InitList()
    {
        previousMapButton.interactable = true;
        postMapButton.interactable = true;
        selectedIdx = 0;
    }

    #endregion Methods

    #region Event Methods

    public void OnClickPreviousMapButton()
    {
        RectTransform curRect = mapImageArray[selectedIdx % 5].rectTransform;
        RectTransform postRect = mapImageArray[(selectedIdx + 1) % 5].rectTransform;
        RectTransform postHideRect = mapImageArray[(selectedIdx + 2) % 5].rectTransform;
        RectTransform previousHideRect = mapImageArray[(selectedIdx + 3) % 5].rectTransform;
        RectTransform previousRect = mapImageArray[(selectedIdx + 4) % 5].rectTransform;

        DOTween.Sequence()
            .OnStart(() => 
                {
                    previousMapButton.interactable = false;
                    postMapButton.interactable = false;
                    selectedIdx--; 
                })
            .OnComplete(() =>
                {
                    previousMapButton.interactable = true;
                    postMapButton.interactable = true;
                })
            .Append(curRect.DOAnchorPosX(previousRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(curRect.DOScale(previousRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousRect.DOAnchorPosX(previousHideRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousRect.DOScale(previousHideRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousHideRect.DOAnchorPosX(postHideRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousHideRect.DOScale(postHideRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postHideRect.DOAnchorPosX(postRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postHideRect.DOScale(postRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postRect.DOAnchorPosX(curRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postRect.DOScale(curRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Play();
    }

    public void OnClickPostMapButton()
    {
        RectTransform curRect = mapImageArray[selectedIdx % 5].rectTransform;
        RectTransform postRect = mapImageArray[(selectedIdx + 1) % 5].rectTransform;
        RectTransform postHideRect = mapImageArray[(selectedIdx + 2) % 5].rectTransform;
        RectTransform previousHideRect = mapImageArray[(selectedIdx + 3) % 5].rectTransform;
        RectTransform previousRect = mapImageArray[(selectedIdx + 4) % 5].rectTransform;

        DOTween.Sequence()
            .OnStart(() =>
            {
                previousMapButton.interactable = false;
                postMapButton.interactable = false;
                selectedIdx++;
            })
            .OnComplete(() =>
            {
                previousMapButton.interactable = true;
                postMapButton.interactable = true;
            })
            .Append(curRect.DOAnchorPosX(postRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(curRect.DOScale(postRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postRect.DOAnchorPosX(postHideRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postRect.DOScale(postHideRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postHideRect.DOAnchorPosX(previousHideRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(postHideRect.DOScale(previousHideRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousHideRect.DOAnchorPosX(previousRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousHideRect.DOScale(previousRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousRect.DOAnchorPosX(curRect.anchoredPosition.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Join(previousRect.DOScale(curRect.localScale.x, rotateDuration))
            .SetEase(Ease.OutExpo)
            .Play();
    }

    #endregion Event Methods
}
