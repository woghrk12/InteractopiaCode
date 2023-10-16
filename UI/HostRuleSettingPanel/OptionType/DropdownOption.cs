using UnityEngine;
using UnityEngine.UI;

public class DropdownOption : MonoBehaviour
{
    #region Variables

    [SerializeField] private Dropdown dropdown = null;

    #endregion Variables

    #region Properties

    public int OptionValue => dropdown.value;

    #endregion Properties

    #region Methods

    public void InitOption(int initValue)
    {
        dropdown.value = initValue;
    }

    #endregion Methods
}
