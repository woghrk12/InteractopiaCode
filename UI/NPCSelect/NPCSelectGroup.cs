using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCSelectGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private Button button = null;
    [SerializeField] private Image buttonImage = null;
    [SerializeField] private ENPCRole npcType = ENPCRole.NONE;

    #endregion

    #region Properties

    public Button Button => button;

    public ENPCRole NPCType => npcType;

    #endregion Properties

    #region Methods

    public void InitGroup()
    {
        button.onClick.AddListener(() =>
        {
            (GameManager.InGame.NPCList[ENPCRole.REPAIR] as RepairBotNPC).Repair(npcType);
            GameManager.UI.ClosePopupPanel<NPCSelectPanel>();
        });
    }

    public void SetActive(bool isActive)
    {
        buttonImage.color = isActive ? new Color(1f, 1f, 1f) : new Color(0.2f, 0.2f, 0.2f);
        button.interactable = isActive;
    }

    #endregion
}
