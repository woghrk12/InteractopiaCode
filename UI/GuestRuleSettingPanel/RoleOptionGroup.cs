using UnityEngine;
using UnityEngine.UI;

public class RoleOptionGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private Text roleName = null;
    [SerializeField] private Image roleIconImage = null;
    [SerializeField] private GameObject checkMarkObject = null;
    [SerializeField] private GameObject questionMarkObject = null;

    #endregion Variables

    #region Methods

    public void InitGroup(RoleInfo roleInfo)
    {
        if (roleInfo.RoleIconSprite != null)
        {
            roleIconImage.color = roleInfo.RoleColor;
            roleIconImage.sprite = roleInfo.RoleIconSprite;
        }

        roleName.text = roleInfo.RoleName;
    }

    public void SetGroup(RoleOption.EOptionState state)
    {
        if (state == RoleOption.EOptionState.EXCLUDE)
        {
            checkMarkObject.SetActive(false);
            questionMarkObject.SetActive(false);
        }
        else if (state == RoleOption.EOptionState.ESSENTIAL)
        {
            checkMarkObject.SetActive(true);
            questionMarkObject.SetActive(false);
        }
        else if (state == RoleOption.EOptionState.OPTIONAL)
        {
            checkMarkObject.SetActive(false);
            questionMarkObject.SetActive(true);
        }
    }

    #endregion Methods
}

