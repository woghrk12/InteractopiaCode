using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    #region Variables

    private Rigidbody2D rigid2D = null;

    [SerializeField] private Transform spriteTransform = null;

    private float moveSpeed = 3f;

    private bool isMine = false;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();

        isMine = GetComponent<PhotonView>().IsMine;
    }

    #endregion Unity Events

    #region Methods

    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void MakeFootStepSound()
    {
        if (!isMine) return;

        SoundManager.Instance.SpawnEffect(ESoundKey.SFX_FOOTSTEP_Generic_Metal_Hollow_Run_RR1_mono);
    }

    public void MoveCharacter(Vector2 moveDirection)
    {
        // Set lookat direction
        if (moveDirection.x != 0f) 
        {
            spriteTransform.localScale = new Vector3(moveDirection.x < 0f ? 0.15f : -0.15f, 0.15f, 1f); 
        }

        rigid2D.MovePosition(rigid2D.position + moveSpeed * Time.fixedDeltaTime * moveDirection);
    }

    #endregion Methods
}
