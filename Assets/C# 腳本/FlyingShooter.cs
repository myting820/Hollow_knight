using UnityEngine;

public class FlyingShooter : MonoBehaviour
{
    [Header("飛行巡邏設定")]
    public float moveSpeed = 3f;
    public Transform wallCheck;
    public float checkDistance = 0.6f;
    public LayerMask whatIsWall;

    [Header("射擊設定")]
    public GameObject bulletPrefab; // 敵人子彈 Prefab
    public Transform firePoint;     // 發射點
    public float fireRate = 2f;     // 幾秒射一次
    private float nextFireTime;

    private Rigidbody2D rb;
    private int facingDirection = 1; // 1=右, -1=左

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextFireTime = Time.time + fireRate; // 初始冷卻
    }

    void FixedUpdate()
    {
        // 1. 飛行移動
        rb.linearVelocity = new Vector2(moveSpeed * facingDirection, rb.linearVelocity.y);

        // 2. 偵測牆壁 (碰到就回頭)
        bool hitWall = Physics2D.Raycast(wallCheck.position, transform.right, checkDistance, whatIsWall);
        if (hitWall)
        {
            Flip();
        }
    }

    void Update()
    {
        // 3. 定時射擊
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // 1. 判斷現在怪物面向哪邊
            // 如果 Scale X 是正的(1)，代表向右，角度就是 0
            // 如果 Scale X 是負的(-1)，代表向左，角度就是 180 (繞 Y 軸轉半圈)
            Quaternion bulletRotation;

            if (transform.localScale.x > 0)
            {
                bulletRotation = Quaternion.identity; // 0度 (向右)
            }
            else
            {
                bulletRotation = Quaternion.Euler(0, 180, 0); // 180度 (向左)
            }

            // 2. 生成子彈時，傳入我們算好的 bulletRotation
            Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
        // 轉身時，發射點和偵測點會自動跟著轉過去
    }

    void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * checkDistance);
        }
    }
}