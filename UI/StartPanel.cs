using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartPanel : UIPanel
{
    #region Variables

    [SerializeField] private Image titleImage = null;
    [SerializeField] private Button startButton = null;
    [SerializeField] private Button howToPlayButton = null;
    [SerializeField] private Button creditButton = null;
    [SerializeField] private Button noticeButton = null;
    [SerializeField] private Button quitButton = null;
    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button authButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        settingButton.onClick.AddListener(OnClickSettingButton);
        authButton.onClick.AddListener(OnClickAuthButton);
        quitButton.onClick.AddListener(OnClickQuitButton);
    }

    public override Sequence ActiveAnimation()
    {
        RectTransform titleImageRect = titleImage.GetComponent<RectTransform>();
        Tween titleImageTween = DoTweenUtil.DoAnchoredPos(
            titleImageRect,
            titleImageRect.anchoredPosition + new Vector2(0f, 800f),
            titleImageRect.anchoredPosition,
            1f,
            Ease.OutExpo);

        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        Tween startBittonTween = DoTweenUtil.DoAnchoredPos(
            startButtonRect,
            startButtonRect.anchoredPosition + new Vector2(1000f, 0f),
            startButtonRect.anchoredPosition,
            0.5f,
            Ease.OutExpo);

        RectTransform howToPlayButtonRect = howToPlayButton.GetComponent<RectTransform>();
        Tween howToPlayButtonTween = DoTweenUtil.DoAnchoredPos(
            howToPlayButtonRect,
            howToPlayButtonRect.anchoredPosition + new Vector2(1000f, 0f),
            howToPlayButtonRect.anchoredPosition,
            0.5f,
            Ease.OutExpo);

        RectTransform creditButtonRect = creditButton.GetComponent<RectTransform>();
        Tween creditButtonTween = DoTweenUtil.DoAnchoredPos(
            creditButtonRect,
            creditButtonRect.anchoredPosition + new Vector2(1000f, 0f),
            creditButtonRect.anchoredPosition,
            0.5f,
            Ease.OutExpo);

        RectTransform noticeButtonRect = noticeButton.GetComponent<RectTransform>();
        Tween noticeButtonTween = DoTweenUtil.DoAnchoredPos(
            noticeButtonRect,
            noticeButtonRect.anchoredPosition + new Vector2(1000f, 0f),
            noticeButtonRect.anchoredPosition,
            0.5f,
            Ease.OutExpo);

        RectTransform quitButtonRect = quitButton.GetComponent<RectTransform>();
        Tween quitButtonTween = DoTweenUtil.DoAnchoredPos(
            quitButtonRect,
            quitButtonRect.anchoredPosition + new Vector2(1000f, 0f),
            quitButtonRect.anchoredPosition,
            0.5f,
            Ease.OutExpo);

        return DOTween.Sequence()
            .Append(GameManager.UI.FadeIn(GlobalDefine.fadeEffectDuration))
            .Append(titleImageTween)
            .Append(startBittonTween)
            .Append(howToPlayButtonTween)
            .Join(creditButtonTween)
            .Join(noticeButtonTween)
            .Append(quitButtonTween);
    }

    public override Sequence DeactiveAnimation()
    {
        return DOTween.Sequence().Append(GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration));
    }

    #endregion Override Methods

    #region Event Methods

    public void OnClickStartButton() => GameManager.UI.OpenPanel<LobbyPanel>();

    public void OnClickSettingButton() => GameManager.UI.PopupPanel<SettingPanel>();

    public void OnClickAuthButton() => GameManager.UI.PopupPanel<AuthPanel>();

    public void OnClickQuitButton()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    #endregion Event Methods
}
