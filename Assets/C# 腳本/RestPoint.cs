using UnityEngine;

public class RestPoint : MonoBehaviour
{
    [Header("設定")]
    public KeyCode interactKey = KeyCode.W; // 按 W 坐下

    // 內部狀態
    private bool isPlayerInRange; // 玩家是否在椅子旁
    private bool isSitting;       // 玩家是否正在坐著

    // 記錄玩家身上的組件 (為了鎖住行動)
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerHealth playerHealth;
    private Rigidbody2D playerRb;
    private Animator playerAnim;

    void Update()
    {
        // 情況 A：站著 + 在範圍內 + 按下 W -> 坐下
        if (isPlayerInRange && !isSitting && Input.GetKeyDown(interactKey))
        {
            SitDown();
        }
        // 情況 B：坐著 + (按空白鍵 或 移動鍵) -> 起立
        else if (isSitting && (Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("Horizontal") != 0))
        {
            StandUp();
        }
    }

    void SitDown()
    {
        isSitting = true;
        Debug.Log("休息中... (血量補滿)");

        // 1. 執行補血
        if (playerHealth != null) playerHealth.HealFull();

        // 【新增】更新重生點
        // 我們把重生點設在椅子稍微上方一點的位置 (transform.position)
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateCheckpoint(transform.position);
        }

        // 2. 鎖住玩家移動 (關閉腳本)
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            // 重要：把物理速度歸零，不然玩家會滑出去
            playerRb.linearVelocity = Vector2.zero;
        }

        // 3. 鎖住玩家攻擊
        if (playerCombat != null) playerCombat.enabled = false;

        // 4. (選用) 如果你有坐下動畫，在這裡觸發
        // if (playerAnim != null) playerAnim.SetBool("IsSitting", true);
    }

    void StandUp()
    {
        isSitting = false;
        Debug.Log("起立！");

        // 1. 恢復控制
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;

        // 2. (選用) 停止坐下動畫
        // if (playerAnim != null) playerAnim.SetBool("IsSitting", false);
    }

    // --- 偵測範圍 ---
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // 趁機把玩家身上的組件抓過來存好
            playerMovement = collision.GetComponent<PlayerMovement>();
            playerCombat = collision.GetComponent<PlayerCombat>();
            playerHealth = collision.GetComponent<PlayerHealth>();
            playerRb = collision.GetComponent<Rigidbody2D>();
            // playerAnim = collision.GetComponentInChildren<Animator>(); // 如果有動畫再開這行
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}