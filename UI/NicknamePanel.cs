using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NicknamePanel : MonoBehaviour
{
    #region Variables

    [SerializeField] private RectTransform setNicknamePanelRect = null;
    [SerializeField] private InputField nicknameInputfield = null;
    [SerializeField] private Button confirmButton = null;

    #endregion Variables

    #region Unity Events

    private void Start()
    {
        nicknameInputfield.onEndEdit.AddListener(OnNicknameEndEdit);
        confirmButton.onClick.AddListener(OnClickConfirmButton);
    }

    #endregion Unity Events

    #region Event Methods

    public void OnClickConfirmButton()
    {
        if (nicknameInputfield.text == string.Empty)
        {
            GameManager.UI.Alert("Please write your nickname!");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = nicknameInputfield.text;

        Tween panelTween = setNicknamePanelRect.DOAnchorPos3DY(1500f, GlobalDefine.panelAnimationDuration)
            .OnComplete(() => gameObject.SetActive(false));
        Tween fadeOutTween = GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration)
            .OnComplete(() => GameManager.Title.StartScene());

        DOTween.Sequence()
            .Append(panelTween)
            .Join(fadeOutTween)
            .Play();
    }

    public void OnNicknameEndEdit(string value)
    {
        if (value == string.Empty) return;
        if (nicknameInputfield.text.Length <= 10) return;

        nicknameInputfield.text = value.Substring(0, 10);
    }

    #endregion Event Methods
}
