public class CustomProperties
{
    // Room CustomProperties
    public const string ROOM_NAME = "RoomName";
    public const string MAP = "Map";
    public const string SPAWN_POSITION = "SpawnPosition";
    public const string IS_VISIBLE = "IsVisible";

    // For common group
    public const string SHORT_DISTANCE_VOICE = "ShortDistanceVoice";
    public const string RANDOM_START_POINT = "RandomStartPoint";
    public const string MAX_MAFIAS = "MaxMafias";
    public const string NUM_MAFIAS = "NumMafias";
    public const string MAX_NEUTRALS = "MaxNeutrals";
    public const string NUM_NEUTRALS = "NumNeutrals";
    public const string HIDE_EMISSION_INFO = "HideEmissionInfo";
    public const string BLIND_MAFIA_MODE = "BlindMafiaMode";

    // For meeting and vote group
    public const string VOTE_TIME = "VoteTime";
    public const string MEETING_TIME = "MeetingTime";
    public const string OPEN_VOTE = "OpenVote";
    public const string EMERGENCY_MEETING_COOLDOWN = "EmergencyMeetingCooldown";

    // For roles and NPC group
    public const string ESSENTIAL_CITIZEN_ROLES = "EssentialCitizenRoles";
    public const string OPTIONAL_CITIZEN_ROLES = "OptionalCitizenRoles";
    public const string ESSENTIAL_MAFIA_ROLES = "EssentialMafiaRoles";
    public const string OPTIONAL_MAFIA_ROLES = "OptionalMafiaRoles";
    public const string ESSENTIAL_NEUTRAL_ROLES = "EssentialNeutralRoles";
    public const string OPTIONAL_NEUTRAL_ROLES = "OptionalNeutralRoles";

    // For movement and sight group
    public const string SIGHT_RANGE = "SightRange";
    public const string MOVE_SPEED = "MoveSpeed";

    // For cooltime and mission group
    public const string KILL_COOLDOWN = "KillCooldown";
    public const string MISSION_COOLDOWN = "MissionCooldown";
    public const string NUM_NPC_MISSION = "NumNPCMission";
    public const string NUM_SPECIAL_MISSION = "NumSpecialMission";
    public const string RANDOM_MISSION_INDEX = "RandomMissionIndex";

    // For character colors
    public const string REMAIN_COLOR_LIST = "RemainColorList";
    public const string USED_COLOR_LIST = "UsedColorList";
}

public class PlayerProperties
{
    // For the player role
    public const string PLAYER_TEAM = "PlayerTeam";
    public const string PLAYER_ROLE = "PlayerRole";

    // For starting the scene simultaneously
    public const string READY_TO_LOAD = "ReadyToLoad";

    // For player color
    public const string PLAYER_COLOR = "PlayerColor";

    // For player character's spawn position
    public const string SPAWN_POSITION_INDEX = "SpawnPositionIndex";

    // For player state
    public const string IS_DIE = "IsDie";

    // For counts of player's npc missions group
    public const string TOTAL_MISSION_COUNT = "TotalMissionCount";
    public const string MANAGER_NPC_MISSION_COUNT = "ManagerNPCMissionCount";
    public const string WATCHER_NPC_MISSION_COUNT = "WatcherNPCMissionCount";
    public const string REPAIR_NPC_MISSION_COUNT = "RepairNPCMissionCount";
    public const string TELEPORT_NPC_MISSION_COUNT = "TeleportNPCMissionCount";

    // For meeting
    public const string IS_VOTE = "IsVote";
    public const string VOTE_TARGET = "VoteTarget";
}
