using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNickname : MonoBehaviour
{
    #region Variables

    [SerializeField] private Text nicknameText = null;

    #endregion Variables

    #region Properties

    public Text NicknameText => nicknameText;

    #endregion Properties

    #region Methods

    public void SetText(string value)
    {
        nicknameText.text = value;
    }

    public void SetColor(Color value)
    { 
        nicknameText.color = value;
    }

    #endregion Methods
}
