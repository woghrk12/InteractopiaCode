using UnityEngine;
using Photon.Pun;

public class SpyPlayer : InGamePlayer
{
    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set max cooldown for kill
        killCooldown = (int)PhotonNetwork.CurrentRoom.CustomProperties[CustomProperties.KILL_COOLDOWN];
        skillCooldown = 30f;

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
        localCharacter.AddInteractionEvent(
            EObjectType.NPC,
            () =>
            {
                if (curSkillCooldown > 0f) return;
                inputManager.SkillButton.IsInteractable = true;
            },
            () =>
            {
                if (curSkillCooldown > 0f) return;
                if (localCharacter.npcCount > 0) return;
                inputManager.SkillButton.IsInteractable = false;
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

    public override void Skill()
    {
        GameObject npcObject = localCharacter.GetNearestObject((int)EObjectType.NPC);

        if (npcObject == null) return;

        npcObject.GetComponent<BaseNPC>().PopupMaxCountPlayer();

        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_TECH_INTERFACE_Computer_Beeps_02);

        curSkillCooldown = skillCooldown;
        skillCooldownCo = StartCoroutine(WaitSkillCooldown());
    }

    protected override bool CheckCanUseSkill()
    {
        return localCharacter.npcCount > 0;
    }

    #endregion Override Methods
}
