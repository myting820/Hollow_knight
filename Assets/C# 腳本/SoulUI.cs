using UnityEngine;
using UnityEngine.UI; // 記得引用 UI

public class SoulUI : MonoBehaviour
{
    [Header("UI 組件")]
    public Image liquidImage; // 那個會升降的白色圓形

    // 更新靈魂顯示
    // 參數：當前靈魂, 最大靈魂
    public void UpdateSoulUI(int currentMana, int maxMana)
    {
        if (liquidImage != null)
        {
            // 計算百分比 (0.0 ~ 1.0)
            // 注意：要強制轉成 float，不然整數相除會變成 0
            float fillAmount = (float)currentMana / maxMana;

            // 設定給 UI
            liquidImage.fillAmount = fillAmount;
        }
    }
}