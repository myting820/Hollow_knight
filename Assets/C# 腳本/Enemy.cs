using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("受擊特效設定")]
    public Material flashMaterial;      // 記得拖入那個 Unlit 的純白材質
    public float flashDuration = 0.15f; // 受擊閃爍時間 (原本0.1改長一點，0.15或0.2比較明顯)

    [Header("死亡特效設定")]
    public float deathFlashInterval = 0.2f; // 死亡閃爍間隔 (設長一點，節奏感較強)
    public GameObject explosionPrefab;      // 粒子爆炸 Prefab

    private SpriteRenderer sr;
    private Material originalMaterial;
    private Coroutine currentFlashRoutine;
    private bool isDying = false;
    private Vector3 startPosition; // 記錄出生點

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        currentHealth = maxHealth;

        // 【新增】記住一開始的位置
        startPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        if (isDying) return;

        currentHealth -= damage;
        Debug.Log(name + " 受到了 " + damage + " 點傷害！");

        if (currentHealth <= 0)
        {
            Die(); // 這裡會處理死亡閃爍 -> 爆炸
        }
        else
        {
            // --- 情況 A：受傷還沒死 ---
            // 執行「受擊閃白」協程
            if (currentFlashRoutine != null) StopCoroutine(currentFlashRoutine);
            currentFlashRoutine = StartCoroutine(HitFlashRoutine());
        }
    }

    void Die()
    {
        isDying = true;

        // 停止碰撞與物理，避免屍體擋路
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false; // 停止物理模擬

        // 停止巡邏
        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        if (patrol) patrol.enabled = false;

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