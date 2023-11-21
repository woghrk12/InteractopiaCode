using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MovementAndSightGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private DropdownOption sightOption = null;
    [SerializeField] private DropdownOption moveSpeedOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.SIGHT_RANGE, sightOption.OptionValue);
            optionSetting.Add(CustomProperties.MOVE_SPEED, moveSpeedOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        sightOption.InitOption((int)roomSetting[CustomProperties.SIGHT_RANGE]);
        moveSpeedOption.InitOption((int)roomSetting[CustomProperties.MOVE_SPEED]);
    }
    
    #endregion Methods
}
