using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Member;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 3;

    // Boss 專用血條 (一般小怪留空即可)
    [Header("Boss 專用設定")]
    public Slider bossHealthBar;

    [Header("受擊特效設定")]
    public Material flashMaterial;
    public float flashDuration = 0.15f;

    [Header("死亡特效設定")]
    public float deathFlashInterval = 0.2f;
    public GameObject explosionPrefab;

    [Header("擊退設定")] // 把這段搬上來，跟其他設定放在一起
    public float knockbackForce = 5f;
    public float stunDuration = 0.2f;

    // --- 以下是內部私有變數 (Inspector 不會顯示) ---
    public int currentHealth; // 雖然你原本寫在上面，但這是內部用的，不用顯示
    private SpriteRenderer sr;
    private Material originalMaterial;
    private Coroutine currentFlashRoutine;
    private bool isDying = false;
    private Vector3 startPosition;
    private Rigidbody2D rb;


    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        currentHealth = maxHealth;

        // 【新增】記住一開始的位置
        startPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, Transform source = null)
    {
        if (isDying) return;

        currentHealth -= damage;

        // 【新增】如果有綁定血條 (Boss)，就更新 UI
        if (bossHealthBar != null)
        {
            bossHealthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 1. 閃白特效
            if (currentFlashRoutine != null) StopCoroutine(currentFlashRoutine);
            currentFlashRoutine = StartCoroutine(HitFlashRoutine());

            // 2. 【新增】執行擊退
            if (source != null && rb != null)
            {
                // 計算方向：(怪物位置 - 攻擊者位置) = 往反方向飛
                Vector2 direction = (transform.position - source.position).normalized;
                // 稍微往上抬一點，避免磨擦地面
                Vector2 knockbackDir = new Vector2(direction.x, 0.2f).normalized;

                StartCoroutine(KnockbackRoutine(knockbackDir));
            }
        }
    }
    // 【新增】Boss 出場時呼叫這個，初始化血條
    public void ActivateBossUI()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.gameObject.SetActive(true); // 顯示血條
            bossHealthBar.maxValue = maxHealth;       // 設定最大值
            bossHealthBar.value = currentHealth;      // 設定當前值
        }
    }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        // A. 暫時關閉各種 AI 移動腳本
        // (我們嘗試抓取所有可能的 AI 腳本，有的話就關掉)
        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        FlyingShooter flying = GetComponent<FlyingShooter>();
        KamikazeAI kamikaze = GetComponent<KamikazeAI>();

        if (patrol) patrol.enabled = false;
        if (flying) flying.enabled = false;
        if (kamikaze) kamikaze.enabled = false;

        // B. 施加瞬間推力
        // 先歸零速度，避免原本的移動慣性干擾
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        // C. 等待暈眩時間
        yield return new WaitForSeconds(stunDuration);

        // D. 恢復速度並重新開啟 AI
        rb.linearVelocity = Vector2.zero; // 停下來
        if (patrol) patrol.enabled = true;
        if (flying) flying.enabled = true;
        if (kamikaze) kamikaze.enabled = true;
    }

    void Die()
    {
        isDying = true;

        // 關閉血條 (如果有的話)
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        // 停止碰撞與物理，避免屍體擋路
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false; // 停止物理模擬

        // 停止巡邏
        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        if (patrol) patrol.enabled = false;

        // 嘗試關閉各種 AI
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // 簡單粗暴地關閉所有名為 AI 或 Enemy 開頭的腳本 (除了自己)
            if (script != this && (script.GetType().Name.Contains("AI") || script.GetType().Name.Contains("Enemy")))
            {
                script.enabled = false;
            }
        }

        // --- 情況 B：死亡 ---
        // 執行「死亡閃爍 -> 爆炸」協程
        StartCoroutine(DeathSequenceRoutine());
    }

    // 1. 普通受擊：閃一下就回來
    IEnumerator HitFlashRoutine()
    {
        sr.material = flashMaterial;       // 變白
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMaterial;    // 變回來
        currentFlashRoutine = null;
    }

    // 【新增】復活函式 (給 GameManager 呼叫)
    public void ResetEnemy()
    {
        // 1. 恢復血量
        currentHealth = maxHealth;
        isDying = false; // 重置死亡狀態

        // 2. 恢復位置
        transform.position = startPosition;

        // 3. 恢復顯示與物理
        gameObject.SetActive(true); // 重新啟用物件

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        if (patrol) patrol.enabled = true;

        // 確保材質變回來
        if (sr != null) sr.material = originalMaterial;

        // ★【新增這段】強制關閉血條 UI
        // 因為回到記憶點時，戰鬥還沒開始，不該看到血條
        // 判斷：如果有綁定「Boss 血條」，代表我是 Boss
        if (bossHealthBar != null)
        {
            // 如果是 Boss：
            // 1. 隱藏血條 UI
            bossHealthBar.gameObject.SetActive(false);
            
            // 2. 【新增】把 Boss 本體也關掉！(讓它乖乖睡覺，等 Trigger 叫醒它)
            gameObject.SetActive(false);
        }
        else
        {
            // 如果是普通小怪：
            // 直接復活顯示
            gameObject.SetActive(true);
        }
    }

    // 2. 死亡序列：閃兩下 -> 爆炸 -> 消失
    IEnumerator DeathSequenceRoutine()
    {
        // 第 1 閃
        sr.material = flashMaterial;
        yield return new WaitForSeconds(deathFlashInterval);
        sr.material = originalMaterial;
        yield return new WaitForSeconds(deathFlashInterval);

        // 第 2 閃
        sr.material = flashMaterial;
        yield return new WaitForSeconds(deathFlashInterval);
        sr.material = originalMaterial;

        // 稍微停頓一瞬間，營造「處決」感 (選用，0.1秒)
        // yield return new WaitForSeconds(0.1f);

        // 生成爆炸特效
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 【修改】不要 Destroy，改成關閉自己
        // Destroy(gameObject); 
        gameObject.SetActive(false);

    }
}