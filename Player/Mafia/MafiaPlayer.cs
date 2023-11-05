using Photon.Pun;

public class MafiaPlayer : InGamePlayer
{
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

    #endregion Override Methods
}
