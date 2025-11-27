using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;       // 飛行速度
    public int damage = 10;         // 傷害
    public float lifeTime = 2f;     // 存活時間 (避免飛到無窮遠)
    public GameObject hitEffect;    // 撞擊特效(選用)

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 設定初速度 (向右飛)
        // 注意：我們會透過旋轉物件來控制它是向左還是向右飛，所以這裡永遠寫 transform.right
        rb.linearVelocity = transform.right * speed;

        // 時間到自動銷毀
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. 碰到敵人
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            //DestroyProjectile(); // 撞到人就消失
        }

        //// 2. 碰到牆壁 (假設牆壁圖層叫做 Ground)
        //// 你可以用 tag 判斷，或是檢查 layer
        //if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    DestroyProjectile();
        //}
    }

    void DestroyProjectile()
    {
        // 如果有做爆炸特效，在這裡生成
        // if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}