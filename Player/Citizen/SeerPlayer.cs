using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SeerPlayer : InGamePlayer
{
    #region Variables

    private float skillTime = 10f;
    private Coroutine skillCo = null;

    private SpriteRenderer black = null;

    private Dictionary<int, CharacterPositionObject> redObjectDictionary = new();
    private Dictionary<int, CharacterPositionObject> yellowObjectDictionary = new();

    #endregion Variables

    #region Override Methods

    public override void InitPlayer(InGameCharacter character)
    {
        base.InitPlayer(character);

        // Set the black sight sprite of camera
        black = Camera.main.GetComponentInChildren<SpriteRenderer>();

        // Set max cooldown for skill
        skillCooldown = 30f;

        // Set the button event
        inputManager.SkillButton.Button.onClick.AddListener(Skill);
        inputManager.SkillButton.IsInteractable = true;

        // Instantiate the red object for character
        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            CharacterPositionObject positionObject = GameManager.Resource.Instantiate("Prefab/PositionObject", transform).GetComponent<CharacterPositionObject>();
            positionObject.gameObject.SetActive(false);
            redObjectDictionary.Add(player.Key, positionObject);
        }

        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        { 
            CharacterPositionObject positionObject = GameManager.Resource.Instantiate("Prefab/PositionObject", transform).GetComponent<CharacterPositionObject>();
            positionObject.gameObject.SetActive(false);
            yellowObjectDictionary.Add((int)npc.Key, positionObject);
        }
        
        // Instantiate the yellow object for npc
        // TODO : Instantiate the yellow object for npc
    }

    public override void Skill()
    {
        curSkillCooldown = skillCooldown;
        inputManager.SkillButton.IsInteractable = false;

        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_TECH_INTERFACE_Computer_Beeps_02);

        skillCo = StartCoroutine(SeeObject());
    }

    public override void StopCooldown()
    {
        base.StopCooldown();

        if (skillCo != null)
        {
            StopCoroutine(skillCo);
            TurnOffObject();
            skillCo = null;
        }
    }

    #endregion Override Methods

    #region Helper Methods

    private void TurnOnObject()
    {
        black.color = new Color(0f, 0.2f, 0f, 0.8f);

        Dictionary<int, Player> playerDictionary = GameManager.Network.PlayerDictionaryByActorNum;
        Dictionary<int, InGameCharacter> characterDictionary = GameManager.InGame.CharacterObjectDictionary;

        foreach (KeyValuePair<int, Player> player in playerDictionary)
        {
            if ((bool)player.Value.CustomProperties[PlayerProperties.IS_DIE]) continue;

            redObjectDictionary[player.Key].SetObject(characterDictionary[player.Key].transform, new Vector3(0f, 0.2f, 0f), Color.red);
            characterDictionary[player.Key].dieEvent = (int actorNumber) => redObjectDictionary[actorNumber].gameObject.SetActive(false);
            redObjectDictionary[player.Key].gameObject.SetActive(true);
        }

        Dictionary<ENPCRole, BaseNPC> npcList = GameManager.InGame.NPCList;
        foreach (KeyValuePair<ENPCRole, BaseNPC> npc in npcList)
        {
            if (npc.Value.IsDie) continue;

            yellowObjectDictionary[(int)npc.Key].SetObject(npcList[npc.Key].transform, new Vector3(0f, 0.2f, 0f), Color.yellow);
            npcList[npc.Key].DieEvent = (ENPCRole npcType) => yellowObjectDictionary[(int)npcType].gameObject.SetActive(false);
            npcList[npc.Key].RepairEvent = (ENPCRole npcType) => yellowObjectDictionary[(int)npcType].gameObject.SetActive(true);
            yellowObjectDictionary[(int)npc.Key].gameObject.SetActive(true);
        }
    }

    private void TurnOffObject()
    {
        black.color = new Color(0f, 0f, 0f, 0.8f);

        foreach (KeyValuePair<int, CharacterPositionObject> redObject in redObjectDictionary)
        {
            redObject.Value.gameObject.SetActive(false);
        }

        foreach (KeyValuePair<int, CharacterPositionObject> yellowObject in yellowObjectDictionary)
        {
            yellowObject.Value.gameObject.SetActive(false);
        }
    }

    #endregion Helper Methods

    #region Coroutine Methods

    private IEnumerator SeeObject()
    {
        TurnOnObject();

        yield return Utilities.WaitForSeconds(skillTime);

        TurnOffObject();
        skillCooldownCo = StartCoroutine(WaitSkillCooldown());
    }

    #endregion Coroutine Methods
}
