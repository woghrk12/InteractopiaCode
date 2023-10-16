using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectButton : MonoBehaviour
{
    #region Variables

    [SerializeField] private Button button = null;
    [SerializeField] private Image buttonImage = null;
    [SerializeField] private Image characterImage = null;
    [SerializeField] private Text nicknameText = null;
    [SerializeField] private UICharacter uiCharacter = null;
    [SerializeField] private GameObject highlightedImageObject = null;

    private int actorNumber = -1;

    #endregion Variables

    #region Properties

    public int ActorNumber => actorNumber;

    #endregion Properties

    #region Methods

    public void InitButton(AssasinSelectPanel panel, int actorNumber)
    {
        button.onClick.AddListener(() => panel.SelectPlayer(this));
        this.actorNumber = actorNumber;
        uiCharacter.SetCharacter(this.actorNumber);
    }

    public void SetActiveButton(bool isActive)
    {
        highlightedImageObject.SetActive(isActive);
    }

    public void SetDisableButton()
    {
        Color disableColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

        buttonImage.color = disableColor;
        characterImage.color = disableColor;
        nicknameText.color = disableColor;
    }

    #endregion Methods
}
