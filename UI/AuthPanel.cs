using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

public class AuthPanel : UIPanel
{
    #region Varibles

    [SerializeField] private Button closeButton = null;
    [SerializeField] private RectTransform infoPanelRect = null;
    [SerializeField] private Button setNicknameButton = null;
    [SerializeField] private Text nicknameValueText = null;

    [SerializeField] private GameObject setNicknameBlockImage = null;
    [SerializeField] private RectTransform setNicknamePanelRect = null;
    [SerializeField] private GameObject setNicknamePanel = null;
    [SerializeField] private InputField nicknameInputfield = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private Button confirmButton = null;

    #endregion Varibles

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        setNicknameButton.onClick.AddListener(OnClickSetNicknameButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
        confirmButton.onClick.AddListener(OnClickConfirmButton);

        nicknameInputfield.onEndEdit.AddListener(OnNicknameEndEdit);

        infoPanelRect.anchoredPosition = new Vector2(-infoPanelRect.sizeDelta.x, infoPanelRect.anchoredPosition.y);
        setNicknamePanelRect.localScale = Vector3.zero;

        setNicknamePanel.SetActive(false);
        setNicknameBlockImage.SetActive(false);

        OnActive += () =>
        {
            nicknameValueText.text = PhotonNetwork.LocalPlayer.NickName;
            closeButton.interactable = true;
        };
    }

    public override Sequence ActiveAnimation()
    {
        Tween panelTween = infoPanelRect.DOAnchorPosX(0f, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.OutExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    public override Sequence DeactiveAnimation()
    {
        Tween panelTween = infoPanelRect.DOAnchorPosX(-infoPanelRect.sizeDelta.x, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.InExpo);

        return DOTween.Sequence().Append(panelTween);
    }

    #endregion Override Methods

    #region Methods

    private void OpenSetNicknamePanel()
    {
        Tween panelTween = setNicknamePanelRect.DOScale(1f, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.OutExpo)
            .OnStart(() =>
            {
                setNicknameBlockImage.SetActive(true);
                setNicknamePanel.SetActive(true);
            })
            .Play();
    }

    private void CloseSetNicknamePanel()
    {
        Tween panelTween = setNicknamePanelRect.DOScale(0f, GlobalDefine.panelAnimationDuration)
            .SetEase(Ease.OutExpo)
            .OnComplete(() =>
            {
                setNicknameBlockImage.SetActive(false);
                setNicknamePanel.SetActive(false);
            })
            .Play();
    }

    #endregion Methods

    #region Event Methods

    public void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        GameManager.UI.ClosePopupPanel<AuthPanel>();
    }

    public void OnClickSetNicknameButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        cancelButton.interactable = true;
        confirmButton.interactable = true;
        OpenSetNicknamePanel();
    }

    public void OnClickCancelButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        cancelButton.interactable = false;
        confirmButton.interactable = false;
        CloseSetNicknamePanel();
    }

    public void OnClickConfirmButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        if (nicknameInputfield.text == string.Empty)
        {
            GameManager.UI.Alert("닉네임을 입력해 주세요");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = nicknameValueText.text = nicknameInputfield.text;
        CloseSetNicknamePanel();
    }

    public void OnNicknameEndEdit(string value)
    {
        if (value == string.Empty) return;
        if (nicknameInputfield.text.Length <= 10) return;
        
        nicknameInputfield.text = value.Substring(0, 10);
    }

    #endregion Event Methods
}
