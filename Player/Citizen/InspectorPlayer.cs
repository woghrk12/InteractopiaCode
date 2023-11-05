using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorPlayer : InGamePlayer
{
    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set max cooldown for skill
        skillCooldown = 20f;

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
        GameObject characterObject = localCharacter.GetNearestObject((int)EObjectType.CHARACTER);

        if (characterObject == null) return;

        characterObject.GetComponent<InGameCharacter>().ShowMissionCount();

        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_TECH_INTERFACE_Computer_Beeps_02);

        curSkillCooldown = skillCooldown;
        skillCooldownCo = StartCoroutine(WaitSkillCooldown());
    }

    protected override bool CheckCanUseSkill()
    {
        return localCharacter.characterCount > 0;
    }

    #endregion Override Methods
}
