using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("要喚醒的 Boss")]
    public GameObject bossObject;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果撞到的是玩家，且還沒觸發過
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // 鎖定，避免重複觸發

            if (bossObject != null)
            {
                // 1. 確保 Boss 物件是開啟的
                bossObject.SetActive(true);

                // 2. 呼叫 Boss 身上的 Enemy 腳本來顯示血條
                Enemy bossStats = bossObject.GetComponent<Enemy>();
                if (bossStats != null)
                {
                    bossStats.ActivateBossUI();
                }
            }
        }
    }

    // ★ 新增的重置函式，必須放在 OnTriggerEnter2D 的外面 (但在 class 裡面)
    public void ResetTrigger()
    {
        hasTriggered = false; 
    }
}