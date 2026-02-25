using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamHandler : MonoBehaviour
{
    public Transform playerTeamA; // Nơi hiển thị thông tin người chơi trong phòng
    public Transform playerTeamB; // Nơi hiển thị thông tin người chơi trong phòng
    List<PlayerInRoomHandler> teamAPlayers = new List<PlayerInRoomHandler>();
    List<PlayerInRoomHandler> teamBPlayers = new List<PlayerInRoomHandler>();
    public Button StartButton;
    public Button LeaveButton;
    private void Start()
    {
        CLoseRoomPanel();
        LeaveButton.onClick.AddListener(() => ClientHandle.Instance.LeaveRoom());
        StartButton.gameObject.SetActive(false); // Ẩn nút START khi khởi tạo, chỉ hiển thị cho chủ phòng
        StartButton.onClick.AddListener(() => ClientHandle.Instance.StartGame());
        teamAPlayers.AddRange(playerTeamA.GetComponentsInChildren<PlayerInRoomHandler>());
        teamBPlayers.AddRange(playerTeamB.GetComponentsInChildren<PlayerInRoomHandler>());
    }
    public void UpdateTeams(List<string> playerTeamInfo, List<int> players_id)
    {
        // Dựa vào playerTeamInfo và players_id để cập nhật UI cho team A và team B
        int indexA = 0;
        int indexB = 0;
        for (int i = 0; i < playerTeamInfo.Count; i++)
        {
            if (playerTeamInfo[i] == "A")
            {
                teamAPlayers[indexA].UpdatePlayerInfo(players_id[i]);
                indexA++;
            }
            else if (playerTeamInfo[i] == "B")
            {
                teamBPlayers[indexB].UpdatePlayerInfo(players_id[i]);
                indexB++;
            }
        }
        for (int i = indexA; i < teamAPlayers.Count; i++)
        {
            teamAPlayers[i].UpdatePlayerInfo(0); // Cập nhật thông tin trống cho các slot còn lại
        }
        for (int i = indexB; i < teamBPlayers.Count; i++)
        {
            teamBPlayers[i].UpdatePlayerInfo(0); // Cập nhật thông tin trống cho các slot còn lại
        }
    }
    public void SetStartButtonActive(bool isActive)
    {
        StartButton.gameObject.SetActive(isActive);
    }
    public void HostUpdate()
    {
        teamAPlayers[0].UpdatePlayerInfo(ClientHandle.Instance.playerId); // Chủ phòng luôn ở team A và là người chơi đầu tiên
    }
    public void CLoseRoomPanel()
    {
        this.GetComponent<CanvasGroup>().alpha = 0;
        this.GetComponent<CanvasGroup>().interactable = false;
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
