using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class MeetingManager : MonoBehaviour
{
    public enum ECauseMeeting { NONE = -1, DEADBODYREPORT, DEADNPCREPORT, EMERGENCYMEETINGCALL }

    #region Variables

    private PhotonView managerPhotonview = null;

    [Header("For emergency meeting")]
    private Coroutine emergencyCooldownCo = null;
    private float emergencyMeetingCooldown = 0f;
    private float curEmergencyCooldown = 0f;

    [Header("For meeting opening")]
    private ECauseMeeting causeMeeting = ECauseMeeting.NONE;
    private int reporter = -1;
    private int deadBody = -1;

    [Header("For meeting")]
    private Coroutine meetingCo = null;
    private MeetingPanel meetingPanel = null;
    private float meetingTime = 0f;

    [Header("For voting")]
    private float votingTime = 0f;
    public Action<int, int> VoteEvent = null;

    [Header("For meeting end")]
    private bool isSkipMeeting = false;
    public int KickedPlayerActorNum = -1;

    #endregion Variables

    #region Properties

    public ECauseMeeting CauseMeeting => causeMeeting;
    public int Reporter => reporter;
    public int DeadBody => deadBody;

    public bool IsSkipMeeting => isSkipMeeting;

    #endregion Properties

    #region Methods

    public void Init()
    {
        managerPhotonview = GetComponent<PhotonView>();

        causeMeeting = ECauseMeeting.NONE;
        reporter = -1;
        deadBody = -1;

        PhotonHashTable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;

        emergencyMeetingCooldown = (int)roomSetting[CustomProperties.EMERGENCY_MEETING_COOLDOWN];

        meetingTime = (int)roomSetting[CustomProperties.MEETING_TIME];

        votingTime = (int)roomSetting[CustomProperties.VOTE_TIME];
        isSkipMeeting = false;

        KickedPlayerActorNum = -1;
    }

    #region Enter Meeting

    public void Report(int deadPlayer)
    {
        int reporter = PhotonNetwork.LocalPlayer.ActorNumber;

        managerPhotonview.RPC(nameof(EnterMeetingRPC), RpcTarget.AllViaServer, (int)ECauseMeeting.DEADBODYREPORT, reporter, deadPlayer);
    }

    public void Report(ENPCRole roleType)
    {
        int reporter = PhotonNetwork.LocalPlayer.ActorNumber;

        managerPhotonview.RPC(nameof(EnterMeetingRPC), RpcTarget.AllViaServer, (int)ECauseMeeting.DEADNPCREPORT, reporter, (int)roleType);
    }

    public void EmergencyCall()
    {
        int reporter = PhotonNetwork.LocalPlayer.ActorNumber;

        managerPhotonview.RPC(nameof(EnterMeetingRPC), RpcTarget.AllViaServer, (int)ECauseMeeting.EMERGENCYMEETINGCALL, reporter, -1);
    }

    [PunRPC]
    private void EnterMeetingRPC(int cause, int reporter, int deadBody)
    {
        causeMeeting = (ECauseMeeting)cause;
        this.reporter = reporter;
        this.deadBody = deadBody;

        isSkipMeeting = false;
        KickedPlayerActorNum = -1;

        // Stop the cooldown of player
        GameManager.InGame.LocalPlayer.StopCooldown();

        // Stop the cooldown of missions
        Dictionary<EMissionType, List<MissionObject>> missionObjectDictionary = GameManager.InGame.MissionObjectDictionary;
        foreach (KeyValuePair<EMissionType, List<MissionObject>> missionList in missionObjectDictionary)
        {
            foreach (MissionObject missionObject in missionList.Value)
            {
                missionObject.StopCooldown();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (causeMeeting == ECauseMeeting.EMERGENCYMEETINGCALL)
            {
                // Set the cooldown of emergency meeting
                curEmergencyCooldown = emergencyMeetingCooldown;
            }
            else
            {
                // Stop the cooldown of emergency meeting
                StopEmergencyCooldown();
            }
        }

        // Stop the cooldown of npcs
        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in GameManager.InGame.NPCList)
        {
            npc.Value.EnterMeeting();
        }

        GameManager.Vivox.BlockVoice();

        if (GameManager.InGame.LocalPlayer.CurTask != null)
        {
            GameManager.InGame.LocalPlayer.CurTask.CloseTaskPanel();
        }

        GameManager.UI.CloseAllPanel(false);
        GameManager.UI.OpenPanel<MeetingOpeningPanel>();
    }

    #endregion Enter Meeting

    #region Emergency Meeting

    public void StartEmergencyCooldown()
    {
        if (curEmergencyCooldown > 0f)
        {
            emergencyCooldownCo = StartCoroutine(WaitEmergencyCooldown());
        }
    }

    public void StopEmergencyCooldown()
    {
        if (emergencyCooldownCo != null)
        {
            StopCoroutine(emergencyCooldownCo);
            emergencyCooldownCo = null;
        }
    }

    private IEnumerator WaitEmergencyCooldown()
    {
        managerPhotonview.RPC(nameof(BlockEmergencyMeeting), RpcTarget.AllViaServer);
        managerPhotonview.RPC(nameof(SetEmergencyCooldown), RpcTarget.AllViaServer, curEmergencyCooldown);

        while (curEmergencyCooldown > 0)
        {
            yield return Utilities.WaitForSeconds(0.1f);
            curEmergencyCooldown -= 0.1f;
            managerPhotonview.RPC(nameof(SetEmergencyCooldown), RpcTarget.AllViaServer, curEmergencyCooldown);
        }

        managerPhotonview.RPC(nameof(EnableEmergencyMeeting), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void SetEmergencyCooldown(float cooldown)
    {
        GameManager.UI.GetPanel<EmergencyMeetingPanel>().SetCooldownText(cooldown);
    }

    [PunRPC]
    private void BlockEmergencyMeeting()
    {
        GameManager.UI.GetPanel<EmergencyMeetingPanel>().BlockCall();
    }

    [PunRPC]
    private void EnableEmergencyMeeting()
    {
        GameManager.UI.GetPanel<EmergencyMeetingPanel>().EnableCall();
    }

    #endregion Emergency Meeting

    #region Start Meeting

    public void StartMeeting()
    {
        GameManager.Network.ReadyToLoad();
        GameManager.InGame.LocalPlayer.IsSetPosition = false;

        if (!PhotonNetwork.IsMasterClient) return;

        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            PhotonHashTable playerSetting = player.Value.CustomProperties;
            bool isDie = (bool)playerSetting[PlayerProperties.IS_DIE];

            playerSetting[PlayerProperties.IS_VOTE] = isDie;
            playerSetting[PlayerProperties.VOTE_TARGET] = -1;

            player.Value.SetCustomProperties(playerSetting);
        }

        StartCoroutine(StartMeetingCo());
    }

    private IEnumerator StartMeetingCo()
    {
        yield return GameManager.Network.CheckReadyCo();

        managerPhotonview.RPC(nameof(StartMeetingRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void StartMeetingRPC()
    {
        GameManager.Vivox.ReleaseVoice();

        meetingPanel = GameManager.UI.GetPanel<MeetingPanel>();
        GameManager.UI.OpenPanel<MeetingPanel>();

        if (PhotonNetwork.IsMasterClient)
        {
            meetingCo = StartCoroutine(WaitMeetingTime());
        }
    }

    #endregion Start Meeting

    [PunRPC]
    private void SetMeetingRPC()
    {
        meetingPanel.MeetingStatusText.text = "회의";
        meetingPanel.BlockVote();
    }

    [PunRPC]
    private void SetVotingRPC()
    {
        meetingPanel.MeetingStatusText.text = "투표";

        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.IS_DIE]) return;

        meetingPanel.EnableVote();
    }

    [PunRPC]
    private void SetTimerRPC(float maxTime, float curTime)
    {
        meetingPanel.SetTimer(maxTime, curTime);
    }

    #region Meeting

    private IEnumerator WaitMeetingTime()
    {
        managerPhotonview.RPC(nameof(SetMeetingRPC), RpcTarget.All);
        
        float time = meetingTime;
        managerPhotonview.RPC(nameof(SetTimerRPC), RpcTarget.All, meetingTime, time);

        while (time > 0)
        {
            yield return Utilities.WaitForSeconds(0.1f);
            time -= 0.1f;
            managerPhotonview.RPC(nameof(SetTimerRPC), RpcTarget.All, meetingTime, time);
        }

        meetingCo = StartCoroutine(WaitVoteTime());
    }

    #endregion Meeting

    #region Vote

    private IEnumerator WaitVoteTime()
    {
        managerPhotonview.RPC(nameof(SetVotingRPC), RpcTarget.All);

        float time = votingTime;
        managerPhotonview.RPC(nameof(SetTimerRPC), RpcTarget.All, votingTime, time);

        while (time > 0)
        {
            yield return Utilities.WaitForSeconds(0.1f);
            time -= 0.1f;
            managerPhotonview.RPC(nameof(SetTimerRPC), RpcTarget.All, votingTime, time);
        }

        CalculateVoteResult();
    }

    public void Vote(int target)
    {
        int voter = PhotonNetwork.LocalPlayer.ActorNumber;

        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        
        playerSetting[PlayerProperties.IS_VOTE] = true;
        playerSetting[PlayerProperties.VOTE_TARGET] = target;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);

        managerPhotonview.RPC(nameof(VoteRPC), RpcTarget.AllViaServer, voter, target);
    }

    [PunRPC]
    private void VoteRPC(int voter, int target)
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_SUCCESS_BEEPS_Single_Tone_Short_07);

        VoteEvent?.Invoke(voter, target);

        if (PhotonNetwork.IsMasterClient)
        {
            CheckAllPlayerVote();
        }
    }

    #endregion Vote

    #region Kill Events

    public void OnKillPlayer(int actorNumber)
    {
        Player player = GameManager.Network.PlayerDictionaryByActorNum[actorNumber];
        PhotonHashTable playerSetting = player.CustomProperties;

        playerSetting[PlayerProperties.IS_VOTE] = true;
        playerSetting[PlayerProperties.VOTE_TARGET] = -1;

        player.SetCustomProperties(playerSetting);

        managerPhotonview.RPC(nameof(OnKillPlayerRPC), RpcTarget.AllViaServer, actorNumber);
    }

    [PunRPC]
    private void OnKillPlayerRPC(int actorNumber)
    {
        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_FIREARM_Shotgun_Model_01b_Fire_Single_RR2_stereo);

        GameManager.UI.GetPanel<MeetingPanel>().DeactiveStateButton(actorNumber);

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            GameManager.UI.GetPanel<MeetingPanel>().BlockVote();
        }

        if (GameManager.InGame.CheckGameEndByAllMafiaDead())
        {
            GameManager.InGame.EndGame();
        }
        else if (GameManager.InGame.CheckGameEndByNumber())
        {
            GameManager.InGame.EndGame();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CheckAllPlayerVote();
        }
    }

    #endregion Kill Events

    public void CheckAllPlayerVote()
    {
        // Check all players vote or die
        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            if (!(bool)player.Value.CustomProperties[PlayerProperties.IS_VOTE]) return;
        }

        CalculateVoteResult();
    }

    public void CalculateVoteResult()
    {
        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;

        // Synthesize the results of the meeting
        Dictionary<int, int> voteResultDictionary = new();
        int countSkip = 0;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            if ((bool)player.Value.CustomProperties[PlayerProperties.IS_DIE]) continue;

            int voteTarget = (int)player.Value.CustomProperties[PlayerProperties.VOTE_TARGET];

            // Skip
            if (voteTarget < 0)
            {
                countSkip++;
                continue;
            }

            // Target player die
            if ((bool)playerDictionary[voteTarget].CustomProperties[PlayerProperties.IS_DIE]) continue;

            if (voteResultDictionary.ContainsKey(voteTarget))
            {
                voteResultDictionary[voteTarget]++;
            }
            else
            {
                voteResultDictionary.Add(voteTarget, 1);
            }
        }

        // Check the results of the meeting
        int maxVoteCount = 0;
        int kickPlayerActorNubmer = -1;
        bool isSameCount = false;
        foreach (KeyValuePair<int, int> vote in voteResultDictionary)
        {
            if (vote.Value > maxVoteCount)
            {
                maxVoteCount = vote.Value;
                kickPlayerActorNubmer = vote.Key;
                isSameCount = false;
            }
            else if (vote.Value == maxVoteCount)
            {
                isSameCount = true;
            }
        }

        managerPhotonview.RPC(nameof(CheckAllPlayerVoteRPC), RpcTarget.AllViaServer, kickPlayerActorNubmer, countSkip >= maxVoteCount || isSameCount);
    }

    [PunRPC]
    private void CheckAllPlayerVoteRPC(int kickedPlayer, bool isSkipMeeting)
    {
        KickedPlayerActorNum = kickedPlayer;
        this.isSkipMeeting = isSkipMeeting;

        if (meetingCo != null)
        {
            StopCoroutine(meetingCo);
            meetingCo = null;
        }

        GameManager.UI.OpenPanel<MeetingResultPanel>();
    }

    #endregion Methods
}
