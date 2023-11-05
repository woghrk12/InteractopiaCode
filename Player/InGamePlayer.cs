using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class InGamePlayer : MonoBehaviour
{
    #region Variables

    private VivoxManager vivoxManager = null;
    protected InputManager inputManager = null;

    private CameraFollow cameraFollow = null;
    protected InGameCharacter localCharacter = null;

    [HideInInspector] public BaseTaskTracker CurTask = null;

    [HideInInspector] public Vector2 sightDirection = Vector2.right;

    private bool canMove = true;
    private bool isSetPosition = false;

    [Header("Variables for setting vivox position")]
    private float updateTime = 0.0f;

    [Header("Variables for kill cooldown")]
    protected float killCooldown = 0f;
    protected float curKillCooldown = 0f;
    public Action killCooldownStartEvent = null;
    public Action<float> killCooldownTickEvent = null;
    public Action<bool> killCooldownEndEvent = null;
    protected Coroutine killCooldownCo = null;

    [Header("Variables for skill cooldown")]
    protected float skillCooldown = 0f;
    protected float curSkillCooldown = 0f;
    public Action skillCooldownStartEvent = null;
    public Action<float> skillCooldownTickEvent = null;
    public Action<bool> skillCooldownEndEvent = null;
    protected Coroutine skillCooldownCo = null;

    #endregion Variables

    #region Properties

    public InGameCharacter LocalCharacter => localCharacter;

    public int PlayerTeam { private set; get; } = -1;
    public int PlayerRole { private set; get; } = -1;

    public bool CanMove 
    {
        set
        {
            canMove = value;

            if(!canMove)
            {
                localCharacter.StopMove();
            }
        }
        get => canMove;
    }

    public bool IsSetPosition
    {
        set
        {
            isSetPosition = value;

            if (!isSetPosition)
            {
                GameManager.Vivox.SetPosition();
                updateTime = 0f;
            }
        }
        get => isSetPosition;
    }

    #endregion Properties

    #region Unity Events

    protected void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        
        // Save team and role info of the local player
        PhotonHashTable playerSetting = PhotonNetwork.LocalPlayer.CustomProperties;
        PlayerTeam = (int)playerSetting[PlayerProperties.PLAYER_TEAM];
        PlayerRole = (int)playerSetting[PlayerProperties.PLAYER_ROLE];

        // Init the character state
        if (playerSetting.ContainsKey(PlayerProperties.IS_DIE))
        {
            playerSetting[PlayerProperties.IS_DIE] = false;
        }
        else
        {
            playerSetting.Add(PlayerProperties.IS_DIE, false);
        }

        // Init the vote state
        if (playerSetting.ContainsKey(PlayerProperties.IS_VOTE))
        {
            playerSetting[PlayerProperties.IS_VOTE] = false;
        }
        else
        {
            playerSetting.Add(PlayerProperties.IS_VOTE, false);
        }
        if (playerSetting.ContainsKey(PlayerProperties.VOTE_TARGET))
        {
            playerSetting[PlayerProperties.VOTE_TARGET] = -1;
        }
        else
        {
            playerSetting.Add(PlayerProperties.VOTE_TARGET, -1);
        }

        // Init the mission state
        if (playerSetting.ContainsKey(PlayerProperties.TOTAL_MISSION_COUNT))
        {
            playerSetting[PlayerProperties.TOTAL_MISSION_COUNT] = 0;
        }
        else
        {
            playerSetting.Add(PlayerProperties.TOTAL_MISSION_COUNT, 0);
        }
        if (playerSetting.ContainsKey(PlayerProperties.MANAGER_NPC_MISSION_COUNT))
        {
            playerSetting[PlayerProperties.MANAGER_NPC_MISSION_COUNT] = 0;
        }
        else
        {
            playerSetting.Add(PlayerProperties.MANAGER_NPC_MISSION_COUNT, 0);
        }
        if (playerSetting.ContainsKey(PlayerProperties.WATCHER_NPC_MISSION_COUNT))
        {
            playerSetting[PlayerProperties.WATCHER_NPC_MISSION_COUNT] = 0;
        }
        else
        {
            playerSetting.Add(PlayerProperties.WATCHER_NPC_MISSION_COUNT, 0);
        }
        if (playerSetting.ContainsKey(PlayerProperties.REPAIR_NPC_MISSION_COUNT))
        {
            playerSetting[PlayerProperties.REPAIR_NPC_MISSION_COUNT] = 0;
        }
        else
        {
            playerSetting.Add(PlayerProperties.REPAIR_NPC_MISSION_COUNT, 0);
        }
        if (playerSetting.ContainsKey(PlayerProperties.TELEPORT_NPC_MISSION_COUNT))
        {
            playerSetting[PlayerProperties.TELEPORT_NPC_MISSION_COUNT] = 0;
        }
        else
        {
            playerSetting.Add(PlayerProperties.TELEPORT_NPC_MISSION_COUNT, 0);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSetting);
    }

    protected void Update()
    {
        if (localCharacter == null) return;
        if (!isSetPosition) return;

        // Set the timer for vivox position
        if (updateTime > 0.0f)
        {
            updateTime -= Time.deltaTime;
        }
        else
        {
            updateTime = 0.3f;
            vivoxManager.SetPosition(localCharacter.CharacterTransform);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (localCharacter == null) return;
        if (inputManager.PlayerInput == null) return;
        if (!canMove) return;

        // Character Sight
        Vector2 direction = inputManager.PlayerInput.MoveDirection;
        localCharacter.Move(direction);

        if (direction != Vector2.zero)
        {
            sightDirection = direction;
        }
    }

    protected void LateUpdate()
    {
        if (localCharacter == null) return;
        if (inputManager.PlayerInput == null) return;

        Vector2 direction = inputManager.PlayerInput.MoveDirection == Vector2.zero ? sightDirection : inputManager.PlayerInput.MoveDirection;
        localCharacter.See(direction);
    }

    #endregion Unity Events
    
    #region Methods

    public virtual void InitPlayer(InGameCharacter character)
    {
        // Set the manager components
        vivoxManager = GameManager.Vivox;
        inputManager = GameManager.Input;

        // Set the local character
        localCharacter = character;

        // Set the camera focus
        cameraFollow.SetTarget(localCharacter.transform);

        // Init the input device
        inputManager.SetPlayerTransform(localCharacter.transform);
        inputManager.SetInputDevice();

        // Add events for the player's button
        inputManager.UseButton.Button.onClick.AddListener(Use);
        localCharacter.AddInteractionEvent(
            EObjectType.OBJECT,
            () => inputManager.UseButton.IsInteractable = true,
            () => 
            {
                if (localCharacter.objectCount > 0) return;
                inputManager.UseButton.IsInteractable = false;
            });
        localCharacter.AddInteractionEvent(
            EObjectType.NPC,
            () => inputManager.UseButton.IsInteractable = true,
            () =>
            {
                if (localCharacter.objectCount > 0) return;
                inputManager.UseButton.IsInteractable = false;
            });
        inputManager.ReportButton.Button.onClick.AddListener(Report);
        localCharacter.AddInteractionEvent(
            EObjectType.BODY,
            () => inputManager.ReportButton.IsInteractable = true,
            () =>
            {
                if (localCharacter.bodyCount > 0) return;
                inputManager.ReportButton.IsInteractable = false;
            });
        localCharacter.AddInteractionEvent(
            EObjectType.NPCBODY,
            () => inputManager.ReportButton.IsInteractable = true,
            () =>
            {
                if (localCharacter.bodyCount > 0) return;
                inputManager.ReportButton.IsInteractable = false;
            });
    }

    public void StartCooldown()
    {
        if (curKillCooldown > 0f)
        {
            killCooldownCo = StartCoroutine(WaitKillCooldown());
        }

        if (curSkillCooldown > 0f)
        {
            skillCooldownCo = StartCoroutine(WaitSkillCooldown());
        }
    }

    public virtual void StopCooldown()
    {
        if (killCooldownCo != null)
        {
            StopCoroutine(killCooldownCo);
            killCooldownCo = null;
        }
        if (skillCooldownCo != null)
        {
            StopCoroutine(skillCooldownCo);
            skillCooldownCo = null;
        }
    }

    #endregion Methods

    #region Player Action

    public void Use() => localCharacter.Use();

    public void Report() => localCharacter.Report();

    public virtual void Kill() { }

    public virtual void Skill() { }

    #endregion Player Action

    #region Cooldown Methods

    protected IEnumerator WaitKillCooldown()
    {
        killCooldownStartEvent?.Invoke();

        while (curKillCooldown > 0)
        {
            curKillCooldown -= Time.fixedDeltaTime;
            killCooldownTickEvent?.Invoke(curKillCooldown);
            
            yield return Utilities.WaitForFixedUpdate;
        }

        curKillCooldown = 0f;
        killCooldownCo = null;
        killCooldownEndEvent?.Invoke(localCharacter.characterCount > 0);
    }

    protected IEnumerator WaitSkillCooldown()
    {
        skillCooldownStartEvent?.Invoke();

        while (curSkillCooldown > 0)
        {
            curSkillCooldown -= Time.fixedDeltaTime;
            skillCooldownTickEvent?.Invoke(curSkillCooldown);

            yield return Utilities.WaitForFixedUpdate;
        }

        curSkillCooldown = 0f;
        skillCooldownCo = null;

        skillCooldownEndEvent?.Invoke(CheckCanUseSkill());
    }

    #endregion Cooldown Methods

    #region Helper Methods

    /// <summary>
    /// Check the player can use the skill after waiting skill cooldown
    /// </summary>
    protected virtual bool CheckCanUseSkill()
    {
        return true;
    }

    #endregion Helper Methods
}
