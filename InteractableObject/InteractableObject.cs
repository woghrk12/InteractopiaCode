using UnityEngine;

public enum EObjectType
{
    NONE = 0,
    CHARACTER = 1 << 0,
    BODY = 1 << 1,
    OBJECT = 1 << 2,
    NPC = 1 << 3,
    NPCBODY = 1 << 4,
    END = 1 << 5
}

public abstract class InteractableObject : MonoBehaviour
{
    #region Variables

    [SerializeField] private SpriteRenderer highlightedSprite = null;
    [SerializeField] private SpriteRenderer outlineSprite = null;

    #endregion Variables

    #region Methods

    // call on button touched
    public abstract void OnInteract();

    // call on ColliderEnter
    public virtual void OnEnter()
    {
        highlightedSprite.material.SetFloat("_Highlighted", 1f);
        outlineSprite.enabled = true;
    }

    // call on ColliderExit
    public virtual void OnExit()
    {
        highlightedSprite.material.SetFloat("_Highlighted", 0f);
        outlineSprite.enabled = false;
    }

    #endregion Methods
}
