using UnityEngine;
using Photon.Pun;

public class CharacterPositionObject : MonoBehaviourPunCallbacks
{
    #region Variables

    private SpriteRenderer spriteRenderer = null;
    private Transform objectTransform = null;

    private Transform followTransform = null;
    private Vector3 offset = Vector3.zero;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        if (followTransform == null) return;

        objectTransform.position = followTransform.position + offset;
    }

    #endregion Unity Events

    #region Methods

    public void SetObject(Transform transform, Vector3 offset, Color color)
    {
        followTransform = transform;
        this.offset = offset;
        spriteRenderer.color = color;
    }

    #endregion Methods
}
