using UnityEngine;
using System.Collections;

public class DarkFairyBoss : MonoBehaviour
{
    [Header("參考物件")]
    public Transform player;
    public Transform[] teleportPoints; // 傳送點陣列
    public GameObject meleeArea;       // 近戰攻擊判定區
    public GameObject magicPrefab;     // 魔法球 Prefab

    [Header("數值設定")]
    public float moveSpeed = 2f;
    public float meleeRange = 2.5f;    // 近戰觸發距離（地面）
    public float teleportRange = 4.0f;  // 【新增】綠色圈圈 (觸發逃跑的距離)
    public float attackRange = 8f;     // 遠程觸發距離
    public float teleportCooldown = 8f;

    [Header("攻擊頻率 (數字越小打越快)")]
    public float spellInterval = 2.0f;  // 放完魔法後的發呆時間
    public float meleeInterval = 1.0f;  // 砍完後的發呆時間

    [Header("狀態控制")]
    public bool isActing = false;      // 是否正在執行動作（攻擊 / 傳送）
    private float nextTeleportTime;
    private Animator anim;
    private int facingDirection = 1;   // 1 向右, -1 向左

    // 【新增 1】用來記錄 Boss 一開始的位置
    private Vector3 startPos;

    void Start()
    {
        anim = GetComponent<Animator>();

        // 自動尋找玩家
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 確保近戰判定一開始是關閉的
        if (meleeArea != null) meleeArea.SetActive(false);

        // 記住出生位置
        startPos = transform.position;
    }

    void Update()
    {
        // 如果玩家不存在，或 Boss 正在執行動作（攻擊 / 傳送），就不做事
        if (player == null || isActing) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- 面向玩家 ---
        // Flip towards player
        if (player.position.x > transform.position.x && facingDirection == -1) Flip();
        else if (player.position.x < transform.position.x && facingDirection == 1) Flip();

        // --- AI 決策樹 (邏輯大修) ---

        // 【優先級 1】保命要緊：如果玩家進入「逃跑圈(綠)」 且 冷卻好了 -> 瞬移逃走
        // 這樣就算玩家貼臉(在紅圈內)，只要冷卻好，她也會優先選擇逃跑而不是揮刀
        if (distance <= teleportRange && Time.time > nextTeleportTime)
        {
            StartCoroutine(TeleportRoutine());
        }
        // 【優先級 2】沒得逃(冷卻中)且貼臉 -> 被迫近戰反擊
        else if (distance <= meleeRange)
        {
            StartCoroutine(MeleeAttackRoutine());
        }
        // 【優先級 3】距離適中 -> 發射魔法
        else if (distance <= attackRange)
        {
            StartCoroutine(CastSpellRoutine());
        }
        // 【優先級 4】太遠 -> 追擊
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    
    }
    // 【新增 3】這就是我們要呼叫的重置功能
    public void ResetBoss()
    {
        // 1. 強制停止所有進行中的技能 (很重要！不然 Coroutine 會繼續跑)
        StopAllCoroutines();

        // 2. 解鎖大腦，讓變數回歸 false
        isActing = false;

        // 3. 【新增】重新尋找玩家 (這一步超級重要！)
        // 因為重生後的玩家是「新的人」，Boss 手上的舊資料已經過期了
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        
        // 4. 關閉攻擊判定框 (避免重生瞬間玩家被打)
        if (meleeArea != null) meleeArea.SetActive(false);

        // 5. 回到出生點
        transform.position = startPos;
        
        // 6. 重置動畫狀態 (避免卡在舉手動作)
        if (anim != null)
        {
            anim.Rebind(); // 強制重置 Animator 到初始狀態
            anim.Update(0f); 
        }

        Debug.Log("Boss 已重置！");
    }


    // --- 技能 1：遠程魔法 ---
    IEnumerator CastSpellRoutine()
    {
        isActing = true;
        anim.SetTrigger("Cast"); // 播放施法動畫

        // 等動畫前搖（例如 0.5 秒）
        yield return new WaitForSeconds(0.5f);

        if (magicPrefab != null)
        {
            // 在玩家位置生成魔法球（追蹤型或定點型）
            Instantiate(magicPrefab, player.position, Quaternion.identity);
        }

        // 攻擊後硬直
        yield return new WaitForSeconds(spellInterval);
        isActing = false;
    }

    // --- 技能 2：近戰攻擊 ---
    IEnumerator MeleeAttackRoutine()
    {
        isActing = true;
        anim.SetTrigger("Melee"); // 播放近戰動畫

        // 前搖
        yield return new WaitForSeconds(0.4f);

        // 開啟攻擊判定
        if (meleeArea != null) meleeArea.SetActive(true);

        // 判定持續時間
        yield return new WaitForSeconds(1.5f);

        // 關閉攻擊判定
        if (meleeArea != null) meleeArea.SetActive(false);

        // 後搖
        yield return new WaitForSeconds(meleeInterval);
        isActing = false;
    }

    // --- 技能 3：瞬移（Hit & Run） ---
    IEnumerator TeleportRoutine()
    {
        isActing = true;
        anim.SetTrigger("Teleport"); // 消失動畫（Out）

        // 等消失動畫結束
        yield return new WaitForSeconds(0.5f);

        // 隨機選一個傳送點
        if (teleportPoints.Length > 0)
        {
            int randIndex = Random.Range(0, teleportPoints.Length);
            transform.position = teleportPoints[randIndex].position;
        }

        // 出現動畫（In）
        anim.SetTrigger("Appear");

        // 等出現動畫
        yield return new WaitForSeconds(0.5f);

        // 設定傳送冷卻時間
        nextTeleportTime = Time.time + teleportCooldown;
        isActing = false;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(
            facingDirection * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            1
        );
    }

    // 在 Scene 視窗顯示攻擊範圍（方便調整）
    void OnDrawGizmosSelected()
    {
        // 紅色：近戰攻擊範圍 (貼臉)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        // 綠色：逃跑觸發範圍 (比近戰大一點)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, teleportRange);

        // 黃色：魔法攻擊範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
