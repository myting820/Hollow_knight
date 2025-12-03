using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("攻擊數值")]
    public int attackDamage = 20;
    public Vector2 attackArea = new Vector2(1.5f, 0.5f);
    public float attackRate = 2f;
    private float nextAttackTime = 0f;
    public int manaGainPerHit = 11;

    [Header("魔法設定")]
    public GameObject fireballPrefab; // 火球的樣本
    public Transform firePoint;       // 火球發射位置
    public float fireRate = 1f;       // 魔法冷卻
    private float nextFireTime = 0f;
    public int fireballCost = 33;

    [Header("設定")]
    public Transform attackPoint;
    public GameObject slashEffect;
    public LayerMask enemyLayers;

    private Animator animator;
    private PlayerMovement movement;
    private Vector3 defaultAttackPos;
    private PlayerMana playerMana;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        playerMana = GetComponent<PlayerMana>();
        if (attackPoint != null) defaultAttackPos = attackPoint.localPosition;

        if (slashEffect != null) slashEffect.SetActive(false);
    }

    public void Attack(float yInput)
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + 1f / attackRate;

        if (animator != null) animator.SetTrigger("AttackTrigger");

        StartCoroutine(ShowSlashEffect());

        // --- 處理攻擊方向與旋轉 ---
        // (這裡的邏輯保持不變，因為方形會跟著 AttackPoint 的旋轉角度走)
        if (!movement.isGrounded && yInput < 0)
        {
            attackPoint.localPosition = new Vector3(0, -1f, 0);
            attackPoint.localRotation = Quaternion.Euler(0, 0, -90);
        }
        else if (yInput > 0)
        {
            attackPoint.localPosition = new Vector3(0, 1f, 0);
            attackPoint.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            attackPoint.localPosition = defaultAttackPos;
            attackPoint.localRotation = Quaternion.Euler(0, 0, 0);
        }

        // --- 修改 2: 偵測命中改用 OverlapBoxAll ---
        // 參數說明: (中心點, 長寬尺寸, 旋轉角度, 圖層)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage, transform);

                if (playerMana != null)
                {
                    playerMana.GainMana(manaGainPerHit);
                }

                if (!movement.isGrounded && yInput < 0) movement.PogoJump();
            }
        }
    }

    // 施法函式
    public void CastSpell()
    {
        if (Time.time < nextFireTime) return;

        // 【修改】檢查靈魂是否足夠
        // 如果 playerMana 存在，且 TrySpendMana 回傳 true (代表扣款成功)，才發射
        if (playerMana != null && playerMana.TrySpendMana(fireballCost))
        {
            nextFireTime = Time.time + 1f / fireRate;

            // 生成火球
            Quaternion rotation = transform.localScale.x > 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
            Instantiate(fireballPrefab, firePoint.position, rotation);
        }
        else
        {
            // 這裡可以加一個 "登登!" 的失敗音效
            Debug.Log("魔力不足或冷卻中");
        }
    }

    IEnumerator ShowSlashEffect()
    {
        if (slashEffect != null)
        {
            slashEffect.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            slashEffect.SetActive(false);
        }
    }

    // 修改 3: 讓編輯器畫出方形，方便你調整
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        // 這行魔法是為了讓畫出來的紅色框框跟著物體旋轉
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        // 畫出線框方塊
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}