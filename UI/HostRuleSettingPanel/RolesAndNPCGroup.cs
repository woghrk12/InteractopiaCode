using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class RolesAndNPCGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private RoleData roleData = null;

    [SerializeField] private GameObject roleGroupPrefab = null;
    [SerializeField] private RectTransform roleOptionContentRect = null;
    [SerializeField] private RectTransform citizenRoleOptionParent = null;
    [SerializeField] private RectTransform mafiaRoleOptionParent = null;
    [SerializeField] private RectTransform neutralRoleOptionParent = null;

    private RoleOption[] citizenRoleOptionList = null;
    private RoleOption[] mafiaRoleOptionList = null;
    private RoleOption[] neutralRoleOptionList = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            // Citizen Roles
            List<int> essentialCitizenRoles = new();
            List<int> optionalCitizenRoles = new();
            for (int index = 0; index < citizenRoleOptionList.Length; index++)
            {
                if (citizenRoleOptionList[index].OptionValue == RoleOption.EOptionState.ESSENTIAL)
                {
                    essentialCitizenRoles.Add(index);
                }
                else if (citizenRoleOptionList[index].OptionValue == RoleOption.EOptionState.OPTIONAL)
                {
                    optionalCitizenRoles.Add(index);
                }
            }
            optionSetting.Add(CustomProperties.ESSENTIAL_CITIZEN_ROLES, ArrayHelper.ListToArray(essentialCitizenRoles));
            optionSetting.Add(CustomProperties.OPTIONAL_CITIZEN_ROLES, ArrayHelper.ListToArray(optionalCitizenRoles));

            // Mafia Roles
            List<int> essentialMafiaRoles = new();
            List<int> optionalMafiaRoles = new();
            for (int index = 0; index < mafiaRoleOptionList.Length; index++)
            {
                if (mafiaRoleOptionList[index].OptionValue == RoleOption.EOptionState.ESSENTIAL)
                {
                    essentialMafiaRoles.Add(index);
                }
                else if (mafiaRoleOptionList[index].OptionValue == RoleOption.EOptionState.OPTIONAL)
                {
                    optionalMafiaRoles.Add(index);
                }
            }
            optionSetting.Add(CustomProperties.ESSENTIAL_MAFIA_ROLES, ArrayHelper.ListToArray(essentialMafiaRoles));
            optionSetting.Add(CustomProperties.OPTIONAL_MAFIA_ROLES, ArrayHelper.ListToArray(optionalMafiaRoles));

            // Neutral Roles
            List<int> essentialNeutralRoles = new();
            List<int> optionalNeutralRoles = new();
            for (int index = 0; index < neutralRoleOptionList.Length; index++)
            {
                if (neutralRoleOptionList[index].OptionValue == RoleOption.EOptionState.ESSENTIAL)
                {
                    essentialNeutralRoles.Add(index);
                }
                else if (neutralRoleOptionList[index].OptionValue == RoleOption.EOptionState.OPTIONAL)
                {
                    optionalNeutralRoles.Add(index);
                }
            }
            optionSetting.Add(CustomProperties.ESSENTIAL_NEUTRAL_ROLES, ArrayHelper.ListToArray(essentialNeutralRoles));
            optionSetting.Add(CustomProperties.OPTIONAL_NEUTRAL_ROLES, ArrayHelper.ListToArray(optionalNeutralRoles));

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void InitGroup()
    {
        // Create citizen role option group prefab
        List<RoleOption> citizenRoleOptionList = new();
        RoleInfo[] citizenRoles = roleData.GetAllRoleInfos(ERoleType.CITIZEN);
        foreach (RoleInfo roleInfo in citizenRoles)
        {
            RoleOption roleOption = Instantiate(roleGroupPrefab, citizenRoleOptionParent).GetComponent<RoleOption>();
            roleOption.InitOption(roleInfo, false);
            citizenRoleOptionList.Add(roleOption);
        }
        this.citizenRoleOptionList = ArrayHelper.ListToArray(citizenRoleOptionList);

        // Create mafia role option group prefab
        List<RoleOption> mafiaRoleOptionList = new();
        RoleInfo[] mafiaRoles = roleData.GetAllRoleInfos(ERoleType.MAFIA);
        foreach (RoleInfo roleInfo in mafiaRoles)
        {
            RoleOption roleOption = Instantiate(roleGroupPrefab, mafiaRoleOptionParent).GetComponent<RoleOption>();
            roleOption.InitOption(roleInfo, false);
            mafiaRoleOptionList.Add(roleOption);
        }
        this.mafiaRoleOptionList = ArrayHelper.ListToArray(mafiaRoleOptionList);

        // Create neutral role option group prefab
        List<RoleOption> neutralRoleOptionList = new();
        RoleInfo[] neutralRoles = roleData.GetAllRoleInfos(ERoleType.NEUTRAL);
        foreach (RoleInfo roleInfo in neutralRoles)
        {
            RoleOption roleOption = Instantiate(roleGroupPrefab, neutralRoleOptionParent).GetComponent<RoleOption>();
            roleOption.InitOption(roleInfo, false);
            neutralRoleOptionList.Add(roleOption);
        }
        this.neutralRoleOptionList = ArrayHelper.ListToArray(neutralRoleOptionList);

        LayoutRebuilder.ForceRebuildLayoutImmediate(roleOptionContentRect);
    }

    public void SetGroup(PhotonHashTable roomSetting)
    {
        int[] roleList;
        // Set the option state of essential Citizen roles
        roleList = (int[])roomSetting[CustomProperties.ESSENTIAL_CITIZEN_ROLES];
        foreach (int role in roleList)
        {
            citizenRoleOptionList[role].OptionValue = RoleOption.EOptionState.ESSENTIAL;
        }

        // Set the option state of optional Citizen roles
        roleList = (int[])roomSetting[CustomProperties.OPTIONAL_CITIZEN_ROLES];
        foreach(int role in roleList)
        {
            citizenRoleOptionList[role].OptionValue = RoleOption.EOptionState.OPTIONAL;
        }

        // Set the option state of essential Mafia roles
        roleList = (int[])roomSetting[CustomProperties.ESSENTIAL_MAFIA_ROLES];
        foreach(int role in roleList)
        {
            mafiaRoleOptionList[role].OptionValue = RoleOption.EOptionState.ESSENTIAL;
        }

        // Set the option state of optional Mafia roles
        roleList = (int[])roomSetting[CustomProperties.OPTIONAL_MAFIA_ROLES];
        foreach(int role in roleList)
        {
            mafiaRoleOptionList[role].OptionValue = RoleOption.EOptionState.OPTIONAL;
        }

        // Set the option state of essential Neutral roles
        roleList = (int[])roomSetting[CustomProperties.ESSENTIAL_NEUTRAL_ROLES];
        foreach(int role in roleList)
        {
            neutralRoleOptionList[role].OptionValue = RoleOption.EOptionState.ESSENTIAL;
        }

        // Set the option state of optional Neutral roles
        roleList = (int[])roomSetting[CustomProperties.OPTIONAL_NEUTRAL_ROLES];
        foreach(int role in roleList)
        {
            neutralRoleOptionList[role].OptionValue = RoleOption.EOptionState.OPTIONAL;
        }
    }
    
    #endregion Methods
}
