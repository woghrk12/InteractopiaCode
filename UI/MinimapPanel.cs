using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum EMinimapOpenCause { NONE = -1, TELOPORT, WATCH, SABOTAGE, END }

public class MinimapPanel : UIPanel
{
    #region Variables

    [HideInInspector] public EMinimapOpenCause OpenCause = EMinimapOpenCause.NONE; 

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private RectTransform mapPositionButtonParent = null;
    private Dictionary<EMapPosition, MapPositionButton> mapPositionButtonDictionary = new();

    #endregion Variables

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        foreach (RectTransform child in mapPositionButtonParent)
        {
            MapPositionButton button = child.GetComponent<MapPositionButton>();
            mapPositionButtonDictionary.Add(button.MapPosition, button);
        }

        OnActive += () =>
        {
            closeButton.interactable = true;

            foreach (KeyValuePair<EMapPosition, MapPositionButton> button in mapPositionButtonDictionary)
            {
                if (OpenCause == EMinimapOpenCause.NONE)
                {
                    button.Value.gameObject.SetActive(false);
                }
                else
                {
                    button.Value.gameObject.SetActive(true);
                    button.Value.Button.interactable = true;
                    button.Value.OpenCause = OpenCause;
                }
            }
        };
        OnDeactive += () =>
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            switch (OpenCause)
            {
                case EMinimapOpenCause.TELOPORT:
                    GameManager.InGame.NPCList[ENPCRole.TRANSPORT].EndInteract(actorNumber);
                    break;
                
                case EMinimapOpenCause.WATCH:
                    GameManager.InGame.NPCList[ENPCRole.SURVEILLANCE].EndInteract(actorNumber);
                    break;
            }
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

    private void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;

        foreach (KeyValuePair<EMapPosition, MapPositionButton> button in mapPositionButtonDictionary)
        {
            button.Value.Button.interactable = false;
        }

        GameManager.UI.ClosePopupPanel<MinimapPanel>();
    }

    #endregion Event Methods
}
