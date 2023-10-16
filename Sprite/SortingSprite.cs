using UnityEngine;

public class SortingSprite : MonoBehaviour
{
    #region Variables

    private SpriteSorter sortingController = null;

    [SerializeField] private SpriteRenderer spriteRender = null;
    [SerializeField] private ESortingType sortingType = ESortingType.UPDATE;

    #endregion Variables

    #region Unity Events

    private void Start()
    {
        sortingController = GameObject.FindObjectOfType<SpriteSorter>();

        spriteRender.sortingOrder = sortingController.GetSortingOrder(transform);
    }

    private void Update()
    {
        if (sortingType == ESortingType.UPDATE)
            spriteRender.sortingOrder = sortingController.GetSortingOrder(transform);
    }

    #endregion Unity Events
}
