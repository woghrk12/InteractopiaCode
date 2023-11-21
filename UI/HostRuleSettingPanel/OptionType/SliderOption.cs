using UnityEngine;
using UnityEngine.UI;

public class SliderOption : MonoBehaviour
{
    #region Variables

    [SerializeField] private Slider slider = null;
    [SerializeField] private Text valueText = null;

    #endregion Variables

    #region Properties

    public int OptionValue => (int)slider.value;

    #endregion Properties

    #region Methods

    public void InitOption(int initValue, int maxValue, int minValue)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = initValue;

        valueText.text = initValue.ToString();
        slider.onValueChanged.AddListener(OnOptionValueChanged);
    }

    public void OnOptionValueChanged(float value)
    {
        int optionValue = (int)value;
        valueText.text = optionValue.ToString();
    }
    

    #endregion Methods
}
