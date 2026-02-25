using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomsHandler : MonoBehaviour
{
    public List<RoomInforHandler> roomInforHandlers = new List<RoomInforHandler>();
    
    void Start()
    {
        roomInforHandlers.AddRange(GetComponentsInChildren<RoomInforHandler>(true));
        Button closeButton = GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(CloseRoomsPanel);
        CloseRoomsPanel();
    }

    public void UpdateRoomInfo(List<RoomInfo> rooms) {
        for(int i = 0; i < rooms.Count; i++)
        {
            roomInforHandlers[i].gameObject.SetActive(true);
            roomInforHandlers[i].UpdateInfo(rooms[i]);
        }
    }
    public void CloseRoomsPanel()
    {
        this.GetComponent<CanvasGroup>().alpha = 0;
        this.GetComponent<CanvasGroup>().interactable = false;
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
