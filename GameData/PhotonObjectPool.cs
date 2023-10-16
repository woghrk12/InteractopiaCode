using System;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(menuName = "Game Data/Photon Object Pool", fileName = "new PhotonObjectPool")]
public class PhotonObjectPool : ScriptableObject
{
    #region Variables

    [SerializeField] private GameObject[] prefabs = null;

    #endregion Variables

    #region Methods

    public void InitPool()
    {
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;

        if (pool == null) { throw new Exception("Null exception : PhotonNetwork.PrefabPool is null!"); }
        if (prefabs == null) { throw new Exception("Null exception : prefabs is null!"); }

        foreach (GameObject prefab in prefabs)
        {
            if (pool.ResourceCache.ContainsKey(prefab.name))
            {
                pool.ResourceCache[prefab.name] = prefab;
            }
            else
            {
                pool.ResourceCache.Add(prefab.name, prefab);
            }
        }
    }

    #endregion Methods
}
