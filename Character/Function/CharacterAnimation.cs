using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    #region Variables

    [SerializeField] private Animator animator = null;

    #endregion Variables

    #region Unity Events

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion Unity Events

    #region Methods

    public void SetTrigger(string key) => animator.SetTrigger(key);

    public void SetBool(string key, bool value) => animator.SetBool(key, value);

    #endregion Methods
}
