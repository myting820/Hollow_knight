using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 往「右邊」飛 (發射時我們會旋轉發射點來決定實際方向)
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. 撞到主角 -> 扣血
        PlayerHealth player = hitInfo.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage, transform); // 傳入 transform 以計算擊退
            Destroy(gameObject);
        }

        // 2. 撞到牆壁 -> 消失
        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}