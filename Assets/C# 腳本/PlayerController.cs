using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerCombat combat;

    private float moveInput;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // 1. 移動
        moveInput = Input.GetAxisRaw("Horizontal");
        movement.Move(moveInput);

        // 2. 跳躍
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movement.Jump();
        }

        // 3. 衝刺
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            movement.Dash();
        }

        // 4. 攻擊 (傳入 Vertical 輸入)
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            // GetAxisRaw("Vertical") 會回傳 1 (上), -1 (下), 0 (沒按)
            combat.Attack(Input.GetAxisRaw("Vertical"));
        }

        // 新增：魔法 (按 K)
        if (Input.GetKeyDown(KeyCode.E))
        {
            combat.CastSpell();
        }
    }
}