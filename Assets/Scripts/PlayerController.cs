using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rollMultiplier = 2f;   // how much faster the roll is
    [SerializeField] private float rollDuration = 0.3f;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;
    private bool isRolling = false;
    private string currentAnim;

    // Animation names
    private readonly string WALK_UP_NW    = "Walk_Up_NW";
    private readonly string WALK_DOWN_NW  = "Walk_Down_NW";
    private readonly string WALK_LEFT_NW  = "Walk_Left_NW";
    private readonly string WALK_RIGHT_NW = "Walk_Right_NW";

    private readonly string IDLE_UP_NW    = "Idle_Up_NW";
    private readonly string IDLE_DOWN_NW  = "Idle_Down_NW";
    private readonly string IDLE_LEFT_NW  = "Idle_Left_NW";
    private readonly string IDLE_RIGHT_NW = "Idle_Right_NW";

    private readonly string ROLL_UP_NW    = "Roll_Up_NW";
    private readonly string ROLL_DOWN_NW  = "Roll_Down_NW";
    private readonly string ROLL_LEFT_NW  = "Roll_Left_NW";
    private readonly string ROLL_RIGHT_NW = "Roll_Right_NW";

    void Update()
    {
        if (isRolling) return;

        // Read input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        // Choose walk or idle animation
        if (movement != Vector2.zero)
        {
            lastDirection = movement;
            SetWalkAnimFromDirection(movement);
        }
        else
        {
            SetIdleAnimFromDirection(lastDirection);
        }

        // Start roll
        if (Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(Roll());
    }

    void FixedUpdate()
    {
        if (!isRolling)
        {
            rb.velocity = movement * speed;
        }
        // else, velocity is being handled in Roll()
    }

    IEnumerator Roll()
    {
        isRolling = true;

        // Select roll animation based on last direction
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
            SetAnimation(lastDirection.x > 0 ? ROLL_RIGHT_NW : ROLL_LEFT_NW);
        else
            SetAnimation(lastDirection.y > 0 ? ROLL_UP_NW : ROLL_DOWN_NW);

        // Apply instantaneous velocity
        rb.velocity = lastDirection * speed * rollMultiplier;

        yield return new WaitForSeconds(rollDuration);

        // End roll
        rb.velocity = Vector2.zero;
        isRolling = false;
    }

    void SetAnimation(string anim)
    {
        if (anim == currentAnim) return;
        if (!animator.HasState(0, Animator.StringToHash(anim)))
        {
            Debug.LogError($"Animation '{anim}' not found on layer 0");
            return;
        }
        animator.Play(anim);
        currentAnim = anim;
    }

    void SetWalkAnimFromDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            SetAnimation(dir.x > 0 ? WALK_RIGHT_NW : WALK_LEFT_NW);
        }
        else
        {
            SetAnimation(dir.y > 0 ? WALK_UP_NW : WALK_DOWN_NW);
        }
    }

    void SetIdleAnimFromDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            SetAnimation(dir.x > 0 ? IDLE_RIGHT_NW : IDLE_LEFT_NW);
        }
        else
        {
            SetAnimation(dir.y > 0 ? IDLE_UP_NW : IDLE_DOWN_NW);
        }
    }
}
