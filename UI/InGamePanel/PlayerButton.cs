using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour
{
    #region Variables

    [Header("Components for UI")]
    [SerializeField] private Button button = null;
    [SerializeField] private Image buttonIcon = null;
    [SerializeField] private Text cooldownText = null;

    #endregion Variables

    #region Properties

    public Button Button => button;

    public bool IsInteractable 
    {
        set { button.interactable = value; }
        get { return button.interactable; }
    }

    #endregion Properties

    #region Methods

    public void InitButton(Sprite buttonIcon, Color color)
    {
        this.buttonIcon.sprite = buttonIcon;

        button.GetComponent<Image>().color = color;
        this.buttonIcon.color = new Color(color.r, color.g, color.b, 0.5f);
        
        if(cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    public void SetActiveText(bool isActive)
    {
        cooldownText.gameObject.SetActive(isActive);
    }

    public void SetCooldownText(float cooldown)
    {
        cooldownText.text = cooldown.ToString("F0");
    }

    #endregion Methods
}
