using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class SelectHeroPanelHandler : MonoBehaviour
{
    public Button ChooseHeroButton;
    public List<Toggle> heroToggles; 
    private int selectedHeroIndex = -1; 
    public Image HeroPreviewImageTeamA;
    public Image HeroPreviewImageTeamB;
    public List<Sprite> heroSprites; // Danh sách sprite tương ứng với các hero, gán trong Inspector
    private void Awake() 
    {
        ChooseHeroButton.interactable = false;
        // Thêm sự kiện Click cho nút chọn
        ChooseHeroButton.onClick.AddListener(OnChooseHeroClick);
        ClosePanel();
        HeroPreviewImageTeamA.gameObject.SetActive(false);
        HeroPreviewImageTeamB.gameObject.SetActive(false);
        Sprite[] sprites = Resources.LoadAll<Sprite>("HeroImages");
        heroSprites = sprites.ToList();
    }

    void Start()
    {
        // Lấy danh sách Toggle nếu chưa kéo thả trong Inspector
        if (heroToggles == null || heroToggles.Count == 0)
        {
            heroToggles = new List<Toggle>(GetComponentsInChildren<Toggle>());
        }

        for (int i = 0; i < heroToggles.Count; i++)
        {
            int index = i; // Lưu index vào biến cục bộ để tránh lỗi closure trong delegate
            
            // 1. Đặt trạng thái ban đầu là false
            heroToggles[i].isOn = false;
            heroToggles[i].interactable = true;
            heroToggles[i].SetIsOnWithoutNotify(false); // Đảm bảo không kích hoạt sự kiện khi thiết lập trạng thái ban đầu
            heroToggles[i].GetComponentInChildren<Image>().sprite = heroSprites[i]; // Gán sprite tương ứng cho mỗi Toggle

            // 2. Đăng ký sự kiện khi bấm vào Toggle
            heroToggles[i].onValueChanged.AddListener((bool isOn) => {
                if (isOn) 
                {
                    OnToggleSelected(index);
                }
            });
        }
    }

    void OnToggleSelected(int index)
    {
        selectedHeroIndex = index;
        ChooseHeroButton.interactable = true; // Kích hoạt nút bấm chọn

        for (int i = 0; i < heroToggles.Count; i++)
        {
            // 3. Nếu là Toggle đang được chọn thì không cho bấm lại (interactable = false)
            // Các Toggle khác thì cho phép bấm (interactable = true)
            heroToggles[i].interactable = (i != index);

            // Đảm bảo các Toggle khác chuyển về false (nếu không dùng ToggleGroup)
            if (i != index)
            {
                heroToggles[i].SetIsOnWithoutNotify(false);
            }
        }
        
        Debug.Log("Đã chọn Hero index: " + selectedHeroIndex);
    }

    void OnChooseHeroClick()
    {
        if (selectedHeroIndex != -1)
        {
            ChooseHeroButton.interactable = false; // Vô hiệu hóa nút sau khi chọn để tránh chọn lại
            Debug.Log("Đã xác nhận chọn Hero index: " + selectedHeroIndex);
            ClientHandle.Instance.SendMove(selectedHeroIndex);
            
        }
    }
    public void OpenPanel()
    {
        GetComponent<CanvasGroup>().alpha = 1; // Hiển thị panel
        GetComponent<CanvasGroup>().interactable = true;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    public void ClosePanel()
    {
        GetComponent<CanvasGroup>().alpha = 0; // Ẩn panel
        GetComponent<CanvasGroup>().interactable = false;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void UpdateHeroPreviews(int teamASkill, int teamBSkill)
    {
        // Cập nhật hình ảnh preview dựa trên skill đã chọn của mỗi đội
        HeroPreviewImageTeamA.gameObject.SetActive(true);
        HeroPreviewImageTeamB.gameObject.SetActive(true);
        HeroPreviewImageTeamA.sprite = heroSprites[teamASkill]; // Giả sử teamASkill là index của hero đã chọn
        HeroPreviewImageTeamB.sprite = heroSprites[teamBSkill];
    }
}
