using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCSelectPanel : UIPanel
{
    #region Variables

    [SerializeField] private RectTransform panelRect = null;
    [SerializeField] private Button closeButton = null;

    [SerializeField] private RectTransform npcSelectParent = null;
    private Dictionary<ENPCRole, NPCSelectGroup> npcSelectDictionary = new();

    #endregion

    #region Override Methods

    public override void InitPanel()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        foreach (RectTransform child in npcSelectParent)
        {
            NPCSelectGroup group = child.GetComponent<NPCSelectGroup>();
            group.InitGroup();
            npcSelectDictionary.Add(group.NPCType, group);
            group.gameObject.SetActive(false);
        }

        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        {
            npcSelectDictionary[npc.Key].gameObject.SetActive(true);
        }

        OnActive += () =>
        {
            Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
            Dictionary<ENPCRole, BaseNPC> deadNPCList = GameManager.InGame.DeadNPCList;
            foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
            {
                npcSelectDictionary[npc.Key].SetActive(false);
            }
            foreach (KeyValuePair<ENPCRole, BaseNPC> npc in deadNPCList)
            {
                npcSelectDictionary[npc.Key].SetActive(true);
            }
        };
        OnDeactive += () =>
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            GameManager.InGame.NPCList[ENPCRole.REPAIR].EndInteract(actorNumber);
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

    #region Methods

    public void SetNPCSelectGroup(ENPCRole npcType, bool isActive)
    {
        npcSelectDictionary[npcType].SetActive(isActive);
    }

    #endregion Methods

    #region Event Methods

    private void OnClickCloseButton()
    {
        closeButton.interactable = false;
        foreach (KeyValuePair<ENPCRole, NPCSelectGroup> group in npcSelectDictionary)
        {
            group.Value.Button.interactable = false;
        }

        GameManager.UI.ClosePopupPanel<NPCSelectPanel>();
    }

    #endregion Event Methods
}
