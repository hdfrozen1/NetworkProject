using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPointA;
    public Transform spawnPointB;

    // 2 Field ID nhân vật (lấy từ ClientHandle)
    private int heroIdA;
    private int heroIdB;

    // Danh sách Prefab load từ Resources
    private GameObject[] heroPrefabs;
    public List<Button> ButtonsSkill;
    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameScene"));
        ClientHandle.Instance.battleManager = this; // Đảm bảo ClientHandle có reference đến BattleManager
        //ClientHandle.Instance.isInChooseMode = false; // Đặt lại trạng thái chọn tướng khi vào trận đấu
        // 2. Lấy ID trực tiếp từ ClientHandle (Vì đã DontDestroyOnLoad)

        GetHeroIdsFromClient();
        // 1. Load toàn bộ Prefab từ thư mục Resources/HeroPrefabs
        LoadHeroPrefabs();

        // 3. Tiến hành tạo nhân vật
        SpawnHeroes();
        for(int i = 0; i < ButtonsSkill.Count; i++)
        {
            int index = i; // Lưu index vào biến cục bộ để tránh lỗi closure trong delegate
            ButtonsSkill[i].onClick.AddListener(() => OnSkillButtonClicked(index));
        }
    }

    private void OnSkillButtonClicked(int index)
    {
        for (int i = 0; i < ButtonsSkill.Count; i++)
        {
            ButtonsSkill[i].interactable = false; // Vô hiệu hóa tất cả nút sau khi chọn
        }
        Debug.Log("Đã chọn kỹ năng với ID: " + index);
        ClientHandle.Instance.SendMove(index); // Gửi ID kỹ năng đã chọn lên server
    }

    void LoadHeroPrefabs()
    {
        // Load tất cả GameObject trong Resources/HeroPrefabs
        // Lưu ý: Tên file hoặc thứ tự load có thể thay đổi, 
        // tốt nhất nên đặt tên file là "0", "1", "2"... để dễ quản lý index.
        heroPrefabs = Resources.LoadAll<GameObject>("HeroPrefabs");
        
        // Sắp xếp lại mảng theo tên để đảm bảo ID 0 luôn là phần tử 0 (nếu bạn đặt tên file là số)
        heroPrefabs = heroPrefabs.OrderBy(p => p.name).ToArray();

        Debug.Log($"Đã load thành công {heroPrefabs.Length} heroes từ Resources.");
    }

    void GetHeroIdsFromClient()
    {
        // Giả sử ClientHandle của bạn dùng Singleton (Instance) 
        // và có biến lưu trữ ID là TeamAHeroID, TeamBHeroID
        if (ClientHandle.Instance != null)
        {
            heroIdA = ClientHandle.Instance.teamAHeroID;
            heroIdB = ClientHandle.Instance.teamBHeroID;
            Debug.Log($"Lấy ID từ ClientHandle: TeamA={heroIdA}, TeamB={heroIdB}");
        }
        else
        {
            Debug.LogError("Không tìm thấy ClientHandle! Hãy chắc chắn nó đã được DontDestroyOnLoad.");
        }
    }

    void SpawnHeroes()
    {
        // Kiểm tra lỗi Index trước khi Spawn
        if (heroIdA < heroPrefabs.Length && heroIdB < heroPrefabs.Length)
        {
            // Tạo Team A
            GameObject playerA = Instantiate(heroPrefabs[heroIdA], spawnPointA.position, spawnPointA.rotation);
            playerA.name = "Hero_TeamA";

            // Tạo Team B (Có thể xoay mặt lại để đối diện Team A)
            GameObject playerB = Instantiate(heroPrefabs[heroIdB], spawnPointB.position, spawnPointB.rotation);
            playerB.name = "Hero_TeamB";
            playerB.transform.Rotate(0, 180, 0); // Quay mặt 180 độ
        }
        else
        {
            Debug.LogError("ID nhân vật vượt quá số lượng Prefab hiện có!");
        }
    }
    public void Fight(int teamASkillId, int teamBSkillId)
    {
        // Ở đây bạn có thể thêm logic để xử lý kỹ năng của hai đội, ví dụ:
        Debug.Log($"Team A sử dụng kỹ năng ID: {teamASkillId}");
        Debug.Log($"Team B sử dụng kỹ năng ID: {teamBSkillId}");

        // Bạn có thể gọi các phương thức khác để áp dụng hiệu ứng kỹ năng, tính toán sát thương, v.v.
        StartCoroutine(TurnOnButtonSkill(1.5f)); // Bật lại nút kỹ năng sau 1.5 giây
    }
    private IEnumerator TurnOnButtonSkill( float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < ButtonsSkill.Count; i++)
        {
            ButtonsSkill[i].interactable = true; // Bật lại tất cả nút sau khi delay
        }
    }
}