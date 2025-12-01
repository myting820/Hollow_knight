using UnityEngine;

public class FlyingShooter : MonoBehaviour
{
    [Header("基礎數值")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 4f;

    [Header("AI 感知設定")]
    public float detectionRange = 8f;
    public float stoppingDistance = 4f;
    public float retreatDistance = 3f;

    [Header("巡邏與障礙設定")]
    public Transform wallCheck;
    public float checkDistance = 0.6f;
    public LayerMask whatIsWall; // 這裡同時當作「巡邏撞牆偵測」和「視線阻擋偵測」

    [Header("射擊設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    private Transform player;
    private Rigidbody2D rb;
    private int facingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        nextFireTime = Time.time + fireRate;
    }

    void Update()
    {
        if (player == null)
        {
            PatrolLogic();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- 修改重點 ---
        // 判斷條件變成了：距離夠近 且 (AND) 看得到玩家
        if (distanceToPlayer < detectionRange && CanSeePlayer(distanceToPlayer))
        {
            EngagePlayer(distanceToPlayer);
        }
        else
        {
            // 看不到或是太遠 -> 回去巡邏
            PatrolLogic();
        }
    }

    // --- 新增：視線檢查函式 ---
    bool CanSeePlayer(float distance)
    {
        // 1. 計算方向
        Vector2 direction = (player.position - transform.position).normalized;

        // 2. 發射射線，只偵測 whatIsWall (牆壁圖層)
        // 注意：我們直接從 transform.position (身體中心) 發射即可
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, whatIsWall);

        // 3. 如果打到了東西 (hit.collider 不為空)，代表被牆壁擋住了
        if (hit.collider != null)
        {
            // (選用) 畫紅線 Debug
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return false; // 看不到
        }

        // (選用) 畫綠線 Debug
        Debug.DrawLine(transform.position, player.position, Color.green);
        return true; // 看得到
    }

    void PatrolLogic()
    {
        rb.linearVelocity = new Vector2(moveSpeed * facingDirection, rb.linearVelocity.y);

        bool hitWall = Physics2D.Raycast(wallCheck.position, transform.right, checkDistance, whatIsWall);
        if (hitWall)
        {
            Flip();
        }
    }

    void EngagePlayer(float distance)
    {
        // 1. 面向玩家
        if (player.position.x > transform.position.x && facingDirection == -1) Flip();
        else if (player.position.x < transform.position.x && facingDirection == 1) Flip();

        // 2. 移動邏輯
        if (distance > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else if (distance < retreatDistance)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 3. 射擊
        if (Time.time >= nextFireTime)
        {
            AimAndShoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void AimAndShoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Vector3 direction = player.position - firePoint.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(bulletPrefab, firePoint.position, rotation);
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * checkDistance);
        }
    }
}