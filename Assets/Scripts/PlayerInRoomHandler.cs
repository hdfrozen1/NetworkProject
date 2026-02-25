using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerInRoomHandler : MonoBehaviour
{
    public TMP_Text userNameText; // Hiển thị tên người chơi hoặc id
    public void UpdatePlayerInfo(int playerId) {
        if (playerId == 0) {
            userNameText.text = "Empty Slot";
            return;
        }
        userNameText.text = $"Player {playerId}";
    }

}
