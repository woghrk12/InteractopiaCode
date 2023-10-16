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

    [Header("For meeting opening")]
    private ECauseMeeting causeMeeting = ECauseMeeting.NONE;
    private int reporter = -1;
    private int deadBody = -1;

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

        // Stop the cooldown of npcs
        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in GameManager.InGame.NPCList)
        {
            npc.Value.EnterMeeting();
        }

        GameManager.Vivox.BlockVoice();

        GameManager.UI.ClosePanel<InGamePanel>(false);
        GameManager.UI.OpenPanel<MeetingOpeningPanel>();
    }

    #endregion Enter Meeting

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

        GameManager.UI.OpenPanel<MeetingPanel>();
    }

    #endregion Start Meeting

    #region Vote

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
        GameManager.UI.GetPanel<MeetingPanel>().DeactiveStateButton(actorNumber);

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            GameManager.UI.GetPanel<MeetingPanel>().DisableStateButtons();
        }

        if (GameManager.InGame.CheckGameEnd())
        {
            GameManager.InGame.EndGame();
            return;
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
        int kickPlayerActorNubmer = 0;
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

        GameManager.UI.OpenPanel<MeetingResultPanel>();
    }

    #endregion Methods
}
