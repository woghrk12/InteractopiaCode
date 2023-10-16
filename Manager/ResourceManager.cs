using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }

            GameObject gameObject = GameManager.Pool.GetOriginal(name);
            
            if (gameObject != null) return gameObject as T;
        }

        return Resources.Load<T>(path);
    }


    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>(path);
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<PoolObject>() != null)
        {
            return GameManager.Pool.Pop(original, parent).gameObject;
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public void Destroy(GameObject gameObject)
    {
        if (gameObject == null)
            return;

        PoolObject poolable = gameObject.GetComponent<PoolObject>();
        if (poolable != null)
        {
            GameManager.Pool.Push(poolable);
            return;
        }

        Object.Destroy(gameObject);
    }
}
