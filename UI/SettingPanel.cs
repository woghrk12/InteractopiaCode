using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SettingPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button exitRoomButton = null;
    [SerializeField] private Button exitGameButton = null;

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        exitRoomButton.onClick.AddListener(OnClickExitRoomButton);
        exitGameButton.onClick.AddListener(OnClickExitGameButton);

        OnActive += () =>
        {
            closeButton.interactable = true;
            exitRoomButton.interactable = true;
            exitGameButton.interactable = true;

            exitRoomButton.gameObject.SetActive(PhotonNetwork.InRoom && SceneManager.GetActiveScene().buildIndex == (int)EScene.TITLE);
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
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        exitRoomButton.interactable = false;
        exitGameButton.interactable = false;

        GameManager.UI.ClosePopupPanel<SettingPanel>();
    }

    public void OnClickExitRoomButton() 
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        exitRoomButton.interactable = false;
        exitGameButton.interactable = false;

        GameManager.Title.LeaveRoom();
    }

    public void OnClickExitGameButton() 
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    #endregion Event Methods
}
