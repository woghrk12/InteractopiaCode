using System;   
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collision2D))]
public class CharacterInteraction : MonoBehaviour
{
    #region Variables

    [SerializeField] private CircleCollider2D interactDetectionTrigger = null;
    [SerializeField] private CircleCollider2D interactableTrigger = null;

    private Dictionary<EObjectType, List<GameObject>> nearObjects = new();
    private Dictionary<EObjectType, Action> addObjectEvents = new();
    private Dictionary<EObjectType, Action> removeObjectEvents = new();

    #endregion Variables

    #region Properties

    public Dictionary<EObjectType, Action> AddObjectEvents => addObjectEvents;
    public Dictionary<EObjectType, Action> RemoveObjectEvents => removeObjectEvents;

    #endregion Properties

    #region Unity Events

    private void Awake()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            if (interactableTrigger != null)
            {
                Destroy(interactableTrigger.gameObject);
            }
        }
        else
        {
            Destroy(interactDetectionTrigger.gameObject);
        }

        int lastIdx = (int)EObjectType.END;

        for (int i = 1; i < lastIdx; i *= 2)
        {
            nearObjects.Add((EObjectType)i, new List<GameObject>());
            addObjectEvents.Add((EObjectType)i, null);
            removeObjectEvents.Add((EObjectType)i, null);
        }
    }

    #endregion Unity Events

    #region Methods

    public void DisableTrigger()
    {
        if (interactableTrigger == null) return;

        interactableTrigger.enabled = false;
    }

    public int GetCountObjects(int flag)
    {
        int count = 0;

        for (int index = 1; index < (int)EObjectType.END; index <<= 1)
        {
            if ((index & flag) == 0) continue;

            count += nearObjects[(EObjectType)index].Count;
        }

        return count;
    }

    public GameObject GetNearestObject(int flag)
    {
        float minDistance = float.MaxValue;
        Vector3 position = transform.position;
        GameObject nearestObject = null;

        for (int index = 1; index < (int)EObjectType.END; index <<= 1)
        {
            if ((index & flag) == 0) continue;

            EObjectType type = (EObjectType)index;
            foreach (GameObject nearObject in nearObjects[type])
            {
                float distanceSqr = Vector3.SqrMagnitude(position - nearObject.transform.position);

                if (minDistance > distanceSqr)
                {
                    minDistance = distanceSqr;
                    nearestObject = nearObject;
                }
            }
        }

        return nearestObject;
    }

    public void AddObject(EObjectType type, GameObject addObject)
    {
        if (nearObjects[type].Contains(addObject)) return;

        nearObjects[type].Add(addObject);
        addObjectEvents[type]?.Invoke();
    }

    public void RemoveObject(EObjectType type, GameObject removeObject)
    {
        if (!nearObjects[type].Contains(removeObject)) return;

        nearObjects[type].Remove(removeObject);
        removeObjectEvents[type]?.Invoke();
    }

    #endregion Methods

}
