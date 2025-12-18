using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("傷害設定")]
    public int damageAmount = 1;
    public bool destroyOnImpact = false;

    [Header("自我毀滅設定 (Magic Prefab 用)")]
    public bool destroyAfterTime = false;
    public float lifeTime = 1f;

    void Start()
    {
        if (destroyAfterTime)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    // ---------------------------------------------------------
    // 情況 A：給「穿透型」物件用 (陷阱、魔法彈) -> Is Trigger 打勾
    // ---------------------------------------------------------
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DealDamage(collision.gameObject); // 呼叫共用功能
        }
    }

    // ---------------------------------------------------------
    // 【新增】情況 B：給「實體型」物件用 (小怪) -> Is Trigger 沒打勾
    // ---------------------------------------------------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject); // 呼叫共用功能
        }
    }

    // ---------------------------------------------------------
    // 把受傷邏輯抽出來寫成一個功能，這樣上面兩個人都可以呼叫，不用寫兩遍
    // ---------------------------------------------------------
    void DealDamage(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // 注意：這裡我保留你原本的寫法，傳送 transform 是為了計算擊退方向
            playerHealth.TakeDamage(damageAmount, transform);
        }

        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
}