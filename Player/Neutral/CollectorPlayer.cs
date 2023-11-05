using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CollectorPlayer : InGamePlayer
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

    public override void Skill()
    {
        GameObject targetNPC = localCharacter.GetNearestObject((int)EObjectType.NPC);

        if (targetNPC == null) return;

        int killer = PhotonNetwork.LocalPlayer.ActorNumber;
        targetNPC.GetComponent<BaseNPC>().Die(killer);

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

    protected override bool CheckCanUseSkill()
    {
        return localCharacter.npcCount > 0;
    }

    #endregion Override Methods
}
