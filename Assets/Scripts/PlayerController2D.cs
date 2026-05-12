using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runMultiplier = 1.2f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 21f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Run Stamina")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaDrainPerSecond = 1f;
    [SerializeField] private float staminaRegenPerSecond = 0.75f;
    [SerializeField] private float tiredDuration = 4f;

    [Header("Crouch")]
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private float standCheckDistance = 0.6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.28f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] bodyColliders;
    private float horizontalInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isCrouching;
    private bool isTired;
    private bool externalCrouchHeld;
    private float stamina;
    private Vector3 initialScale;
    private float jumpBufferTimer;
    private float coyoteTimer;

    public bool IsCrouching => isCrouching;
    public bool IsTired => isTired;
    public float Stamina01 => Mathf.Clamp01(stamina / maxStamina);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyColliders = GetComponents<Collider2D>();
        initialScale = transform.localScale;
        stamina = maxStamina;
        ConfigureLowFriction();
        SetColliderHeight(normalHeight);
    }

    private void Update()
    {
        horizontalInput = GetHorizontalInput();
        if (GetJumpPressed())
        {
            jumpPressed = true;
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        HandleCrouchInput();
        HandleStamina();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }

        Move();
        Jump();
    }

    private void HandleCrouchInput()
    {
        if (GetCrouchHeld())
        {
            isCrouching = true;
            SetColliderHeight(crouchHeight);
            return;
        }

        if (isCrouching && !CanStand())
        {
            return;
        }

        isCrouching = false;
        SetColliderHeight(normalHeight);
    }

    private void HandleStamina()
    {
        if (isTired)
        {
            return;
        }

        bool wantsRun = GetRunHeld();
        bool canRun = wantsRun && !isTired && Mathf.Abs(horizontalInput) > 0.01f;

        if (canRun)
        {
            stamina -= staminaDrainPerSecond * Time.deltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                StartCoroutine(TiredRoutine());
            }
            return;
        }

        if (!isTired)
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenPerSecond * Time.deltaTime);
        }
    }

    private void Move()
    {
        float speed = walkSpeed;
        bool wantsRun = GetRunHeld();

        if (wantsRun && !isTired)
        {
            speed *= runMultiplier;
        }

        if (isCrouching)
        {
            speed *= crouchSpeedMultiplier;
        }

        if (isTired)
        {
            speed = Mathf.Min(speed, walkSpeed);
        }

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (!jumpPressed && jumpBufferTimer <= 0f)
        {
            return;
        }

        if (coyoteTimer <= 0f)
        {
            return;
        }

        jumpPressed = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private bool CanStand()
    {
        Vector2 origin = capsule.bounds.center;
        float distance = standCheckDistance;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.up, distance, ceilingMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform == transform)
            {
                continue;
            }

            if (hit.collider.isTrigger)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private IEnumerator TiredRoutine()
    {
        if (isTired)
        {
            yield break;
        }

        isTired = true;
        stamina = 0f;
        yield return new WaitForSeconds(tiredDuration);
        isTired = false;
        stamina = maxStamina;
    }

    private void SetColliderHeight(float targetHeight)
    {
        Vector2 size = capsule.size;
        size.y = targetHeight;
        capsule.size = size;

        float crouchVisualScale = targetHeight <= crouchHeight + 0.01f ? 0.5f : 1f;
        transform.localScale = new Vector3(initialScale.x, initialScale.y * crouchVisualScale, initialScale.z);

        if (spriteRenderer != null)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, targetHeight);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void ConfigureGroundCheck(Transform targetGroundCheck, LayerMask groundLayerMask, LayerMask ceilingLayerMask)
    {
        groundCheck = targetGroundCheck;
        groundMask = groundLayerMask;
        ceilingMask = ceilingLayerMask;
    }

    private float GetHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float value = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                value -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                value += 1f;
            }

            return Mathf.Clamp(value, -1f, 1f);
        }
#endif
        return Input.GetAxisRaw("Horizontal");
    }

    private bool GetJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private bool GetRunHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }

    private bool GetCrouchHeld()
    {
        if (externalCrouchHeld)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
#else
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
    }

    private bool CheckGrounded()
    {
        Vector2 checkPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 1.05f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, groundCheckRadius, groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == transform)
            {
                continue;
            }

            if (hit.isTrigger)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void ConfigureLowFriction()
    {
        PhysicsMaterial2D slip = new PhysicsMaterial2D("PlayerSlip")
        {
            friction = 0f,
            bounciness = 0f
        };

        if (bodyColliders == null)
        {
            return;
        }

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            if (bodyColliders[i] != null)
            {
                bodyColliders[i].sharedMaterial = slip;
            }
        }
    }

    public void ExternalJumpPress()
    {
        jumpPressed = true;
        jumpBufferTimer = jumpBufferTime;
    }

    public void ExternalCartJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpPressed = false;
    }

    public void SetExternalCrouch(bool isHeld)
    {
        externalCrouchHeld = isHeld;
    }
}
