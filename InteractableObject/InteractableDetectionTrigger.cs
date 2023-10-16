using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetectionTrigger : MonoBehaviour
{
    #region Variables

    [SerializeField] private CharacterInteraction characterInteraction = null;

    #endregion Variables

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject interactableObject = collision.transform.parent.gameObject;

        if (interactableObject.CompareTag("Character"))
        {
            characterInteraction.AddObject(EObjectType.CHARACTER, interactableObject);
        }
        else if (interactableObject.CompareTag("Body"))
        {
            characterInteraction.AddObject(EObjectType.BODY, interactableObject);
        }
        else if (interactableObject.CompareTag("Object"))
        {
            characterInteraction.AddObject(EObjectType.OBJECT, interactableObject);
        }
        else if (interactableObject.CompareTag("NPC"))
        {
            characterInteraction.AddObject(EObjectType.NPC, interactableObject);
        }
        else if (interactableObject.CompareTag("NPCBody"))
        {
            characterInteraction.AddObject(EObjectType.NPCBODY, interactableObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject interactableObject = collision.transform.parent.gameObject;

        if (interactableObject.CompareTag("Character"))
        {
            characterInteraction.RemoveObject(EObjectType.CHARACTER, interactableObject);
        }
        else if (interactableObject.CompareTag("Body"))
        {
            characterInteraction.RemoveObject(EObjectType.BODY, interactableObject);
        }
        else if (interactableObject.CompareTag("Object"))
        {
            characterInteraction.RemoveObject(EObjectType.OBJECT, interactableObject);
        }
        else if (interactableObject.CompareTag("NPC"))
        {
            characterInteraction.RemoveObject(EObjectType.NPC, interactableObject);
        }
        else if (interactableObject.CompareTag("NPCBody"))
        {
            characterInteraction.RemoveObject(EObjectType.NPCBODY, interactableObject);
        }
    }
}
