using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("狀態監控")]
    public Vector3 respawnPoint;
    public GameObject victoryPanel; 
    private PlayerHealth playerHealth;
    private GameObject playerObj;
    private List<Enemy> allEnemies = new List<Enemy>();

    // ★ 新增：用來存所有的 Boss 觸發器
    private List<BossTrigger> allTriggers = new List<BossTrigger>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 抓取主角
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            respawnPoint = playerObj.transform.position;
        }

        // 抓取所有敵人
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            allEnemies.Add(enemy);
        }

        // ★ 新增：抓取所有 BossTrigger
        BossTrigger[] triggers = FindObjectsOfType<BossTrigger>();
        foreach (BossTrigger trigger in triggers)
        {
            allTriggers.Add(trigger);
        }
        // 【新增】確保遊戲開始時時間是流動的 (避免上次贏了之後時間停在 0)
        Time.timeScale = 1f;
        
        // 【新增】確保勝利畫面一開始是關閉的
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }
    }

    public void UpdateCheckpoint(Vector3 newPos)
    {
        respawnPoint = newPos;
        Debug.Log("存檔點已更新！");
    }

    public void RespawnPlayer()
    {
        Debug.Log("重生中...");

        // 1. 重置主角位置
        if (playerObj != null)
        {
            playerObj.transform.position = respawnPoint;
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 2. 重置主角狀態
        if (playerHealth != null)
        {
            playerHealth.HealFull();
        }

        // 3. 重置所有敵人
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.ResetEnemy();
            }
        }

        // ★ 新增：重置所有 Boss 觸發器
        foreach (BossTrigger trigger in allTriggers)
        {
            if (trigger != null)
            {
                trigger.ResetTrigger();
            }
        }
        // 5. 【新增】重置機關門 (DropTrap)
        // 這裡我們直接在場景找 DropTrap 腳本並呼叫它的 ResetTrap
        DropTrap trap = FindObjectOfType<DropTrap>();
        if (trap != null)
        {
            trap.ResetTrap();
        }

        Debug.Log("🔍 GameManager 正在尋找 Boss...");

        // 【修改】原本的 FindObjectOfType 改成下面這行「透視眼」指令：
        // FindFirstObjectByType 是 Unity 6 / 2023+ 的新寫法
        // FindObjectsInactive.Include 代表「就算 Boss 是關掉的，也要把他抓出來！」
        DarkFairyBoss boss = FindFirstObjectByType<DarkFairyBoss>(FindObjectsInactive.Include); 
        
        if (boss != null)
        {
            Debug.Log("👉 抓到了！Boss 名字是：" + boss.name);
            boss.ResetBoss(); // 執行重置
        }
        else
        {
            Debug.LogError("❌ 還是找不到！這真的太奇怪了，請確認場景裡真的有 DarkFairyBoss 這個物件嗎？");
        }
    }
    // 【新增】這個功能給 Boss 死掉的時候呼叫
    public void GameWin()
    {
        Debug.Log("🎉 恭喜通關！顯示勝利畫面！");

        // 1. 顯示勝利面板
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // 2. 暫停遊戲時間 (所有人都會定格，包括主角)
        Time.timeScale = 0f; 
    }
}