using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("無敵時間設定")]
    public float invincibilityDuration = 0.75f; // 你剛剛調整的 0.75
    public float flashDuration = 0.08f;

    [Header("擊退設定")]
    public float knockbackForceX = 10f; // 水平擊退力
    public float knockbackForceY = 5f;  // 垂直擊退力 (稍微往上彈)
    public float knockbackDuration = 0.2f; // 擊退失控時間

    public HealthUI healthUI;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // 1. 新增：用來讀取主角移動狀態
    private PlayerMovement movement;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        // 2. 抓取同一物件身上的 PlayerMovement 組件
        movement = GetComponent<PlayerMovement>();

        // 【新增】遊戲開始時，初始化 UI
        if (healthUI != null)
        {
            healthUI.InitHealth(maxHealth);
            healthUI.UpdateHealth(currentHealth); // 確保一開始是滿的
        }
    }

    public void TakeDamage(int damage, Transform source = null)
    {
        if (movement != null && movement.isDashing)
        {
            Debug.Log("衝刺無敵中，免疫傷害！");
            return;
        }

        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("玩家受傷！剩餘血量：" + currentHealth);

        // 【新增】受傷時通知 UI 更新
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }

        if (animator != null) animator.SetTrigger("HurtTrigger");

        // --- 【新增】計算擊退 ---
        if (movement != null && source != null)
        {
            // 1. 計算方向：(主角位置 - 敵人位置) = 往反方向飛
            // 我們只關心左右方向，所以只看 x
            int direction = transform.position.x > source.position.x ? 1 : -1;

            // 2. 組合力道向量 (X:反方向力道, Y:稍微向上)
            Vector2 knockbackVector = new Vector2(direction * knockbackForceX, knockbackForceY);

            // 3. 呼叫 Movement 執行
            movement.ApplyKnockback(knockbackVector, knockbackDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityRoutine());
    }

    void Die()
    {
        Debug.Log("玩家死亡！");
        Time.timeScale = 0;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float endTime = Time.time + invincibilityDuration;

        while (Time.time < endTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                yield return new WaitForSeconds(flashDuration);
            }
            else
            {
                yield return null;
            }
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        isInvincible = false;


    }


}