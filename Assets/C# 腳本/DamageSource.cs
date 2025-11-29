using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public int damageAmount = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // 【修改】把 transform (自己) 傳進去
            health.TakeDamage(damageAmount, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // 【修改】把 transform (自己) 傳進去
            health.TakeDamage(damageAmount, transform);
        }
    }
}