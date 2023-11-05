using UnityEngine;

public class ManipulatorPlayer : InGamePlayer
{
    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set max cooldown for skill
        skillCooldown = 60f;

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
        GameObject npcObject = localCharacter.GetNearestObject((int)EObjectType.NPC);

        if (npcObject == null) return;

        npcObject.GetComponent<BaseNPC>().SetBomb();

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
