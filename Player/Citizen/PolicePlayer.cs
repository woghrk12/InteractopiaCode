using Photon.Pun;

public class PolicePlayer : InGamePlayer
{
    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set max cooldown for skill
        skillCooldown = (int)PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.KILL_COOLDOWN];

        // Set the button event
        inputManager.SkillButton.Button.onClick.AddListener(Skill);
        localCharacter.AddInteractionEvent(
            EObjectType.CHARACTER,
            () =>
            {
                if (curSkillCooldown > 0f) return;
                inputManager.SkillButton.IsInteractable = true;
            },
            () =>
            {
                if (curSkillCooldown > 0f) return;
                if (localCharacter.characterCount > 0) return;
                inputManager.SkillButton.IsInteractable = false;
            });
    }

    public override void Skill()
    {
        int? actorNumber = localCharacter.Kill();

        if (actorNumber == null) return;

        ERoleType team = (ERoleType)GameManager.Network.PlayerDictionaryByActorNum[actorNumber.Value].CustomProperties[PlayerProperties.PLAYER_TEAM];
        if (team != ERoleType.CITIZEN)
        {
            localCharacter.Die(actorNumber.Value);
            inputManager.SkillButton.gameObject.SetActive(false);
        }
        else
        {
            int killer = PhotonNetwork.LocalPlayer.ActorNumber;
            if (GameManager.InGame.NPCList.TryGetValue(ENPCRole.SURVEILLANCE, out BaseNPC npc))
            {
                if (!npc.IsDie)
                {
                    (npc as WatcherBotNPC).WatchKiller(killer);
                }
            }
            
            curSkillCooldown = skillCooldown;
            skillCooldownCo = StartCoroutine(WaitSkillCooldown());
        }
    }

    protected override bool CheckCanUseSkill()
    {
        return localCharacter.characterCount > 0;
    }

    #endregion Override Methods
}
