using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed       = 5f;
    public float jumpForce       = 7f;

    [Header("Ground Check")]
    public float     groundCheckRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("References")]
    public Transform groundCheck;
    public GameObject bulletPrefab;
    public Transform firePoint;

    // ── Private state ─────────────────────────────────────────────────────────
    Rigidbody2D    rb;
    SpriteRenderer spriteRenderer;
    Animator       anim;
    PlayerControls controls;

    float moveInput;
    bool  isGrounded;
    bool  jumpQueued;

    // Cached flags — set once in Awake; avoids per-frame parameter string lookups
    bool hasIsWalking;
    bool hasIsJumping;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rb            = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim           = GetComponent<Animator>();

        controls = new PlayerControls();
        controls.Gameplay.Jump.performed += OnJump;
        controls.Gameplay.Shoot.performed += OnShoot;

        // Only cache anim params if a controller is assigned (no controller yet in this step)
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            hasIsWalking = HasAnimParam(anim, "isWalking");
            hasIsJumping = HasAnimParam(anim, "isJumping");
        }
    }

    void OnEnable()  => controls.Enable();
    void OnDisable() => controls.Disable();

    void OnDestroy()
    {
        controls.Gameplay.Jump.performed -= OnJump;
        controls.Gameplay.Shoot.performed -= OnShoot;
        controls.Dispose();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        moveInput = controls.Gameplay.Move.ReadValue<Vector2>().x;

        // Flip sprite to face movement direction
        if      (moveInput >  0.01f) spriteRenderer.flipX = false;
        else if (moveInput < -0.01f) spriteRenderer.flipX = true;

        // Ground detection
        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Animator parameters — silently skip if controller / parameter is absent
        if (anim != null)
        {
            if (hasIsWalking) anim.SetBool("isWalking", Mathf.Abs(moveInput) > 0.01f);
            if (hasIsJumping) anim.SetBool("isJumping", !isGrounded);
        }
    }

    // ── FixedUpdate ───────────────────────────────────────────────────────────

    void FixedUpdate()
    {
        // Horizontal move — preserve existing vertical velocity
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (jumpQueued)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }
    }

    // ── Input callbacks ───────────────────────────────────────────────────────

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded) jumpQueued = true;
    }

    void OnShoot(InputAction.CallbackContext ctx)
    {
        Shoot();
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        float direction = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        if (bullet != null)
            bullet.SetDirection(direction);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static bool HasAnimParam(Animator animator, string paramName)
    {
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}
