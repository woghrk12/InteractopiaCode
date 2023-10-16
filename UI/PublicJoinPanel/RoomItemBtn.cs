using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;


public class RoomItemBtn : MonoBehaviour
{
    #region Variables

    private RoomInfo roomInfo = null;

    [SerializeField] private Button selectBtn = null;
    [SerializeField] private Image mapIcon = null;
    [SerializeField] private Text roomNameText = null;
    [SerializeField] private Text roomModeText = null;
    [SerializeField] private Text playerNumText = null;

    #endregion Variables

    #region Properties

    public RoomInfo RoomInfo => roomInfo;

    public Button SelectBtn => selectBtn;

    #endregion Properties

    #region Methods

    public void SetRoomItem(RoomInfo roomInfo)
    {
        this.roomInfo = roomInfo;
        roomNameText.text = this.roomInfo.CustomProperties["RoomName"].ToString();
        playerNumText.text = $"{this.roomInfo.PlayerCount.ToString()} / {this.roomInfo.MaxPlayers.ToString()}";
    }

    #endregion Methods
}
