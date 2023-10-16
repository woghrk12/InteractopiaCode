using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private Queue<PoolObject> poolQueue = new();

    #region Properties

    public GameObject Original { private set; get; }
    public Transform RootTransform { private set; get; }

    #endregion Properties

    public void Init(GameObject original, int count = 1)
    {
        Original = original;
        RootTransform = new GameObject().transform;
        RootTransform.name = $"@Root_{original.name}";
    }

    private PoolObject Create()
    {
        GameObject gameObject = Object.Instantiate<GameObject>(Original);
        gameObject.name = Original.name;

        return gameObject.GetOrAddComponent<PoolObject>();
    }

    public void Push(PoolObject poolObject)
    {
        if (poolObject == null) return;

        poolObject.transform.parent = RootTransform;
        poolObject.gameObject.SetActive(false);
        poolObject.IsUsing = false;

        poolQueue.Enqueue(poolObject);
    }

    public PoolObject Pop(Transform parent)
    {
        if (parent == null) return null;

        PoolObject poolObject = poolQueue.Count > 0 ? poolQueue.Dequeue() : Create();
        
        poolObject.gameObject.SetActive(true);
        poolObject.transform.parent = parent;
        poolObject.IsUsing = true;

        return poolObject;
    }
}

public class PoolManager
{
    #region Variables

    private Dictionary<string, Pool> poolDictionary = new();
    private Transform rootTransform = null;

    #endregion Variables

    #region Methods

    public void Init()
    {
        if (rootTransform == null)
        {
            rootTransform = new GameObject("@PoolRoot").transform;
            Object.DontDestroyOnLoad(rootTransform);
        }
    }

    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new();
        pool.Init(original, count);
        pool.RootTransform.parent = rootTransform;

        poolDictionary.Add(original.name, pool);
    }

    public void Push(PoolObject poolable)
    {
        string name = poolable.gameObject.name;

        if (!poolDictionary.ContainsKey(name))
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        poolDictionary[name].Push(poolable);
    }

    public PoolObject Pop(GameObject original, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(original.name))
        {
            CreatePool(original);
        }
        
        return poolDictionary[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        if (!poolDictionary.ContainsKey(name)) return null;
        
        return poolDictionary[name].Original;
    }

    public void Clear()
    {
        foreach (Transform child in rootTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        poolDictionary.Clear();
    }

    #endregion Methods
}
