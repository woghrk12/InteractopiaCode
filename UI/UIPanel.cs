using System;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public abstract class UIPanel : MonoBehaviourPunCallbacks
{
    #region Variables

    protected Canvas panelCanvas = null;

    #endregion Variables

    #region Properties

    public int SortingOrder
    {
        set
        {
            panelCanvas.sortingOrder = value;
        }
        get
        {
            return panelCanvas.sortingOrder;
        }
    }

    public Action OnActive { protected set; get; }

    public Action OnDeactive { protected set; get; }

    #endregion Properties

    #region Unity Events

    protected void Awake()
    {
        panelCanvas = GetComponent<Canvas>();
    }

    #endregion Unity Events

    #region Methods

    public abstract void InitPanel();

    public abstract Sequence ActiveAnimation();

    public abstract Sequence DeactiveAnimation();

    #endregion Methods
}
