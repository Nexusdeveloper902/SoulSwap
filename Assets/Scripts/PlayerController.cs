using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Body Settings")]
    [Tooltip("1 = No Weapon (suffix _NW); 2 = G (suffix _G); 3 = SS (suffix _SS)")]
    public int body = 1;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rollMultiplier = 2f;
    [SerializeField] private float rollDuration = 0.3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float attackSlowMultiplier = 0.5f;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    [Header("Block Settings (Body 3)")]
    [Tooltip("Block input for body 3")]
    [SerializeField] private KeyCode blockKey = KeyCode.Mouse1;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;
    private bool isRolling = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private string currentAnim;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Always read movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        // Handle block for body==3
        if (body == 3)
        {
            if (Input.GetKeyDown(blockKey) && !isRolling && !isAttacking)
                StartCoroutine(Block());

            // while blocking, ignore other actions
            if (isBlocking)
                return;
        }

        // Update facing and animations when not rolling/attacking/blocking
        if (!isRolling && !isAttacking && !isBlocking)
        {
            if (movement != Vector2.zero)
                lastDirection = movement;

            PlayWalkOrIdle();
        }

        // Roll
        if (!isRolling && !isAttacking && !isBlocking && Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(Roll());

        // Attack (available for all bodies)
        if (!isRolling && !isAttacking && !isBlocking && Input.GetKeyDown(attackKey))
            StartCoroutine(Attack());
    }

    void FixedUpdate()
    {
        if (isRolling)
            return;

        if (isAttacking)
        {
            rb.velocity = movement * speed * attackSlowMultiplier;
            return;
        }

        if (isBlocking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = movement * speed;
    }

    IEnumerator Roll()
    {
        isRolling = true;

        PlayDirectionalAnim("Roll");
        rb.velocity = lastDirection * speed * rollMultiplier;
        yield return new WaitForSeconds(rollDuration);
        isRolling = false;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        PlayDirectionalAnim("Attack");
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        PlayWalkOrIdle();
    }

    IEnumerator Block()
    {
        isBlocking = true;

        PlayDirectionalAnim("Block");
        // block lasts while key held; stop when released
        while (Input.GetKey(blockKey))
            yield return null;

        isBlocking = false;
        PlayWalkOrIdle();
    }

    private void PlayWalkOrIdle()
    {
        if (isRolling || isAttacking || isBlocking)
            return;

        if (movement != Vector2.zero)
            PlayDirectionalAnim("Walk");
        else
            PlayDirectionalAnim("Idle");
    }

    private void PlayDirectionalAnim(string action)
    {
        string direction = Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y)
            ? (lastDirection.x > 0 ? "Right" : "Left")
            : (lastDirection.y > 0 ? "Up" : "Down");

        PlayAnim(action, direction);
    }

    private void PlayAnim(string action, string direction)
    {
        string suffix = body == 1 ? "NW"
                      : body == 2 ? "G"
                      : body == 3 ? "SS" : "";
        string animName = $"{action}_{direction}_{suffix}";

        if (animName == currentAnim)
            return;

        int hash = Animator.StringToHash(animName);
        if (!animator.HasState(0, hash))
        {
            Debug.LogError($"Animation '{animName}' not found on layer 0");
            return;
        }

        animator.Play(animName);
        currentAnim = animName;
    }
}
