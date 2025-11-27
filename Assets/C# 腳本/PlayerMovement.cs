using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // ... (原本的數值保持不變) ...
    [Header("基礎數值")]
    public float moveSpeed = 8f;
    public float jumpForce = 22f;

    [Header("衝刺設定")]
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 0.4f;

    [Header("蹬牆跳設定")]
    public float wallSlidingSpeed = 2f;
    public Vector2 wallJumpPower = new Vector2(10f, 25f);
    public float wallJumpDuration = 0.2f;

    [Header("下劈設定")]
    public float pogoForce = 18f;

    [Header("偵測設定")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    // --- 內部狀態 ---
    private Rigidbody2D rb;
    public int facingDirection { get; private set; } = 1;
    public bool isGrounded { get; private set; }
    public bool isTouchingWall { get; private set; }
    public bool isWallSliding { get; private set; }
    public bool isDashing { get; private set; }
    public bool isWallJumping { get; private set; }

    // 【新增】擊退狀態：如果正在被擊退，就公開讓 Controller 知道
    public bool isKnockedBack { get; private set; }

    private bool canDoubleJump;
    private bool canDash = true;
    private bool hasDashedInAir;
    private float gravityStore;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityStore = rb.gravityScale;
    }

    void Update()
    {
        CheckSurroundings();
        HandleWallSlide();
    }

    public void Move(float input)
    {
        // 【修改】加入 isKnockedBack 檢查：如果正在被擊退，不能控制移動
        if (isDashing || isWallJumping || isKnockedBack) return;

        rb.linearVelocity = new Vector2(input * moveSpeed, rb.linearVelocity.y);

        if (input > 0 && facingDirection == -1) Flip();
        else if (input < 0 && facingDirection == 1) Flip();
    }

    public void Jump()
    {
        // 【修改】加入 isKnockedBack 檢查
        if (isKnockedBack) return;

        if (isWallSliding) StartCoroutine(WallJumpRoutine());
        else if (isGrounded) PerformJump();
        else if (canDoubleJump) { PerformJump(); canDoubleJump = false; hasDashedInAir = false; }
    }

    public void Dash()
    {
        // 【修改】加入 isKnockedBack 檢查
        if (isKnockedBack) return;

        if (canDash && (isGrounded || !hasDashedInAir)) StartCoroutine(DashRoutine());
    }

    public void PogoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoForce);
        canDoubleJump = true;
        hasDashedInAir = false;
        canDash = true;
    }

    // --- 【新增】擊退功能 ---
    // 參數：方向力道 (Vector2)、持續時間 (float)
    public void ApplyKnockback(Vector2 knockbackForce, float duration)
    {
        StartCoroutine(KnockbackRoutine(knockbackForce, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 knockbackForce, float duration)
    {
        isKnockedBack = true;

        // 1. 重置當前速度 (避免被之前的慣性影響)
        rb.linearVelocity = Vector2.zero;

        // 2. 施加擊退力 (向後上方飛)
        rb.linearVelocity = knockbackForce;

        // 3. 等待擊退時間 (這段時間玩家失去控制)
        yield return new WaitForSeconds(duration);

        // 4. 恢復控制
        // (為了手感，可以把速度歸零，或者讓重力自然接管，這裡我們讓重力接管)
        isKnockedBack = false;

        // 可選：擊退結束後稍微減速，避免滑太遠
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // ... (剩下的 PerformJump, Flip, CheckSurroundings 等保持不變) ...
    // ... (為節省版面，請保留你原本下面的程式碼) ...
    private void PerformJump() { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); }
    private void Flip() { facingDirection *= -1; transform.localScale = new Vector3(facingDirection, 1, 1); }
    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround);
        if (isGrounded || isWallSliding) { canDoubleJump = true; hasDashedInAir = false; }
    }
    private void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            if (rb.linearVelocity.y < -wallSlidingSpeed) rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlidingSpeed);
        }
        else isWallSliding = false;
    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true; isWallSliding = false;
        rb.linearVelocity = new Vector2(-facingDirection * wallJumpPower.x, wallJumpPower.y);
        Flip();
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }
    private IEnumerator DashRoutine()
    {
        canDash = false; isDashing = true;
        if (!isGrounded) hasDashedInAir = true;
        rb.gravityScale = 0f; rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravityStore; isDashing = false;
        yield return new WaitForSeconds(dashCooldown); canDash = true;
    }
}