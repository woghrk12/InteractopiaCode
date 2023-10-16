using UnityEngine;

public enum EObjectType
{
    NONE = 0,
    CHARACTER = 1 << 0,
    BODY = 1 << 1,
    OBJECT = 1 << 2,
    NPC = 1 << 3,
    NPCBODY = 1<< 4,
    END = 1 << 5
}

public abstract class InteractableObject : MonoBehaviour
{
    // call on button touched
    public abstract void OnInteract();

    // call on ColliderEnter
    public abstract void OnEnter();

    // call on ColliderExit
    public abstract void OnExit();
}
