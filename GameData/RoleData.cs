using System;
using UnityEngine;

public enum ERoleType { NONE = -1, CITIZEN, MAFIA, NEUTRAL, NPC, END }

public enum ECitizenRole { NONE = -1, POLICE, VAGILANTE, INSPECTOR, TECHNICIAN, SEER, END }
public enum EMafiaRole { NONE = -1, ASSASSIN, SPY, MANIPULATOR, END }
public enum ENeutralRole { NONE = -1, ROGUE, COLLECTOR, END }
public enum ENPCRole { NONE = -1, INSPECTION, SURVEILLANCE, REPAIR, TRANSPORT, END }

[Serializable]
public struct RoleInfo
{
    public ERoleType RoleType;
    public int ID;
    public string RoleName;
    public Color RoleColor;
    public Sprite RoleIconSprite;
    public string Description;
    public GameObject PlayerObject;
}

[CreateAssetMenu(menuName = "Game Data/Role Data", fileName = "new RoleData")]
public class RoleData : ScriptableObject
{
    #region Variables

    [Header("Strings for team name")]
    [SerializeField] private string[] teamNames = null;

    [Header("Default role of each team")]
    [SerializeField] private RoleInfo defaultCitizenRole;
    [SerializeField] private RoleInfo defaultMafiaRole;

    [Header("Role info of each team")]
    [SerializeField] private RoleInfo[] citizenRoles = null;
    [SerializeField] private RoleInfo[] mafiaRoles = null;
    [SerializeField] private RoleInfo[] neutralRoles = null;
    [SerializeField] private RoleInfo[] npcRoles = null;

    #endregion Variables

    #region Methods

    public string GetTeamName(int index)
    {
        if (index < 0 || index >= (int)ERoleType.END)
        {
            throw new Exception($"Out of range. Input index : {index}");
        }

        return teamNames[index];
    }

    public RoleInfo[] GetAllRoleInfos(ERoleType type)
    {
        switch (type)
        {
            case ERoleType.CITIZEN:
                return citizenRoles;

            case ERoleType.MAFIA:
                return mafiaRoles;

            case ERoleType.NEUTRAL:
                return neutralRoles;

            case ERoleType.NPC:
                return npcRoles;
        }

        throw new Exception($"Unknown Role Type. Input type : {(int)type}");
    }

    public RoleInfo GetRoleInfo(ERoleType type, int index)
    {
        switch (type)
        {
            case ERoleType.CITIZEN:
                return index >= 0 && index < (int)ECitizenRole.END ? citizenRoles[index] : defaultCitizenRole;

            case ERoleType.MAFIA:
                return index >= 0 && index < (int)EMafiaRole.END ? mafiaRoles[index] : defaultMafiaRole;

            case ERoleType.NEUTRAL:
                if (index < 0 || index >= (int)ENeutralRole.END) { throw new Exception($"Out of range. Input idx : {index}"); }
                return neutralRoles[index];

            case ERoleType.NPC:
                if (index < 0 || index >= (int)ENPCRole.END) { throw new Exception($"Out of range. Input idx : {index}"); }
                return npcRoles[index];
        }

        throw new Exception($"Unknown Role Type. Input type : {(int)type}");
    }

    #endregion Methods
}
