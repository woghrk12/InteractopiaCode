using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CooldownAndMissionGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private ButtonOption killCooldownOption = null;
    [SerializeField] private ButtonOption missionCooldownOption = null;
    [SerializeField] private DropdownOption numNPCMissionOption = null;
    [SerializeField] private DropdownOption numSpecialMissionOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.KILL_COOLDOWN, killCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.MISSION_COOLDOWN, missionCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_NPC_MISSION, numNPCMissionOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_SPECIAL_MISSION, numSpecialMissionOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        killCooldownOption.InitOption((int)roomSetting[CustomProperties.KILL_COOLDOWN]);
        missionCooldownOption.InitOption((int)roomSetting[CustomProperties.MISSION_COOLDOWN]);
        numNPCMissionOption.InitOption((int)roomSetting[CustomProperties.NUM_NPC_MISSION]);
        numSpecialMissionOption.InitOption((int)roomSetting[CustomProperties.NUM_SPECIAL_MISSION]);
    }
    
    #endregion Methods
}
