
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraTransform;
    public Transform playerModel;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 10f;
    public float gravity = -30f;

    [Header("Jump")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 150f;
    public float maxLookAngle = 90f;
    public float mouseSmoothness = 15f;

    private float xRotation;
    private float targetMouseX;
    private float targetMouseY;
    private float smoothMouseX;
    private float smoothMouseY;

    [Header("Slide")]
    public KeyCode slideKey = KeyCode.LeftControl;
    public float slideSpeed = 12f;
    public float slideDuration = 0.8f;

    [Header("Slide Camera")]
    public float slideCameraDrop = 0.5f;
    public float cameraSlideSmoothness = 10f;

    [Header("Wall Run")]
    public float wallDistance = 2f;
    public float wallRunFallSpeed = 2f;

    [Header("Wall Climb")]
    public KeyCode climbKey = KeyCode.E;
    public float climbSpeed = 5f;
    public float climbDistance = 2f;

    private Vector3 velocity;

    private bool sliding;
    private float slideTimer;

    private bool wallRunning;
    private bool wallClimbing;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private float normalHeight;
    private Vector3 normalCenter;

    private Vector3 modelNormalPosition;
    private Quaternion modelNormalRotation;

    // Camera position variables
    private Vector3 cameraNormalPosition;
    private Vector3 cameraTargetPosition;

    void Start()
    {
        // Save normal Character Controller settings
        normalHeight = controller.height;
        normalCenter = controller.center;

        // Save model position and rotation
        if (playerModel != null)
        {
            modelNormalPosition = playerModel.localPosition;
            modelNormalRotation = playerModel.localRotation;
        }

        // Save normal camera position
        if (cameraTransform != null)
        {
            cameraNormalPosition = cameraTransform.localPosition;
            cameraTargetPosition = cameraNormalPosition;
        }

        // Lock mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLook();

        HandleJumpInput();

        CheckWall();
        CheckWallClimb();

        HandleSlide();
        HandleSlideCamera();

        Movement();
    }

    // =========================================================
    // MOUSE LOOK
    // =========================================================

    void MouseLook()
    {
        float mouseX =
            Input.GetAxisRaw("Mouse X") *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            Input.GetAxisRaw("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        targetMouseX = mouseX;
        targetMouseY = mouseY;

        // Smooth horizontal movement
        smoothMouseX = Mathf.Lerp(
            smoothMouseX,
            targetMouseX,
            mouseSmoothness * Time.deltaTime
        );

        // Smooth vertical movement
        smoothMouseY = Mathf.Lerp(
            smoothMouseY,
            targetMouseY,
            mouseSmoothness * Time.deltaTime
        );

        // Vertical camera rotation
        xRotation -= smoothMouseY;

        xRotation = Mathf.Clamp(
            xRotation,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal player rotation
        transform.Rotate(Vector3.up * smoothMouseX);
    }

    // =========================================================
    // JUMP INPUT
    // =========================================================

    void HandleJumpInput()
    {
        // Jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // Coyote time
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        float currentSpeed = walkSpeed;

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift) && !sliding)
        {
            currentSpeed = sprintSpeed;
        }

        // Slide speed
        if (sliding)
        {
            currentSpeed = slideSpeed;
        }

        // Prevent faster diagonal movement
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        // Horizontal movement
        controller.Move(
            move * currentSpeed * Time.deltaTime
        );

        // Ground
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Jump
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = jumpForce;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            // Cancel wall climbing
            wallClimbing = false;
        }

        // Vertical movement
        if (wallClimbing)
        {
            velocity.y = 0f;
        }
        else if (wallRunning && !controller.isGrounded)
        {
            velocity.y = -wallRunFallSpeed;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply vertical movement
        controller.Move(
            Vector3.up * velocity.y * Time.deltaTime
        );
    }

    // =========================================================
    // WALL RUN
    // =========================================================

    void CheckWall()
    {
        bool wallRight = Physics.Raycast(
            transform.position,
            transform.right,
            wallDistance
        );

        bool wallLeft = Physics.Raycast(
            transform.position,
            -transform.right,
            wallDistance
        );

        bool movingForward =
            Input.GetAxisRaw("Vertical") > 0;

        wallRunning =
            (wallLeft || wallRight) &&
            !controller.isGrounded &&
            movingForward &&
            !wallClimbing;
    }

    // =========================================================
    // WALL CLIMB
    // =========================================================

    void CheckWallClimb()
    {
        // Check in front
        bool frontWall = Physics.Raycast(
            transform.position,
            transform.forward,
            climbDistance
        );

        // Check left
        bool leftWall = Physics.Raycast(
            transform.position,
            -transform.right,
            climbDistance
        );

        // Check right
        bool rightWall = Physics.Raycast(
            transform.position,
            transform.right,
            climbDistance
        );

        bool touchingWall =
            frontWall || leftWall || rightWall;

        // Hold E near a wall to climb
        if (touchingWall && Input.GetKey(climbKey))
        {
            wallClimbing = true;

            // Stop falling
            velocity.y = 0f;

            // Move upward
            controller.Move(
                Vector3.up * climbSpeed * Time.deltaTime
            );
        }
        else
        {
            wallClimbing = false;
        }
    }

    // =========================================================
    // SLIDE
    // =========================================================

    void HandleSlide()
    {
        // Start slide
        if (Input.GetKeyDown(slideKey) &&
            controller.isGrounded &&
            !sliding)
        {
            StartSlide();
        }

        // Continue slide
        if (sliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f ||
                !Input.GetKey(slideKey))
            {
                StopSlide();
            }
        }
    }

    // =========================================================
    // SLIDE CAMERA
    // =========================================================

    void HandleSlideCamera()
    {
        if (cameraTransform == null)
            return;

        // Smoothly lower and raise the camera
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraTargetPosition,
            cameraSlideSmoothness * Time.deltaTime
        );
    }

    // =========================================================
    // START SLIDE
    // =========================================================

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;

        // Lower camera smoothly
        cameraTargetPosition =
            cameraNormalPosition +
            Vector3.down * slideCameraDrop;

        // Make Character Controller shorter
        controller.height = normalHeight / 2f;

        controller.center = new Vector3(
            normalCenter.x,
            normalCenter.y / 2f,
            normalCenter.z
        );

        // Move model down and rotate it
        if (playerModel != null)
        {
            playerModel.localPosition =
                modelNormalPosition +
                new Vector3(0f, -0.5f, 0f);

            playerModel.localRotation =
                Quaternion.Euler(
                    45f,
                    modelNormalRotation.eulerAngles.y,
                    0f
                );
        }
    }

    // =========================================================
    // STOP SLIDE
    // =========================================================

    void StopSlide()
    {
        sliding = false;

        // Return camera smoothly to normal height
        cameraTargetPosition = cameraNormalPosition;

        // Restore Character Controller
        controller.height = normalHeight;
        controller.center = normalCenter;

        // Restore model
        if (playerModel != null)
        {
            playerModel.localPosition = modelNormalPosition;
            playerModel.localRotation = modelNormalRotation;
        }
    }

    // =========================================================
    // DEBUG WALL RAYS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        // Climb detection
        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * climbDistance
        );

        // Wall run detection
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            transform.right * wallDistance
        );

        Gizmos.DrawRay(
            transform.position,
            -transform.right * wallDistance
        );
    }
}
