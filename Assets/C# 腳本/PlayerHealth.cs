using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class PlayerHealth : MonoBehaviour
{
    // ==========================================
    // 1. 變數宣告區
    // ==========================================
    [Header("血量設定")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("無敵時間設定")]
    public float invincibilityDuration = 0.75f;
    public float flashDuration = 0.08f;

    [Header("擊退設定")]
    public float knockbackForceX = 10f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.2f;

    [Header("參考組件")]
    public HealthUI healthUI;
    public KeyItem currentKey; // 記錄當前持有的鑰匙

    // 內部組件
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerMovement movement;
    private CinemachineImpulseSource impulseSource;


    // ==========================================
    // 2. 初始化與生命週期
    // ==========================================
    void Start()
    {
        currentHealth = maxHealth;

        // 抓取組件 (使用 GetComponentInChildren 以支援父子分離架構)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        impulseSource = GetComponent<CinemachineImpulseSource>(); 

        // 初始化 UI
        if (healthUI != null)
        {
            healthUI.InitHealth(maxHealth);
            healthUI.UpdateHealth(currentHealth);
        }
    }


    // ==========================================
    // 3. 公開互動函式 (給外部呼叫用)
    // ==========================================

    // 受傷扣血 (給 DamageSource 呼叫)
    public void TakeDamage(int damage, Transform source = null)
    {
        // A. 無敵/衝刺檢查
        if (movement != null && movement.isDashing) return; // 衝刺無敵
        if (isInvincible) return; // 受傷無敵

        // B. 掉落鑰匙
        if (currentKey != null)
        {
            currentKey.Drop();
            currentKey = null;
        }


        // C. 扣血邏輯
        currentHealth -= damage;
        Debug.Log("玩家受傷！剩餘血量：" + currentHealth);

        // 【新增】觸發震動！
        // GenerateImpulseWithForce(1f) 代表使用 100% 的力道
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithForce(1f);
        }

        // 更新 UI
        if (healthUI != null) healthUI.UpdateHealth(currentHealth);

        // 觸發動畫
        if (animator != null) animator.SetTrigger("HurtTrigger");

        // D. 執行擊退
        if (movement != null && source != null)
        {
            int direction = transform.position.x > source.position.x ? 1 : -1;
            Vector2 knockbackVector = new Vector2(direction * knockbackForceX, knockbackForceY);
            movement.ApplyKnockback(knockbackVector, knockbackDuration);
        }

        // E. 死亡檢查
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // F. 開始無敵閃爍
        StartCoroutine(InvincibilityRoutine());
    }

    // 【新增】補滿血量 (給 RestPoint 長椅呼叫) -> 放在 TakeDamage 下面很合理
    public void HealFull()
    {
        // 1. 數值補滿
        currentHealth = maxHealth;

        // 2. 狀態重置 (選用：如果坐椅子想順便解除無敵狀態)
        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        // 3. 更新 UI
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }

        Debug.Log("血量已補滿！");
    }

    // 撿起鑰匙 (給 KeyItem 呼叫)
    public void PickUpKey(KeyItem key)
    {
        currentKey = key;
    }





    // ==========================================
    // 4. 內部邏輯 (私有函式)
    // ==========================================

    void Die()
    {
        Debug.Log("玩家死亡！");
        // 【修改】呼叫 GameManager 進行重生
        if (GameManager.instance != null)
        {
            GameManager.instance.RespawnPlayer();
        }
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