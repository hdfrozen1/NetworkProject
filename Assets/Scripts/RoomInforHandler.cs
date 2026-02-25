using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInforHandler : MonoBehaviour
{
    public TMP_Text currentUserText;
    public TMP_Text roomInfoText;
    public Button JoinButton;
    private void Awake()
    {
        var texts = GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length >= 3)
        {
            roomInfoText = texts[1];
            currentUserText = texts[2];
        }
        JoinButton = GetComponentInChildren<Button>(true);
        gameObject.SetActive(false);
    }
    void Start()
    {
        JoinButton.onClick.AddListener(() => ClientHandle.Instance.RequestJoinRoom(roomInfoText.text));
    }
    public void UpdateInfo(RoomInfo info)
    {
        roomInfoText.text = "" + info.room_id;
        currentUserText.text = $"Players: {info.player_count} / 10";
    }
}
