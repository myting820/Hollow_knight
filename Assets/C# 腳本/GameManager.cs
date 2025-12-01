using UnityEngine;
using System.Collections.Generic; // 用來使用 List

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 單例模式 (讓大家都能輕鬆找到它)

    [Header("狀態監控")]
    public Vector3 respawnPoint; // 重生點座標

    private PlayerHealth playerHealth;
    private GameObject playerObj;
    private List<Enemy> allEnemies = new List<Enemy>(); // 場景上所有敵人的名單

    void Awake()
    {
        // 設定單例 (確保只有一個管理器)
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 抓取主角
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            // 預設重生點 = 遊戲開始時主角的位置
            respawnPoint = playerObj.transform.position;
        }

        // 2. 抓取場景上所有的敵人，存入名單
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            allEnemies.Add(enemy);
        }
    }

    void Update()
    {
        // 測試功能：按 R 鍵自殺/重生
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }
    }

    // 更新存檔點 (給長椅呼叫)
    public void UpdateCheckpoint(Vector3 newPos)
    {
        respawnPoint = newPos;
        Debug.Log("存檔點已更新！");
    }

    // 執行重生流程
    public void RespawnPlayer()
    {
        Debug.Log("重生中...");

        // 1. 重置主角位置
        if (playerObj != null)
        {
            playerObj.transform.position = respawnPoint;

            // 讓物理速度歸零 (避免帶著慣性飛出去)
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 2. 重置主角狀態 (補滿血)
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
    }
}