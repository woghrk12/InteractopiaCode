using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectButton : MonoBehaviour
{
    #region Variables

    private ERoleType team = ERoleType.NONE;
    private int role = -1;

    [SerializeField] private Button button = null;
    [SerializeField] private Image buttonImage = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private Text roleText = null;
    [SerializeField] private GameObject highlightedImageObject = null;

    #endregion Variables

    #region Properties

    public ERoleType Team => team;
    public int Role => role;

    #endregion Properties

    #region Methods

    public void InitButton(AssasinSelectPanel panel, RoleInfo roleInfo)
    {
        button.onClick.AddListener(() => 
            {
                SoundManager.Instance.SpawnEffect(ESoundKey.SFX_POP_Brust_08);
                panel.SelectRole(this); 
            });

        team = roleInfo.RoleType;
        role = roleInfo.ID;

        buttonImage.color = roleInfo.RoleColor;
        roleText.color = roleInfo.RoleColor;
        iconImage.color = roleInfo.RoleColor;

        iconImage.sprite = roleInfo.RoleIconSprite;
        roleText.text = roleInfo.RoleName;
    }

    public void SetActiveButton(bool isActive)
    {
        highlightedImageObject.SetActive(isActive);   
    }

    #endregion Methods
}
