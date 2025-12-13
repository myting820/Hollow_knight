using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("狀態監控")]
    public Vector3 respawnPoint;

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
    }
} // <--- 這裡原本可能少了一個括號 (error CS1513)