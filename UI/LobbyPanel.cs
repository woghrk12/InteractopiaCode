using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyPanel : UIPanel
{
    #region Varibles

    [SerializeField] private Button createRoomButton = null;
    [SerializeField] private Button publicJoinButton = null;
    [SerializeField] private Button privateJoinButton = null;
    [SerializeField] private Button cancelButton = null;

    #endregion Varibles

    #region Override Methods

    public override void InitPanel()
    {
        createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
        publicJoinButton.onClick.AddListener(OnClickPublicJoinButton);
        privateJoinButton.onClick.AddListener(OnClickPrivateJoinButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
    }

    public override Sequence ActiveAnimation()
    {
        return DOTween.Sequence()
            .Append(GameManager.UI.FadeIn(GlobalDefine.fadeEffectDuration));
    }

    public override Sequence DeactiveAnimation()
    {
        return DOTween.Sequence()
            .Append(GameManager.UI.FadeOut(GlobalDefine.fadeEffectDuration));
    }

    #endregion Override Methods

    #region Event Methods

    public void OnClickCreateRoomButton() => GameManager.UI.OpenPanel<CreateRoomPanel>();

    public void OnClickPublicJoinButton() => GameManager.UI.OpenPanel<PublicJoinPanel>();

    public void OnClickPrivateJoinButton() => GameManager.UI.PopupPanel<PrivateJoinPanel>();

    public void OnClickCancelButton() => GameManager.UI.OpenPanel<StartPanel>();

    #endregion Event Methods
}
