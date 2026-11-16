using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement & Jump")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float defaultGravity = 3f;

    [Header("Ground Check (Anti-Slide)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Interactions")]
    [Tooltip("Seconds the interact action locks movement")]
    public float interactDuration = 0.4f;
    [Tooltip("Seconds into the action when the event fires")]
    public float interactFireTime = 0.15f;
    public UnityEvent onInteract;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference interactAction;

    [Header("Event Channels")]
    [SerializeField] private VoidEventChannelSO levelStartedChannel;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float moveInput;
    private float lockTimer;
    private bool isGrounded;
    private bool canControl = false; // נפתח רק אחרי בחירה נכונה במפת רומא

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); // אופציונלי - לא יזרוק שגיאה אם אין Animator

        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (interactAction != null) interactAction.action.Enable();

        if (levelStartedChannel != null)
            levelStartedChannel.OnEventRaised += EnableControl;
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (interactAction != null) interactAction.action.Disable();

        if (levelStartedChannel != null)
            levelStartedChannel.OnEventRaised -= EnableControl;
    }

    private void EnableControl()
    {
        canControl = true;
    }

    private void Update()
    {
        if (!canControl) return;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (lockTimer > 0f)
        {
            lockTimer -= Time.deltaTime;
            moveInput = 0f;
        }
        else
        {
            // קריאת תנועה אופקית בלבד
            moveInput = (moveAction != null && moveAction.action != null)
                ? moveAction.action.ReadValue<Vector2>().x
                : 0f;

            if (moveInput > 0.01f) sr.flipX = false;
            else if (moveInput < -0.01f) sr.flipX = true;

            // קפיצה
            if (jumpAction != null && jumpAction.action != null && jumpAction.action.WasPressedThisFrame() && isGrounded)
            {
                rb.gravityScale = defaultGravity;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }

            // אינטראקציה (למשל סיבוב המראה של המלאך)
            if (interactAction != null && interactAction.action != null && interactAction.action.WasPressedThisFrame())
            {
                StartInteract();
            }
        }

        if (anim != null && anim.runtimeAnimatorController != null)
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    private void FixedUpdate()
    {
        if (!canControl) return;

        // מניעת החלקה במדרגות כשעוזבים את המקשים על הקרקע
        if (isGrounded && Mathf.Abs(moveInput) < 0.01f && rb.linearVelocity.y <= 0.1f)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    public void StartInteract()
    {
        lockTimer = interactDuration;

        if (anim != null && anim.runtimeAnimatorController != null)
            anim.SetTrigger("Interact");

        Invoke(nameof(FireInteract), interactFireTime);
    }

    private void FireInteract()
    {
        onInteract?.Invoke();
    }
}