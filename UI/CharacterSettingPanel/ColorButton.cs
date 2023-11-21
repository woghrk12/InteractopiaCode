using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ColorButton : MonoBehaviour
{
    public enum EButtonState { NONE = -1, MINE, INUSE}

    #region Variables

    private EButtonState buttonState = EButtonState.NONE;

    [SerializeField] private ECharacterColor characterColor = ECharacterColor.NONE;

    [SerializeField] private GameObject xImgObject = null;
    [SerializeField] private GameObject checkImgObject = null;

    #endregion Variables

    #region Properties

    public ECharacterColor CharacterColor => characterColor;

    public EButtonState ButtonState => buttonState;

    #endregion Properties

    #region Methods

    public void SetButton(EButtonState state)
    {
        switch (state)
        {
            case EButtonState.NONE:
                xImgObject.SetActive(false);
                checkImgObject.SetActive(false);
                break;

            case EButtonState.MINE:
                xImgObject.SetActive(false);
                checkImgObject.SetActive(true);
                break;

            case EButtonState.INUSE:
                xImgObject.SetActive(true);
                checkImgObject.SetActive(false);
                break;

            default:
                throw new System.Exception($"Currently not supported! Input state : {state}");
        }

        buttonState = state;
    }

    #endregion Methods
}
