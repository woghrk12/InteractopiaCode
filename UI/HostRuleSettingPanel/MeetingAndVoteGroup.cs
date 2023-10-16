using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MeetingAndVoteGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private ToggleOption openingAddressOption = null;
    [SerializeField] private ButtonOption meetingTimeOption = null;
    [SerializeField] private ButtonOption voteTimeOption = null;
    [SerializeField] private DropdownOption openVoteOption = null;
    [SerializeField] private ToggleOption openVoteResultOption = null;
    [SerializeField] private ButtonOption emergencyMeetingCooldownOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.OPENING_ADDRESS, openingAddressOption.OptionValue);
            optionSetting.Add(CustomProperties.MEETING_TIME, meetingTimeOption.OptionValue);
            optionSetting.Add(CustomProperties.VOTE_TIME, voteTimeOption.OptionValue);
            optionSetting.Add(CustomProperties.OPEN_VOTE, openVoteOption.OptionValue);
            optionSetting.Add(CustomProperties.OPEN_VOTE_RESULT, openVoteResultOption.OptionValue);
            optionSetting.Add(CustomProperties.EMERGENCY_MEETING_COOLDOWN, emergencyMeetingCooldownOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        openingAddressOption.InitOption((bool)roomSetting[CustomProperties.OPENING_ADDRESS]);
        meetingTimeOption.InitOption((int)roomSetting[CustomProperties.MEETING_TIME]);
        voteTimeOption.InitOption((int)roomSetting[CustomProperties.VOTE_TIME]);
        openVoteOption.InitOption((int)roomSetting[CustomProperties.OPEN_VOTE]);
        openVoteResultOption.InitOption((bool)roomSetting[CustomProperties.OPEN_VOTE_RESULT]);
        emergencyMeetingCooldownOption.InitOption((int)roomSetting[CustomProperties.EMERGENCY_MEETING_COOLDOWN]);
    }

    #endregion Methods
}
