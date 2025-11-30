using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("跟隨設定")]
    public float followSpeed = 3f;      // 跟隨的平滑速度
    public Vector3 followOffset = new Vector3(-0.5f, 1f, 0); // 飄在主角頭頂偏後的位置

    [Header("狀態 (唯讀)")]
    public bool isCollected = false;    // 是否已被撿起

    private Transform player;           // 記錄主角的位置

    void Update()
    {
        // 如果被撿起來了，就持續跟隨主角
        if (isCollected && player != null)
        {
            // 目標位置 = 主角位置 + 偏移量
            // (這裡我們用主角的 scale.x 來判斷鑰匙要在左邊還是右邊，讓它永遠飄在身後)
            float direction = player.localScale.x;
            Vector3 targetPos = player.position + new Vector3(followOffset.x * direction, followOffset.y, 0);

            // 使用 Lerp 讓移動有「飄動感」，而不是死板的黏著
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有在「還沒被撿起」的時候，才進行撿起判定
        if (!isCollected && collision.CompareTag("Player"))
        {
            Collect(collision.transform);
        }
    }

    // --- 被撿起 ---
    void Collect(Transform playerTransform)
    {
        isCollected = true;
        player = playerTransform;

        // 呼叫主角身上的血量腳本，告訴它「你拿到鑰匙了」
        // (我們等一下會去修改 PlayerHealth)
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.PickUpKey(this);
        }

        Debug.Log("撿起鑰匙！");
    }

    // --- 被丟下 (受傷時呼叫) ---
    public void Drop()
    {
        isCollected = false;
        player = null; // 斷開連結，不再跟隨
        Debug.Log("鑰匙掉落！");
    }
}