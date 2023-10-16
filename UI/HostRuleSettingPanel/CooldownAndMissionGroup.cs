using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CooldownAndMissionGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private ButtonOption killCooldownOption = null;
    [SerializeField] private ButtonOption sabotageCooldownOption = null;
    [SerializeField] private ButtonOption numMissionOption = null;
    [SerializeField] private ButtonOption minMissionCooldownOption = null;
    [SerializeField] private ButtonOption maxMissionCooldownOption = null;
    [SerializeField] private DropdownOption numSpecialMissionOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.KILL_COOLDOWN, killCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.SABOTAGE_COOLDOWN, sabotageCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_MISSION, numMissionOption.OptionValue);
            optionSetting.Add(CustomProperties.MIN_MISSION_COOLDOWN, minMissionCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.MAX_MISSION_COOLDOWN, maxMissionCooldownOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_SPECIAL_MISSION, numSpecialMissionOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        killCooldownOption.InitOption((int)roomSetting[CustomProperties.KILL_COOLDOWN]);
        sabotageCooldownOption.InitOption((int)roomSetting[CustomProperties.SABOTAGE_COOLDOWN]);
        numMissionOption.InitOption((int)roomSetting[CustomProperties.NUM_MISSION]);
        minMissionCooldownOption.InitOption((int)roomSetting[CustomProperties.MIN_MISSION_COOLDOWN]);
        maxMissionCooldownOption.InitOption((int)roomSetting[CustomProperties.MAX_MISSION_COOLDOWN]);
        numSpecialMissionOption.InitOption((int)roomSetting[CustomProperties.NUM_SPECIAL_MISSION]);
    }
    
    #endregion Methods
}
