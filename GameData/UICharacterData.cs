using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/UI Color Data", fileName = "new UIColorData")]
public class UICharacterData : ScriptableObject
{
    #region Variables

    [SerializeField] private Sprite[] headUISpriteList = null;
    [SerializeField] private Sprite[] bodyUISpriteList = null;
    [SerializeField] private Sprite[] deadUISpriteList = null;

    #endregion Variables

    #region Methods

    public Sprite GetHeadSprite(ECharacterColor color)
    {
        if (color <= ECharacterColor.NONE || color >= ECharacterColor.END)
        {
            throw new System.Exception($"Out of range. Input color : {color}");
        }

        return headUISpriteList[(int)color];
    }

    public Sprite GetBodySprite(ECharacterColor color)
    {
        if (color <= ECharacterColor.NONE || color >= ECharacterColor.END)
        {
            throw new System.Exception($"Out of range. Input color : {color}");
        }

        return bodyUISpriteList[(int)color];
    }

    public Sprite GetDeadSprite(ECharacterColor color)
    {
        if (color <= ECharacterColor.NONE || color >= ECharacterColor.END)
        {
            throw new System.Exception($"Out of range. Input color : {color}");
        }

        return deadUISpriteList[(int)color];
    }

    #endregion Methods
}
