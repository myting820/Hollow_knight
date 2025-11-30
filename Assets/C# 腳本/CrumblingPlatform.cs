using UnityEngine;
using System.Collections;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("時間設定")]
    public float crumbleTime = 2f;    // 踩上去後幾秒會碎掉
    public float respawnTime = 3f;    // 碎掉後幾秒會重生

    [Header("視覺回饋")]
    public float shakeAmount = 0.05f; // 抖動幅度 (警告玩家用)
    public Color warningColor = new Color(0.6f, 0.6f, 0.6f); // 變色警告

    // 內部組件
    private BoxCollider2D myCollider;
    private SpriteRenderer myRenderer;
    private Vector3 originalPos;
    private Color originalColor;
    private bool isCrumbling = false; // 是否正在崩壞流程中

    void Start()
    {
        myCollider = GetComponent<BoxCollider2D>();
        myRenderer = GetComponent<SpriteRenderer>();

        // 記住原本的位置和顏色，方便重生時復原
        originalPos = transform.position;
        originalColor = myRenderer.color;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 只有玩家碰到，且目前沒有在崩壞中，才觸發
        if (collision.gameObject.CompareTag("Player") && !isCrumbling)
        {
            // 2. (選用) 檢查是否從「上方」踩到 (避免頭頂撞到也觸發)
            // 如果接觸點的法線向下 (y < 0)，代表玩家是在這個平台的上方
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(CrumbleRoutine());
                    break;
                }
            }
        }
    }

    IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;

        // --- 階段一：警告期 (抖動 + 變色) ---
        float timer = 0f;
        while (timer < crumbleTime)
        {
            // 隨機抖動位置
            float x = originalPos.x + Random.Range(-shakeAmount, shakeAmount);
            float y = originalPos.y + Random.Range(-shakeAmount, shakeAmount);
            transform.position = new Vector3(x, y, originalPos.z);

            // 稍微變暗
            myRenderer.color = warningColor;

            timer += Time.deltaTime;
            yield return null; // 等待下一幀
        }

        // --- 階段二：破碎 (消失) ---
        // 關閉碰撞 (讓玩家掉下去)
        myCollider.enabled = false;
        // 關閉圖片 (看起來消失)
        myRenderer.enabled = false;

        // (這裡可以生成碎石粒子特效 Instantiate(debrisEffect...))

        // --- 階段三：重生冷卻 ---
        yield return new WaitForSeconds(respawnTime);

        // --- 階段四：重生 ---
        // 恢復位置、顏色、碰撞、圖片
        transform.position = originalPos;
        myRenderer.color = originalColor;
        myRenderer.enabled = true;
        myCollider.enabled = true;

        isCrumbling = false;
    }
}