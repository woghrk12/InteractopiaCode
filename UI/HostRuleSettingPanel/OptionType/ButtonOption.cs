using UnityEngine;
using UnityEngine.UI;

public class ButtonOption : MonoBehaviour
{
    #region Variables

    private int optionValue = 0;

    [SerializeField] private int stepValue = 0;
    [SerializeField] private int maxValue = 0;
    [SerializeField] private int minValue = 0;

    [SerializeField] private Button plusBtn = null;
    [SerializeField] private Button minusBtn = null;
    [SerializeField] private Text valueText = null;

    #endregion Variables

    #region Properties

    public int OptionValue => optionValue;

    #endregion Properties

    #region Methods

    public void InitOption(int initValue)
    {
        if (initValue >= maxValue)
        {
            optionValue = maxValue;
            plusBtn.interactable = false;
        }
        else if (initValue <= minValue)
        {
            optionValue = minValue;
            minusBtn.interactable = false;
        }
        else
        {
            optionValue = initValue;
        }

        valueText.text = optionValue.ToString();

        plusBtn.onClick.AddListener(OnClickPlusBtn);
        minusBtn.onClick.AddListener(OnClickMinusBtn);
    }

    public void OnClickPlusBtn()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        optionValue += stepValue;
        valueText.text = optionValue.ToString();

        if (optionValue >= maxValue) { plusBtn.interactable = false; }
        if (!minusBtn.interactable) { minusBtn.interactable = true; }
    }

    public void OnClickMinusBtn()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        optionValue -= stepValue;
        valueText.text = optionValue.ToString();

        if (optionValue <= minValue) { minusBtn.interactable = false; }
        if (!plusBtn.interactable) { plusBtn.interactable = true; }
    }

    #endregion Methods
}
