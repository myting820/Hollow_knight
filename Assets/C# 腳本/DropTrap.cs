using UnityEngine;

public class DropTrap : MonoBehaviour
{
    [Header("設定")]
    public Rigidbody2D trapGate; // 那個要掉下來的門
    public float dropGravity = 8f; // 下墜速度 (重力倍率)

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果玩家走過去，且還沒觸發過
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            ActivateTrap();
        }
    }

    void ActivateTrap()
    {
        hasTriggered = true;
        Debug.Log("機關觸發！門掉下來了！");

        if (trapGate != null)
        {
            // 【關鍵】把門從「懸浮 (Kinematic)」改成「受重力 (Dynamic)」
            trapGate.bodyType = RigidbodyType2D.Dynamic;
            trapGate.gravityScale = dropGravity;

            // 2. 【新增這行】強制喚醒剛體！
            trapGate.WakeUp();
        }

        // (選用) 播放石頭摩擦聲或重擊聲
    }
}