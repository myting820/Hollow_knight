using UnityEngine;
using System.Collections;

public class DarkFairyBoss : MonoBehaviour
{
    [Header("核心參考")]
    public Transform player;
    public Transform[] teleportPoints; // 傳送點陣列
    public GameObject meleeArea;       // 剛剛做的近戰判定框
    public GameObject magicPrefab;     // 魔法陣 Prefab

    [Header("戰鬥數值")]
    public float moveSpeed = 2f;
    public float meleeRange = 2.5f;    // 近戰觸發距離 (靠太近)
    public float attackRange = 8f;     // 遠程觸發距離
    public float teleportCooldown = 5f;

    [Header("狀態監控")]
    public bool isActing = false;      // 是否正在做動作 (硬直中)
    private float nextTeleportTime;
    private Animator anim;
    private int facingDirection = 1;   // 1向右, -1向左

    void Start()
    {
        anim = GetComponent<Animator>();

        // 自動抓玩家
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 確保近戰框一開始是關的
        if (meleeArea != null) meleeArea.SetActive(false);
    }

    void Update()
    {
        // 如果玩家死了，或 Boss 正在忙(攻擊/瞬移)，就不要動
        if (player == null || isActing) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- 面向玩家 ---
        //Flip towards player
        if (player.position.x > transform.position.x && facingDirection == -1) Flip();
        else if (player.position.x < transform.position.x && facingDirection == 1) Flip();


        // --- AI 決策樹 ---

        // 1. 玩家貼臉 -> 近戰攻擊
        if (distance <= meleeRange)
        {
            StartCoroutine(MeleeAttackRoutine());
        }
        // 2. 玩家太近但沒貼臉 -> 瞬移拉開距離 (如果冷卻好了)
        else if (distance < 4f && Time.time > nextTeleportTime)
        {
            StartCoroutine(TeleportRoutine());
        }
        // 3. 距離適中 -> 發射魔法
        else if (distance <= attackRange)
        {
            StartCoroutine(CastSpellRoutine());
        }
        // 4. 太遠 -> 飛過去
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    // --- 技能 1: 遠程魔法 ---
    IEnumerator CastSpellRoutine()
    {
        isActing = true;
        anim.SetTrigger("Cast"); // 播放抬手動畫

        // 等待動畫演到一半 (例如 0.5秒) 才生成魔法
        yield return new WaitForSeconds(0.5f);

        if (magicPrefab != null)
        {
            // 在玩家腳下生成魔法陣 (或是 Boss 前方，看你想怎樣)
            // 這裡設定為：直接在玩家當前位置生成 (追蹤)
            Instantiate(magicPrefab, player.position, Quaternion.identity);
        }

        // 等待後搖 (休息一下)
        yield return new WaitForSeconds(1f);
        isActing = false;
    }

    // --- 技能 2: 近戰攻擊 ---
    IEnumerator MeleeAttackRoutine()
    {
        isActing = true;
        anim.SetTrigger("Melee"); // 播放揮刀動畫

        // 前搖：等待刀子揮下去的那一刻
        yield return new WaitForSeconds(0.4f);

        // 開啟傷害判定
        if (meleeArea != null) meleeArea.SetActive(true);

        // 判定持續時間 (刀子揮過的時間)
        yield return new WaitForSeconds(0.2f);

        // 關閉傷害判定
        if (meleeArea != null) meleeArea.SetActive(false);

        // 後搖
        yield return new WaitForSeconds(0.5f);
        isActing = false;
    }

    // --- 技能 3: 瞬移 (Hit & Run) ---
    IEnumerator TeleportRoutine()
    {
        isActing = true;
        anim.SetTrigger("Teleport"); // 1. 播放消失動畫 (Out)

        // 等待消失動畫播完 (假設 0.5秒)
        yield return new WaitForSeconds(0.5f);

        // 2. 真正的位移 (改變座標)
        // 隨機選一個傳送點
        if (teleportPoints.Length > 0)
        {
            int randIndex = Random.Range(0, teleportPoints.Length);
            transform.position = teleportPoints[randIndex].position;
        }

        // 3. 播放出現動畫 (In - 也就是倒帶)
        anim.SetTrigger("Appear");

        // 等待出現動畫播完
        yield return new WaitForSeconds(0.5f);

        // 設定冷卻時間
        nextTeleportTime = Time.time + teleportCooldown;
        isActing = false;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    // 畫出攻擊範圍 (方便除錯)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}