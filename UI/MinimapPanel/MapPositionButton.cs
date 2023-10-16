using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPositionButton : MonoBehaviour
{
    #region Variables

    private Button button = null;
    [SerializeField] private EMapPosition mapPosition = EMapPosition.NONE;

    [HideInInspector] public EMinimapOpenCause OpenCause = EMinimapOpenCause.NONE;

    #endregion Variables

    #region Properties

    public Button Button => button;

    public EMapPosition MapPosition => mapPosition;

    #endregion Properties

    #region Unity Events

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClickMapPositionButton);
    }

    #endregion Unity Events

    #region Event Methods

    public void OnClickMapPositionButton()
    {
        // TODO : Check why the player open the minimap panel
        switch (OpenCause)
        {
            case EMinimapOpenCause.TELOPORT:
                (GameManager.InGame.NPCList[ENPCRole.TRANSPORT] as ShiftBotNPC).Teleport(mapPosition);
                break;

            case EMinimapOpenCause.WATCH:
                break;
        }

        GameManager.UI.ClosePopupPanel<MinimapPanel>();
    }

    #endregion Event Methods
}
