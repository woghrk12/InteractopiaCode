using UnityEngine;
using UnityEngine.UI;

public class ToggleOption : MonoBehaviour
{
    #region Variables

    [SerializeField] private Toggle toggle = null;

    #endregion Variables

    #region Properties

    public bool OptionValue => toggle.isOn;

    #endregion Properties

    #region Methods

    public void InitOption(bool initValue)
    {
        toggle.isOn = initValue;
    }

    #endregion Methods
}
