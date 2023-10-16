using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    #region Variables

    private Rigidbody2D rigid2D = null;

    [SerializeField] private Transform spriteTransform = null;
    [SerializeField] private float moveSpeed = 0.0f;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    #endregion Unity Events

    #region Methods

    public void MoveCharacter(Vector2 moveDirection)
    {
        // Set lookat direction
        if (moveDirection.x != 0f) 
        {
            spriteTransform.localScale = new Vector3(moveDirection.x < 0f ? -1f : 1f, 1f, 1f); 
        }

        rigid2D.MovePosition(rigid2D.position + moveSpeed * Time.fixedDeltaTime * moveDirection);
    }

    #endregion Methods
}
