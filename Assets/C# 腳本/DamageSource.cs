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

    // 穿透物件(陷阱、魔法彈) -> Is Trigger 打勾
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DealDamage(collision.gameObject); // 呼叫共用功能
        }
    }

    // 實體物件用 (小怪) -> Is Trigger 沒打勾
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject); // 呼叫共用功能
        }
    }
    void DealDamage(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount, transform);
        }

        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
}