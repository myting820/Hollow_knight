using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;

    [Header("偵測設定")]
    public Transform wallCheck;   // 看牆壁的點
    public Transform ledgeCheck;  // 看懸崖的點
    public float checkDistance = 0.5f; // 射線長度
    public LayerMask whatIsGround;     // 什麼是地板(Ground圖層)

    private Rigidbody2D rb;
    private int facingDirection = 1; // 1 = 面向右, -1 = 面向左

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 1. 持續移動
        rb.linearVelocity = new Vector2(moveSpeed * facingDirection, rb.linearVelocity.y);

        // 2. 進行環境偵測
        CheckSurroundings();
    }

    void CheckSurroundings()
    {
        // --- A. 偵測牆壁 (向前射出一條線) ---
        // transform.right 會根據物體的面向自動改變方向
        bool hitWall = Physics2D.Raycast(wallCheck.position, transform.right, checkDistance, whatIsGround);

        // --- B. 偵測懸崖 (從前方往下射出一條線) ---
        bool hitGround = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, whatIsGround);

        // --- 3. 判斷轉身 ---
        // 如果 (撞到牆壁) 或者 (前面沒有地板了)
        if (hitWall || !hitGround)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    // 畫出輔助線 (讓你在 Scene 視窗看得到射線)
    void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            // 畫出牆壁偵測線
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * checkDistance);
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            // 畫出懸崖偵測線
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * checkDistance);
        }
    }
}