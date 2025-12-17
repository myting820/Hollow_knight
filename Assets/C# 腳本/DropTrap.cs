using UnityEngine;

public class DropTrap : MonoBehaviour
{
    [Header("陷阱")]
    public Rigidbody2D trapGate; // 閘門要掉下來的物件
    public float dropGravity = 8f; // 下落速度 (重力影響)

    private bool hasTriggered = false;
    private Vector3 startPosition; // 【新增】用來記住門原本的位置

    void Start()
    {
        // 【新增】遊戲開始時，先記住門現在掛在哪裡
        if (trapGate != null)
        {
            startPosition = trapGate.transform.position;
            // 確保一開始是靜止的，避免還沒觸發就掉下來
            trapGate.bodyType = RigidbodyType2D.Static; 
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果是玩家經過，而且還沒觸發過
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            ActivateTrap();
        }
    }

    void ActivateTrap()
    {
        hasTriggered = true;
        Debug.Log("陷阱啟動！閘門掉下來了！");

        if (trapGate != null)
        {
            // 將剛體模式從靜態 (Kinematic) 轉為動態 (Dynamic)
            trapGate.bodyType = RigidbodyType2D.Dynamic;
            trapGate.gravityScale = dropGravity;

            // 2. 強制喚醒物理模擬
            trapGate.WakeUp();
        }

        // (選用) 如果有需要可以加音效
    }

    // 【新增】這個功能要在「玩家重生」的時候被呼叫
    public void ResetTrap()
    {
        if (trapGate != null)
        {
            // 1. 停止物理模擬 (變回 Static) 這樣它才不會繼續往下掉
            trapGate.bodyType = RigidbodyType2D.Static;
            trapGate.linearVelocity = Vector2.zero; // 速度歸零
            trapGate.angularVelocity = 0f;    // 旋轉歸零

            // 2. 瞬移回原本記錄的位置
            trapGate.transform.position = startPosition;
            trapGate.transform.rotation = Quaternion.identity; // 轉正(預防歪掉)
        }

        // 3. 重置開關，讓玩家下次經過可以再次觸發
        hasTriggered = false;
        
        Debug.Log("機關門已重置！");
    }
}
