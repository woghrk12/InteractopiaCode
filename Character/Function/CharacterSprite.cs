using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSprite : MonoBehaviour
{
    #region Variables

    [SerializeField] private SpriteRenderer spriteRenderer = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        Material material = Instantiate(spriteRenderer.material);
        spriteRenderer.material = material;
    }

    #endregion Unity Events

    #region Methods

    public void SetFloat(string param, float value)
    {
        spriteRenderer.material.SetFloat(param, value);
    }

    public void SetColor(string param, Color value)
    {
        spriteRenderer.material.SetColor(param, value);
    }

    #endregion Methods
}
