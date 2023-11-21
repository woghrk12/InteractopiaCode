using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CommonGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private ToggleOption shortDistanceVoiceOption = null;
    [SerializeField] private ToggleOption randomStartPointOption = null;
    [SerializeField] private SliderOption numMafiaOption = null;
    [SerializeField] private SliderOption numNeutralOption = null;
    [SerializeField] private ToggleOption hideEmissionInfoOption = null;
    [SerializeField] private ToggleOption blindMafiaModeOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.SHORT_DISTANCE_VOICE, shortDistanceVoiceOption.OptionValue);
            optionSetting.Add(CustomProperties.RANDOM_START_POINT, randomStartPointOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_MAFIAS, numMafiaOption.OptionValue);
            optionSetting.Add(CustomProperties.NUM_NEUTRALS, numNeutralOption.OptionValue);
            optionSetting.Add(CustomProperties.HIDE_EMISSION_INFO, hideEmissionInfoOption.OptionValue);
            optionSetting.Add(CustomProperties.BLIND_MAFIA_MODE, blindMafiaModeOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        shortDistanceVoiceOption.InitOption((bool)roomSetting[CustomProperties.SHORT_DISTANCE_VOICE]);
        randomStartPointOption.InitOption((bool)roomSetting[CustomProperties.RANDOM_START_POINT]);
        numMafiaOption.InitOption(
            (int)roomSetting[CustomProperties.NUM_MAFIAS],
            (int)roomSetting[CustomProperties.MAX_MAFIAS],
            1);
        numNeutralOption.InitOption(
            (int)roomSetting[CustomProperties.NUM_NEUTRALS],
            (int)roomSetting[CustomProperties.MAX_NEUTRALS],
            0);
        hideEmissionInfoOption.InitOption((bool)roomSetting[CustomProperties.HIDE_EMISSION_INFO]);
        blindMafiaModeOption.InitOption((bool)roomSetting[CustomProperties.BLIND_MAFIA_MODE]);
    }

    #endregion Methods
}
