using System;   
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Spawn Positions", fileName = "new SpawnPositions")]
public class SpawnPositionData : ScriptableObject
{
    #region Variables

    [SerializeField] private Vector3[] spawnPositions = null;

    #endregion Variables

    #region Methods

    public int GetLength()
    {
        return spawnPositions.Length;
    }

    public Vector3 GetPosition(int index)
    {
        if (index < 0 || index >= spawnPositions.Length)
        {
            throw new Exception($"Out of range. Input index : {index}");
        }

        return spawnPositions[index];
    }

    #endregion Methods
}
