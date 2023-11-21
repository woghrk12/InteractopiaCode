using UnityEngine;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MeetingAndVoteGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private ButtonOption meetingTimeOption = null;
    [SerializeField] private ButtonOption voteTimeOption = null;
    [SerializeField] private DropdownOption openVoteOption = null;
    [SerializeField] private ButtonOption emergencyMeetingCooldownOption = null;

    #endregion Variables

    #region Properties

    public PhotonHashTable OptionSetting
    {
        get
        {
            PhotonHashTable optionSetting = new();

            optionSetting.Add(CustomProperties.MEETING_TIME, meetingTimeOption.OptionValue);
            optionSetting.Add(CustomProperties.VOTE_TIME, voteTimeOption.OptionValue);
            optionSetting.Add(CustomProperties.OPEN_VOTE, openVoteOption.OptionValue);
            optionSetting.Add(CustomProperties.EMERGENCY_MEETING_COOLDOWN, emergencyMeetingCooldownOption.OptionValue);

            return optionSetting;
        }
    }

    #endregion Properties

    #region Methods

    public void SetGroup(PhotonHashTable roomSetting)
    {
        meetingTimeOption.InitOption((int)roomSetting[CustomProperties.MEETING_TIME]);
        voteTimeOption.InitOption((int)roomSetting[CustomProperties.VOTE_TIME]);
        openVoteOption.InitOption((int)roomSetting[CustomProperties.OPEN_VOTE]);
        emergencyMeetingCooldownOption.InitOption((int)roomSetting[CustomProperties.EMERGENCY_MEETING_COOLDOWN]);
    }

    #endregion Methods
}
