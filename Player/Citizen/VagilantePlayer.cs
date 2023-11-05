using Photon.Pun;

public class VagilantePlayer : InGamePlayer
{
    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set the button event
        inputManager.SkillButton.Button.onClick.AddListener(Skill);
        localCharacter.AddInteractionEvent(
            EObjectType.CHARACTER,
            () => inputManager.SkillButton.IsInteractable = true,
            () =>
            {
                if (localCharacter.characterCount > 0) return;
                inputManager.SkillButton.IsInteractable = false;
            });
    }

    public override void Skill()
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

        inputManager.SkillButton.gameObject.SetActive(false);
    }

    #endregion Override Methods
}
