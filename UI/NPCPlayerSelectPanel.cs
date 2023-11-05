using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class NPCPlayerSelectPanel : UIPanel
{
    #region Variables
    
    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private RectTransform playerSelectParent = null;
    private Dictionary<int, PlayerSelectGroup> playerSelectGroupDictionary = new();

    #endregion

    #region Override Methods
    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            PlayerSelectGroup selectGroup = GameManager.Resource.Instantiate("UI/NPCPlayerSelectPanel/PlayerSelectGroup", playerSelectParent).GetComponent<PlayerSelectGroup>();
            selectGroup.InitGroup(player.Key);
            playerSelectGroupDictionary.Add(player.Key, selectGroup);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerSelectParent);

        OnDeactive += () =>
        {
            closeButton.interactable = true;
            foreach (KeyValuePair<int, PlayerSelectGroup> group in playerSelectGroupDictionary)
            {
                group.Value.Button.interactable = true;
            }
        };
        OnDeactive += () =>
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            GameManager.InGame.NPCList[ENPCRole.INSPECTION].EndInteract(actorNumber);
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
   
    #endregion

    #region Event Methods

    private void OnClickCloseButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        closeButton.interactable = false;
        foreach (KeyValuePair<int, PlayerSelectGroup> group in playerSelectGroupDictionary)
        {
            group.Value.Button.interactable = false;
        }

        GameManager.UI.ClosePopupPanel<NPCPlayerSelectPanel>();
    }

    #endregion Event Methods
}
