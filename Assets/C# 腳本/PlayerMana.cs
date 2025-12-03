using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("靈魂設定")]
    public int maxMana = 99;
    public int currentMana = 0;

    // 【新增】UI 參考
    public SoulUI soulUI;

    void Start()
    {
        // 遊戲開始時，先更新一次 UI (確保顯示正確)
        if (soulUI != null) soulUI.UpdateSoulUI(currentMana, maxMana);
    }

    public void GainMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;

        Debug.Log($"獲得靈魂: {currentMana}");

        // 【新增】更新 UI
        if (soulUI != null) soulUI.UpdateSoulUI(currentMana, maxMana);
    }

    public bool TrySpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            Debug.Log($"消耗靈魂: {currentMana}");

            // 【新增】更新 UI
            if (soulUI != null) soulUI.UpdateSoulUI(currentMana, maxMana);

            return true;
        }
        else
        {
            // (進階) 這裡可以讓 UI 閃爍紅光警告
            Debug.Log("魔力不足！");
            return false;
        }
    }
}