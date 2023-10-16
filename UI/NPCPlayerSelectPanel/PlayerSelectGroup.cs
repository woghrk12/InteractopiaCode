using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectGroup : MonoBehaviour
{
    #region Variables

    [SerializeField] private Button button = null;
    [SerializeField] private UICharacter uiCharacter = null;

    private int ownerNumber = -1;

    #endregion Variables

    #region Properties

    public Button Button => button;

    #endregion Properties

    #region Methods

    public void InitGroup(int actorNumber)
    {
        ownerNumber = actorNumber;
        uiCharacter.SetCharacter(actorNumber);
        button.onClick.AddListener(() =>
        {
            (GameManager.InGame.NPCList[ENPCRole.INSPECTION] as ManagerBotNPC).Investigate(ownerNumber);
            GameManager.UI.ClosePopupPanel<NPCPlayerSelectPanel>();
        });
    }

    #endregion
}
