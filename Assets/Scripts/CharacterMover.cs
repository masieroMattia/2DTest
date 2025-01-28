using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class CharacterMover : MonoBehaviour
{
    public enum Direction
    {
        Left = 1,
    }

    #region Public Variables
    public InputActionAsset inputActions;
    public float walkSpeed = 2.0f;
    public float runSpeed = 4.0f;
    public float jumpForce = 5.0f;
    public float groundCheckRadius = 0.2f;

    #endregion

    #region Private Variables
    [SerializeField]
    private LayerMask groundLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private InputAction walk;
    private InputAction jump;
    private InputAction run;

    private int directionHash;
    private int isWalkingHash;
    private int isRunningHash;
    private int jumpHash;

    private Direction currentDirection;
    private bool isGrounded;
    #endregion

    #region Cycle Life
    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        InputActionMap inputActionMap = inputActions.FindActionMap("Character");
        walk = inputActionMap.FindAction("Move");
        jump = inputActionMap.FindAction("Jump");
        run = inputActionMap.FindAction("Run");

        directionHash = Animator.StringToHash("Direction");
        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
        jumpHash = Animator.StringToHash("Jump");

    }

    private void OnEnable()
    {
        walk.Enable();
        jump.Enable();
        run.Enable();

        jump.performed += OnJumpPerformed;
    }

    void OnDisable()
    {
        walk.Disable();
        jump.Disable();
        run.Disable();

        jump.performed -= OnJumpPerformed;
    }

    void Update()
    {
        Move();
        CheckGroundStatus();
    }

    #endregion

    #region Private Methods

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            animator.SetTrigger(jumpHash);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Apply jump force
        }
    }

    private void Move()
    {
        Vector2 moveInput = walk.ReadValue<Vector2>();

        float currentSpeed = run.ReadValue<float>() > 0 ? runSpeed : walkSpeed;  // Check if running

        rb.velocity = new Vector2(moveInput.x * currentSpeed, rb.velocity.y);

        bool isWalking = moveInput.magnitude > 0 && currentSpeed == walkSpeed;
        bool isRunning = moveInput.magnitude > 0 && currentSpeed == runSpeed;

        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isRunningHash, isRunning);

        if (moveInput.x > 0)
        {
            SetDirection(Direction.Left);
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0)
        {
            SetDirection(Direction.Left);
            spriteRenderer.flipX = false;
        }

    }

    private void SetDirection(Direction direction)
    {
        currentDirection = direction;
        animator.SetFloat(directionHash, (float)direction);
    }

    private void CheckGroundStatus()
    {
        // Check for ground within a radius around the player's position
        isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);
        animator.SetBool("IsGrounded", isGrounded); // Update animator with ground status
    }
    #endregion
}
