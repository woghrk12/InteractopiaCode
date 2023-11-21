using UnityEngine;
using UnityEngine.UI;

public class RoleOption : MonoBehaviour
{
    public enum EOptionState { NONE = -1, EXCLUDE = 0, OPTIONAL, ESSENTIAL}

    #region Variables

    private EOptionState optionValue = 0;

    private bool isNPC = false;

    [SerializeField] private Text roleName = null;
    [SerializeField] private Image roleIconImage = null;
    [SerializeField] private Button optionButton = null;
    [SerializeField] private GameObject checkMarkObject = null;
    [SerializeField] private GameObject questionMarkObject = null;
    
    #endregion Variables

    #region Properties

    public EOptionState OptionValue
    {
        set
        {
            optionValue = value;

            if (optionValue == EOptionState.ESSENTIAL)
            {
                checkMarkObject.SetActive(true);
                questionMarkObject.SetActive(false);
            }
            else if (optionValue == EOptionState.OPTIONAL)
            {
                checkMarkObject.SetActive(false);
                questionMarkObject.SetActive(true);
            }
            else
            {
                checkMarkObject.SetActive(false);
                questionMarkObject.SetActive(false);
            }
        }
        get => optionValue;
    }

    #endregion Properties

    #region Methods

    public void InitOption(RoleInfo roleInfo, bool isNPC)
    {
        this.isNPC = isNPC;

        if (roleInfo.RoleIconSprite != null)
        {
            roleIconImage.sprite = roleInfo.RoleIconSprite;
            roleIconImage.color = roleInfo.RoleColor;
        }

        roleName.text = roleInfo.RoleName;
        OptionValue = EOptionState.EXCLUDE;

        optionButton.onClick.AddListener(OnClickOptionButton);
    }

    #endregion Methods

    #region Event Methods

    public void OnClickOptionButton()
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);

        if (!isNPC)
        {
            if (optionValue == EOptionState.EXCLUDE) 
            {
                OptionValue = EOptionState.ESSENTIAL; 
            }
            else if (optionValue == EOptionState.OPTIONAL)
            {
                OptionValue = EOptionState.EXCLUDE; 
            }
            else 
            {
                OptionValue = EOptionState.OPTIONAL;
            }
        }
        else
        {
            if (optionValue == EOptionState.EXCLUDE)
            {
                OptionValue = EOptionState.ESSENTIAL;
            }
            else
            {
                OptionValue = EOptionState.EXCLUDE;
            }
        }
    }

    #endregion Event Methods
}
