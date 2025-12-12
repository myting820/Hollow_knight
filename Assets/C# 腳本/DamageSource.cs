using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("傷害設定")]
    public int damageAmount = 1;      // 造成的傷害量
    public bool destroyOnImpact = false; // 撞到人是否消失 (子彈要勾，魔法陣不用)

    [Header("自我毀滅設定 (Magic Prefab 用)")]
    public bool destroyAfterTime = false; // 是否啟用時間銷毀
    public float lifeTime = 1f;           // 存活時間 (秒)

    void Start()
    {
        // 如果勾選了自我毀滅，就在 lifeTime 秒後自動刪除自己
        if (destroyAfterTime)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 判斷撞到的是不是玩家 (或是敵人，看你把這個腳本掛在誰身上)
        if (collision.CompareTag("Player"))
        {
            // 呼叫玩家受傷
            // (假設你的玩家有 PlayerHealth 腳本)
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform);
            }

            // 如果是子彈，撞到人就銷毀
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}