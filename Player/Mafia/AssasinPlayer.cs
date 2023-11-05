using UnityEngine;
using Photon.Pun;
using Photon.Realtime;  

public class AssasinPlayer : InGamePlayer
{
    #region Variables

    private int countAssasination = 2;

    #endregion Variables

    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set max cooldown for kill
        killCooldown = (int)PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.KILL_COOLDOWN];

        // Set the button event
        inputManager.KillButton.Button.onClick.AddListener(Kill);
        localCharacter.AddInteractionEvent(
            EObjectType.CHARACTER,
            () =>
            {
                if (curKillCooldown > 0f) return;
                inputManager.KillButton.IsInteractable = true;
            },
            () =>
            {
                if (curKillCooldown > 0f) return;
                if (localCharacter.characterCount > 0) return;
                inputManager.KillButton.IsInteractable = false;
            });

        inputManager.SkillButton.Button.onClick.AddListener(Skill);
    }

    public override void Kill()
    {
        int? actorNumber = localCharacter.Kill();

        if (actorNumber == null) return;

        int killer = PhotonNetwork.LocalPlayer.ActorNumber;
        if (GameManager.InGame.NPCList.TryGetValue(ENPCRole.SURVEILLANCE, out BaseNPC npc))
        {
            if (!npc.IsDie)
            {
                (npc as WatcherBotNPC).WatchKiller(killer);
            }
        }

        curKillCooldown = killCooldown;
        killCooldownCo = StartCoroutine(WaitKillCooldown());
    }

    public override void Skill()
    {
        GameManager.UI.PopupPanel<AssasinSelectPanel>();
    }

    #endregion Override Methods

    public void Assasinate(int actorNumber, ERoleType team, int role)
    {
        if (!GameManager.Network.PlayerDictionaryByActorNum.TryGetValue(actorNumber, out Player player)) return;

        ERoleType targetTeam = (ERoleType)player.CustomProperties[PlayerProperties.PLAYER_TEAM];
        int targetRole = (int)player.CustomProperties[PlayerProperties.PLAYER_ROLE];

        int dieActorNumber = -1;
        if (targetTeam == team && targetRole == role)
        {
            dieActorNumber = actorNumber;
            countAssasination--;

            if (countAssasination <= 0)
            {
                GameManager.Input.SkillButton.gameObject.SetActive(false);
            }
            else
            {
                GameManager.Input.SkillButton.IsInteractable = false;
            }
        }
        else
        {
            dieActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            GameManager.Input.SkillButton.gameObject.SetActive(false);
        }

        GameManager.InGame.CharacterObjectDictionary[dieActorNumber].Die(-1);
        GameManager.Meeting.OnKillPlayer(dieActorNumber);
    }
}
