using UnityEngine;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;


// 1. Lớp vỏ bọc để xem Action là gì
[Serializable]
public class ServerResponse {
    public string action;
}

// 2. Dữ liệu cho ROOM_LIST
[Serializable]
public class RoomInfo {
    public int room_id;
    public int player_count;
}
[Serializable]
public class PlayerInRoom
{
    public List<string> players_team; // Chứa "a", "b"...
    public List<int> all_players;     // Chứa 101, 102...
    public int room_id;
}

[Serializable]
public class RoomListResponse : ServerResponse {
    public List<RoomInfo> rooms;
}

// 3. Dữ liệu cho VOTE_RESULTS
[Serializable]
public class VoteResultsResponse : ServerResponse {
    public int team_a_skill;
    public int team_b_skill;
}

// 4. Dữ liệu cho ROOM_CREATED
[Serializable]
public class RoomCreatedResponse : ServerResponse {
    public int room_id;
    public int player_id;
}
[Serializable]
public class GameMessage
{
    public string action;
    // public string value;
    public int room_id; // Chỉ dùng khi gửi lệnh JOIN và khi chơi
    public int player_id; // Server trả về ID người chơi, dùng để gửi lệnh tạo phòng, join phòng, và chơi
    public int move; // Dùng để gửi kỹ năng đã chọn khi vote
    
}
public class ClientHandle : MonoBehaviour
{
    public static ClientHandle Instance { get; private set; }
    public TMP_InputField roomInputField;
    private WebSocket websocket;
    private string serverUrl = "wss://noninherent-unacerbically-laree.ngrok-free.dev/ws";
    public CanvasGroup RoomsPanel; // Gán trong Inspector
    public CanvasGroup Room; // Gán trong Inspector
    public RoomsHandler roomsHandler; // Gán trong Inspector để cập nhật UI danh sách phòng khi nhận được ROOM_LIST từ server
    public TeamHandler teamHandler; // Gán trong Inspector để cập nhật UI đội khi có người chơi mới vào phòng
    public SelectHeroPanelHandler selectHeroPanelHandler; // Gán trong Inspector để hiển thị bảng chọn tướng khi bắt đầu game
    [UnitHeaderInspectable("Client Information")]
    public int playerId; // Lưu ID người chơi sau khi kết nối
    public int currentRoomId; // Lưu ID phòng hiện tại (nếu có)
    private bool IsHost = false; // Biến để xác định nếu người chơi này là chủ phòng, mặc định là false, server sẽ trả về true nếu người chơi này tạo phòng thành công
    private bool isInChooseMode = false;
    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại đối tượng này khi chuyển scene
        }
        else
        {
            Destroy(gameObject); // Đảm bảo chỉ có một instance tồn tại
        }
    }
    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        // Sự kiện khi kết nối thành công
        websocket.OnOpen += () =>
        {
            Debug.Log("Đã kết nối tới Server Go!");
            // Ví dụ: Tự động gửi yêu cầu tạo phòng khi vừa vào (nếu muốn)
            // CreateRoom(); 
        };

        // Sự kiện khi có lỗi
        websocket.OnError += (e) =>
        {
            Debug.LogError("Lỗi WebSocket: " + e);
        };

        // Sự kiện khi đóng kết nối
        websocket.OnClose += (e) =>
        {
            Debug.Log("Đã ngắt kết nối Server.");
        };

        // --- TRÁI TIM CỦA VIỆC NHẬN TIN NHẮN ---
        websocket.OnMessage += (bytes) =>
        {
            // 1. Chuyển byte sang string
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Nhận được: " + message);

            // 2. Phân loại và xử lý
            HandleIncomingMessage(message);
        };

        // Bắt đầu kết nối
        await websocket.Connect();
    }

    void Update()
    {
        // Rất quan trọng: Đẩy tin nhắn từ queue vào Main Thread của Unity
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif
    }

    /// <summary>
    /// Hàm gửi yêu cầu tạo phòng mới lên server. Server sẽ trả về ID phòng và ID người chơi sau khi tạo thành công.
    /// </summary>
    public async void CreateRoom() {
            var msg = new GameMessage { action = "CREATE" };
            await websocket.SendText(JsonUtility.ToJson(msg));
    }

    /// <summary>
    /// Hàm gửi yêu cầu tham gia phòng lên server. Server sẽ trả về ID phòng và ID người chơi sau khi tham gia thành công.
    /// </summary>
    public async void RequestJoinRoom(string roomId  = "") {
        if (int.TryParse(roomId == "" ? roomInputField.text : roomId, out int id)) {
            var msg = new GameMessage { action = "JOIN", room_id = id};
            await websocket.SendText(JsonUtility.ToJson(msg));
        }
    }
    public async void LeaveRoom() {
        var msg = new GameMessage { action = "LEAVE", room_id = this.currentRoomId };
            await websocket.SendText(JsonUtility.ToJson(msg));
    }
    public async void Refresh() {
            var msg = new GameMessage { action = "REFRESH" };
            await websocket.SendText(JsonUtility.ToJson(msg));
    }

    private void HandleIncomingMessage(string jsonString)
    {
        try
        {
            // Bước 1: Đọc thử action để biết đây là loại tin nhắn gì
            ServerResponse baseMsg = JsonUtility.FromJson<ServerResponse>(jsonString);

            // Bước 2: Dựa vào action để bóc tách dữ liệu chi tiết
            switch (baseMsg.action)
            {
                case "PLAYER_JOINED":
                    Debug.Log("Một người chơi mới đã tham gia phòng!");
                    var playerData = JsonUtility.FromJson<PlayerInRoom>(jsonString);
                    // Có thể gửi yêu cầu REFRESH lại danh sách phòng hoặc cập nhật UI phòng hiện tại
                    RefreshPlayerInRoom(playerData.players_team,playerData.all_players,playerData.room_id);
                    break;
                case "CONNECTED":
                    var connectData = JsonUtility.FromJson<RoomCreatedResponse>(jsonString);
                    this.playerId = connectData.player_id; // Lưu ID người chơi
                    this.currentRoomId = connectData.room_id; // Lưu ID phòng (nếu có)
                    Debug.Log($"Kết nối thành công! ID của bạn: {this.playerId}");
                    break;
                case "ROOM_LIST":
                    var roomListData = JsonUtility.FromJson<RoomListResponse>(jsonString);
                    OnRoomListReceived(roomListData.rooms);
                    break;

                case "ROOM_CREATED":
                    var createdData = JsonUtility.FromJson<RoomCreatedResponse>(jsonString);
                    OnRoomCreated(createdData.room_id, createdData.player_id);
                    break;

                case "VOTE_RESULTS":
                    var voteData = JsonUtility.FromJson<VoteResultsResponse>(jsonString);
                    OnVoteResultsReceived(voteData.team_a_skill, voteData.team_b_skill);
                    break;
                case "BACK_TO_LOBBY":
                    var lobbyData = JsonUtility.FromJson<RoomCreatedResponse>(jsonString);
                    this.currentRoomId = 0; // Reset room ID khi quay về lobby
                    this.IsHost = false;
                    teamHandler.SetStartButtonActive(IsHost);
                    teamHandler.CLoseRoomPanel(); // Chuyển về màn hình lobby
                    break;
                case "GAME_STARTED":
                    Debug.Log("Trò chơi đã bắt đầu! Chuẩn bị chuyển scene...");
                    OnGameStarted();
                    break;
                default:
                    Debug.LogWarning("Action chưa được hỗ trợ: " + baseMsg.action);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi khi giải mã JSON: " + ex.Message);
        }
    }

    private void RefreshPlayerInRoom(List<string> playerTeamInfo, List<int> players_id, int roomId)
    {
        this.currentRoomId = roomId; // Cập nhật ID phòng hiện tại khi có người chơi mới vào, đảm bảo luôn có thông tin phòng mới nhất
        teamHandler.UpdateTeams(playerTeamInfo, players_id);
        GoToRoom(); // Hiển thị UI phòng khi có người chơi mới vào, có thể điều chỉnh lại logic này nếu không muốn tự động chuyển phòng khi có người chơi mới vào
        roomsHandler.CloseRoomsPanel(); // Đóng bảng danh sách phòng khi đã vào phòng, có thể điều chỉnh lại logic này nếu muốn giữ bảng danh sách phòng mở để vào tiếp các phòng khác mà không cần quay lại lobby
    }

    // --- CÁC HÀM ĐỂ BẠN TỰ VIẾT LOGIC XỬ LÝ ---

    private void OnRoomListReceived(List<RoomInfo> rooms)
    {
        Debug.Log($"Có {rooms.Count} phòng khả dụng.");
        foreach (var room in rooms)
        {
            Debug.Log($"Phòng ID: {room.room_id} - Số người: {room.player_count}/10");
        }
        // TODO: Cập nhật UI ScrollView hoặc List phòng của bạn ở đây
        roomsHandler.UpdateRoomInfo(rooms);
        OpenRoomsPanel();
    }
    private void OnGameStarted()
    {
        isInChooseMode = true;
        teamHandler.CLoseRoomPanel();
        selectHeroPanelHandler.OpenPanel();
    }

    private void OnRoomCreated(int roomId, int playerId)
    {
        Debug.Log($"Đã tạo/vào phòng thành công! ID Phòng: {roomId}, ID Của bạn: {playerId}");
        this.IsHost = true;
        this.currentRoomId = roomId; // Lưu ID phòng hiện tại
        teamHandler.SetStartButtonActive(IsHost); // Chỉ hiển thị nút START nếu người chơi này là chủ phòng
        // TODO: Chuyển Scene hoặc ẩn bảng Lobby, hiện bảng chọn tướng
        GoToRoom();
        teamHandler.HostUpdate(); // Cập nhật UI đội cho chủ phòng, chủ phòng luôn ở team A và là người chơi đầu tiên nên sẽ được cập nhật trước khi có phản hồi PLAYER_JOINED từ server
    }

    private void OnVoteResultsReceived(int skillA, int skillB)
    {
        selectHeroPanelHandler.UpdateHeroPreviews(skillA, skillB);
    }

    // --- CÁC HÀM GỬI DỮ LIỆU LÊN SERVER ---

    /// <summary>
    /// Hàm gửi kỹ năng đã chọn khi vote
    /// </summary>
    /// <param name="skill"> Kỹ năng đã chọn (1-5)</param>
    public async void SendMove(int skill)
    {
        if (websocket.State == WebSocketState.Open)
        {
            var msg = new GameMessage{ action = "MOVE", player_id = this.playerId, room_id = this.currentRoomId, move = skill };
            string json = JsonUtility.ToJson(msg);
            await websocket.SendText(json);
        }
    }
    public async void StartGame() {
        isInChooseMode = true;
        selectHeroPanelHandler.OpenPanel();
        var msg = new GameMessage { action = "START", room_id = this.currentRoomId ,player_id = this.playerId}; 
        await websocket.SendText(JsonUtility.ToJson(msg));
    }
    private async void OnApplicationQuit()
    {
        if (websocket != null) await websocket.Close();
    }
    public void OpenRoomsPanel()
    {
        RoomsPanel.alpha = 1f; // Hiển thị panel
        RoomsPanel.interactable = true; // Cho phép tương tác
        RoomsPanel.blocksRaycasts = true; // Cho phép nhận sự kiện chuột
    }
    public void GoToRoom()
    {
        Room.alpha = 1f; // Hiển thị panel
        Room.interactable = true; // Cho phép tương tác
        Room.blocksRaycasts = true; // Cho phép nhận sự kiện chuột
    }
}
