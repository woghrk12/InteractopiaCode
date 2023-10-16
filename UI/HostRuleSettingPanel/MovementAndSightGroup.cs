using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MovementAndSightGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private DropdownOption citizenSightOption = null;
    [SerializeField] private DropdownOption mafiaSightOption = null;
    [SerializeField] private DropdownOption neutralSightOption = null;
    [SerializeField] private DropdownOption moveSpeedOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.CITIZEN_SIGHT, citizenSightOption.OptionValue);
            optionSetting.Add(CustomProperties.MAFIA_SIGHT, mafiaSightOption.OptionValue);
            optionSetting.Add(CustomProperties.NEUTRAL_SIGHT, neutralSightOption.OptionValue);
            optionSetting.Add(CustomProperties.MOVE_SPEED, moveSpeedOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        citizenSightOption.InitOption((int)roomSetting[CustomProperties.CITIZEN_SIGHT]);
        mafiaSightOption.InitOption((int)roomSetting[CustomProperties.MAFIA_SIGHT]);
        neutralSightOption.InitOption((int)roomSetting[CustomProperties.NEUTRAL_SIGHT]);
        moveSpeedOption.InitOption((int)roomSetting[CustomProperties.MOVE_SPEED]);
    }
    
    #endregion Methods
}
