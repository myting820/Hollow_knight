using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("圖片設定")]
    public Sprite openSprite; // 開門後的圖片

    private BoxCollider2D physicsCollider;
    private SpriteRenderer sr;

    void Start()
    {
        physicsCollider = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // 當有物體「撞」到門的時候觸發
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 確認撞到的是不是主角
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. 檢查主角身上有沒有鑰匙
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            // 如果主角存在 且 手上有鑰匙
            if (player != null && player.currentKey != null)
            {
                OpenDoor(player);
            }
            else
            {
                Debug.Log("門鎖著，你需要鑰匙！");
            }
        }
    }

    void OpenDoor(PlayerHealth player)
    {
        Debug.Log("門開了！");

        // 1. 消耗鑰匙
        // 銷毀跟隨在主角身後的鑰匙物件
        Destroy(player.currentKey.gameObject);
        // 清空主角身上的鑰匙記錄
        player.currentKey = null;

        // 2. 視覺變化：換成開門圖片
        if (openSprite != null)
        {
            sr.sprite = openSprite;
        }
        else
        {
            // 如果沒設定開門圖，就直接把門隱藏
            sr.enabled = false;
        }

        // 3. 物理變化：關閉碰撞體 (讓玩家可以走過去)
        physicsCollider.enabled = false;

        // 4. (選用) 播放音效或特效
    }
}