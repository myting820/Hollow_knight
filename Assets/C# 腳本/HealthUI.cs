using UnityEngine;
using UnityEngine.UI; // 記得引用 UI 命名空間
using System.Collections.Generic; // 為了使用 List

public class HealthUI : MonoBehaviour
{
    [Header("UI 設定")]
    public GameObject heartPrefab;  // 心心的樣本 (Prefab)
    public Transform container;     // 容器 (HealthBar)

    // 為了簡單，我們用顏色區分：白色=有血，黑色半透明=扣血
    public Color fullColor = Color.white;
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    // 儲存生成出來的心心圖片列表
    private List<Image> hearts = new List<Image>();

    // 1. 初始化：根據最大血量生成圖示
    public void InitHealth(int maxHealth)
    {
        // 先清空舊的 (防止重複生成)
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        hearts.Clear();

        // 生成對應數量的心心
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, container);
            hearts.Add(newHeart.GetComponent<Image>());
        }
    }

    // 2. 更新顯示：根據當前血量改變顏色
    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                // 如果索引小於當前血量，顯示滿血顏色
                hearts[i].color = fullColor;
            }
            else
            {
                // 否則顯示空血顏色
                hearts[i].color = emptyColor;
            }
        }
    }
}
