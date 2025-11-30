using UnityEngine;

public class KamikazeAI : MonoBehaviour
{
    [Header("飛行設定")]
    public float flySpeed = 6f;
    public float detectionRange = 10f;

    [Header("視線設定 ")]
    public LayerMask obstacleLayer; // 設定什麼是「牆壁」

    private Transform player;
    private Rigidbody2D rb;
    private Enemy myHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myHealth = GetComponent<Enemy>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 修改邏輯：距離夠近 且 視線沒有被擋住
        if (distanceToPlayer < detectionRange && CanSeePlayer(distanceToPlayer))
        {
            ChasePlayer();
        }
        else
        {
            // 超出範圍或被牆壁擋住，停下來
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 新增：視線檢查函式
    bool CanSeePlayer(float distance)
    {
        // 計算射線方向
        Vector2 direction = (player.position - transform.position).normalized;

        // 發射一條射線，只偵測 obstacleLayer (牆壁)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        // 如果射線打到了東西 (hit.collider 不為空)，代表打到了牆壁
        if (hit.collider != null)
        {
            // Debug 畫紅色線 (代表被擋住)
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return false; // 看不到主角
        }

        // Debug 畫綠色線 (代表看得到)
        Debug.DrawLine(transform.position, player.position, Color.green);
        return true; // 看得到主角
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * flySpeed;

        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 撞到主角 或 撞到地板 都自爆
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (myHealth != null) myHealth.TakeDamage(9999);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}