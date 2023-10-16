using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager
{
    #region Variables

    private UIPanel activePanel = null;
    private Stack<UIPanel> popupPanelStack = new();
    private int panelStackIndex = 1;

    private Transform rootTransform = null;
    private Image fadeImage = null;

    private Dictionary<string, UIPanel> panelDictionary = new();

    #endregion Variables

    #region Properties

    public Transform RootTransform
    {
        get
        {
            if (rootTransform == null)
            {
                GameObject root = GameObject.Find("@UIRoot");

                if (root == null)
                {
                    root = GameManager.Resource.Instantiate("UI/@UIRoot");
                    Object.DontDestroyOnLoad(root);
                }

                rootTransform = root.transform;
            }

            return rootTransform;
        }
    }

    #endregion Properties

    #region Methods

    public void Init()
    {
        // Set the root transform for instantiating the panel
        if (rootTransform == null)
        {
            GameObject root = GameObject.Find("@UIRoot");

            if (root == null)
            {
                root = GameManager.Resource.Instantiate("UI/@UIRoot");
                Object.DontDestroyOnLoad(root);
            }

            rootTransform = root.transform;
        }

        // Set the image for fade effect
        fadeImage = rootTransform.Find("FadeImage").GetComponent<Image>();
    }

    public T GetPanel<T>() where T : UIPanel
    {
        string name = typeof(T).Name;

        // Find the panel
        return panelDictionary.ContainsKey(name) ? panelDictionary[name] as T : null;
    }

    public void OpenPanel<T>(bool isPlayAnimation = true) where T : UIPanel
    {
        string name = typeof(T).Name;

        // Find the panel to be opened and to be closed
        if (!panelDictionary.TryGetValue(name, out UIPanel openPanel))
        {
            openPanel = InstantiatePanel<T>();
        }
        UIPanel closePanel = activePanel;

        // Update the panel variable
        activePanel = openPanel;
        activePanel.SortingOrder = 0;

        if (isPlayAnimation)
        {
            Sequence sequence = DOTween.Sequence();

            // Animation for closing active panel
            if (closePanel != null)
            {
                Sequence closeAnimation = closePanel.DeactiveAnimation();
                closeAnimation.OnComplete(() =>
                {
                    closePanel.OnDeactive?.Invoke();
                    closePanel.gameObject.SetActive(false);
                });
                sequence.Append(closeAnimation);
            }

            // Animation for popup panel
            while (popupPanelStack.Count > 0)
            {
                UIPanel popupPanel = popupPanelStack.Pop();
                Sequence closeAnimation = popupPanel.DeactiveAnimation();
                closeAnimation.OnComplete(() =>
                {
                    popupPanel.OnDeactive?.Invoke();
                    popupPanel.gameObject.SetActive(false);
                });
                sequence.Join(closeAnimation);
            }

            panelStackIndex = 1;

            // Animation for opening new panel 
            Sequence openAnimation = openPanel.ActiveAnimation();
            openAnimation.OnStart(() =>
            {
                openPanel.gameObject.SetActive(true);
                openPanel.OnActive?.Invoke();
            });
            sequence.Append(openAnimation);

            sequence.Play();
        }
        else // No panel animation
        {
            if (closePanel != null)
            {
                closePanel.OnDeactive?.Invoke();
                closePanel.gameObject.SetActive(false);
            }

            while (popupPanelStack.Count > 0)
            {
                UIPanel popupPanel = popupPanelStack.Pop();
                popupPanel.OnDeactive?.Invoke();
                popupPanel.gameObject.SetActive(false);
            }
            panelStackIndex = 1;

            openPanel.gameObject.SetActive(true);
            openPanel.OnActive?.Invoke();
        }
    }

    public void PopupPanel<T>(bool isPlayAnimation = true) where T : UIPanel
    {
        string name = typeof(T).Name;

        // Find the panel to be opened
        if (!panelDictionary.TryGetValue(name, out UIPanel openPanel))
        {
            openPanel = InstantiatePanel<T>();
        }

        // Update the panel variable
        openPanel.SortingOrder = panelStackIndex;
        popupPanelStack.Push(openPanel);
        panelStackIndex++;

        if (isPlayAnimation)
        {
            // Animation for opening new panel 
            openPanel.ActiveAnimation()
            .OnStart(() =>
            {
                openPanel.gameObject.SetActive(true);
                openPanel.OnActive?.Invoke();
            })
            .Play();
        }
        else // No panel animation
        {
            openPanel.gameObject.SetActive(true);
            openPanel.OnActive?.Invoke();
        }
    }

    public void ClosePanel<T>(bool isPlayAnimation = true)
    {
        string name = typeof(T).Name;

        // Find the panel to be closed
        if (!panelDictionary.TryGetValue(name, out UIPanel closePanel)) return;

        // Check if the panel is already turned off
        if (!closePanel.gameObject.activeSelf) return;

        // Update the panel variable
        activePanel = null;

        if (isPlayAnimation)
        {
            // Animation for closing the panel
            closePanel.DeactiveAnimation()
           .OnComplete(() =>
           {
               closePanel.OnDeactive?.Invoke();
               closePanel.gameObject.SetActive(false);
           })
           .Play();
        }
        else // No panel animation
        {
            closePanel.OnDeactive?.Invoke();
            closePanel.gameObject.SetActive(false);
        }
    }

    public void ClosePopupPanel<T>(bool isPlayAnimation = true)
    {
        string name = typeof(T).Name;

        // Find the panel to be closed
        if (!panelDictionary.TryGetValue(name, out UIPanel closePanel)) return;

        // Check if the panel is already turned off
        if (!closePanel.gameObject.activeSelf) return;

        // Check if the panel is top of the stack
        if (popupPanelStack.Peek() != closePanel) return;

        popupPanelStack.Pop();
        panelStackIndex--;

        if (isPlayAnimation)
        {
            // Animation for closing the panel
            closePanel.DeactiveAnimation()
           .OnComplete(() =>
           {
               closePanel.OnDeactive?.Invoke();
               closePanel.gameObject.SetActive(false);
           })
           .Play();
        }
        else // No panel animation
        {
            closePanel.OnDeactive?.Invoke();
            closePanel.gameObject.SetActive(false);
        }
    }

    public void CloseAllPopupPanel(bool isPlayAnimation = true)
    {
        Stack<UIPanel> closePopupPanelStack = popupPanelStack;

        if (isPlayAnimation)
        {
            Sequence sequence = DOTween.Sequence();

            // Animation for closing the active popup panel
            while (popupPanelStack.Count > 0)
            {
                UIPanel panel = popupPanelStack.Pop();
                Sequence panelAnimation = panel.DeactiveAnimation()
                    .OnComplete(() =>
                    {
                        panel.OnDeactive?.Invoke();
                        panel.gameObject.SetActive(false);
                    });
                sequence.Join(panelAnimation);
            }

            panelStackIndex = 1;

            sequence.Play();
        }
        else // No panel animation
        {
            while (popupPanelStack.Count > 0)
            {
                UIPanel panel = popupPanelStack.Pop();
                panel.OnDeactive?.Invoke();
                panel.gameObject.SetActive(false);
            }

            panelStackIndex = 1;
        }
    }

    public void CloseAllPanel(bool isPlayAnimation = true)
    {
        UIPanel closeActivePanel = activePanel;

        // Update the panel variable
        activePanel = null;
        popupPanelStack.Clear();
        panelStackIndex = 1;

        if (isPlayAnimation)
        {
            Sequence sequence = DOTween.Sequence();

            if (closeActivePanel != null && closeActivePanel.gameObject.activeSelf)
            {
                // Animation for closing the active panel
                Sequence closeActivePanelAnimation = closeActivePanel.DeactiveAnimation()
                    .OnComplete(() =>
                    {
                        closeActivePanel.OnDeactive?.Invoke();
                        closeActivePanel.gameObject.SetActive(false);
                    });
                sequence.Append(closeActivePanelAnimation);
            }

            while (popupPanelStack.Count > 0)
            {
                UIPanel panel = popupPanelStack.Pop();
                Sequence panelAnimation = panel.DeactiveAnimation()
                    .OnComplete(() =>
                    {
                        panel.OnDeactive?.Invoke();
                        panel.gameObject.SetActive(false);
                    });
                sequence.Join(panelAnimation);
            }

            panelStackIndex = 1;

            sequence.Play();
        }
        else // No panel animation
        {
            if (closeActivePanel != null)
            {
                closeActivePanel.OnDeactive?.Invoke();
                closeActivePanel.gameObject.SetActive(false);
            } 
            
            while (popupPanelStack.Count > 0)
            {
                UIPanel panel = popupPanelStack.Pop();
                panel.OnDeactive?.Invoke();
                panel.gameObject.SetActive(false);
            }

            panelStackIndex = 1;
        }
    }

    public void Alert(string message)
    {
        string name = typeof(AlertPanel).Name;
        (panelDictionary[name] as AlertPanel).SetAlertMessage(message);
        PopupPanel<AlertPanel>();
    }

    #endregion Methods

    #region Tween Methods

    public Tween FadeIn(float duration)
    {
        return fadeImage.DOColor(new Color(0f, 0f, 0f, 0f), duration)
            .OnStart(() =>
            {
                fadeImage.gameObject.SetActive(true);
                fadeImage.color = new Color(0f, 0f, 0f, 1f);
            })
            .OnComplete(() => fadeImage.gameObject.SetActive(false));
    }

    public Tween FadeOut(float duration)
    {
        return fadeImage.DOColor(new Color(0f, 0f, 0f, 1f), duration)
            .OnStart(() =>
            {
                fadeImage.gameObject.SetActive(true);
                fadeImage.color = new Color(0f, 0f, 0f, 0f);
            });
    }

    #endregion Tween Methods

    #region Helper Methods

    private T InstantiatePanel<T>() where T : UIPanel
    {
        string name = typeof(T).Name;

        T panel = GameManager.Resource.Instantiate($"UI/{name}").GetComponent<T>();

        panel.transform.SetParent(RootTransform);
        panel.InitPanel();
        panelDictionary.Add(name, panel);

        panel.gameObject.SetActive(false);

        return panel;
    }

    private void ClearPanelDictionary()
    {
        activePanel = null;
        popupPanelStack.Clear();

        foreach (KeyValuePair<string, UIPanel> panel in panelDictionary)
        {
            GameManager.Resource.Destroy(panel.Value.gameObject);
        }

        panelDictionary.Clear();
    }

    public void PremakeUIForTitle()
    {
        ClearPanelDictionary();

        InstantiatePanel<StartPanel>();
        InstantiatePanel<LobbyPanel>();
        InstantiatePanel<CreateRoomPanel>();
        InstantiatePanel<PublicJoinPanel>();
        InstantiatePanel<RoomPanel>();
        InstantiatePanel<PrivateJoinPanel>();
        InstantiatePanel<HostRuleSettingPanel>();
        InstantiatePanel<GuestRuleSettingPanel>();
        InstantiatePanel<CharacterSettingPanel>();
        InstantiatePanel<SettingPanel>();
        InstantiatePanel<TextChattingPanel>();
        InstantiatePanel<AuthPanel>();
        InstantiatePanel<AlertPanel>();
    }

    public void PremakeUIForInGame()
    {
        ClearPanelDictionary();

        InstantiatePanel<GameStartPanel>();
        InstantiatePanel<InGamePanel>();
        InstantiatePanel<MeetingOpeningPanel>();
        InstantiatePanel<MeetingPanel>();
        InstantiatePanel<MeetingResultPanel>();
        InstantiatePanel<EndingPanel>();
        InstantiatePanel<TextChattingPanel>();
        InstantiatePanel<SettingPanel>();
        InstantiatePanel<AlertPanel>();
        InstantiatePanel<MinimapPanel>();
        InstantiatePanel<NPCPlayerSelectPanel>();
        InstantiatePanel<NPCSelectPanel>();
        InstantiatePanel<AssasinSelectPanel>();
    }

    #endregion Helper Methods
}