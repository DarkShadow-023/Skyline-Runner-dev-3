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

    [Header("Slide")]
    public KeyCode slideKey = KeyCode.LeftControl;
    public float slideSpeed = 12f;
    public float slideDuration = 0.8f;

    [Header("Wall Run")]
    public float wallDistance = 2f;
    public float wallRunFallSpeed = 2f;

    [Header("Wall Climb")]
    public KeyCode climbKey = KeyCode.E;
    public float climbSpeed = 5f;
    public float climbDistance = 2f;

    private Vector3 velocity;

    private float xRotation;

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

    void Start()
    {
        normalHeight = controller.height;
        normalCenter = controller.center;

        if (playerModel != null)
        {
            modelNormalPosition = playerModel.localPosition;
            modelNormalRotation = playerModel.localRotation;
        }

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

        Movement();
    }

    // =========================================================
    // MOUSE LOOK
    // =========================================================

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") *
                       mouseSensitivity *
                       Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       mouseSensitivity *
                       Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(
            xRotation,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // =========================================================
    // JUMP INPUT
    // =========================================================

    void HandleJumpInput()
    {
        // Remember jump input for a short time
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

        // Slide
        if (sliding)
        {
            currentSpeed = slideSpeed;
        }

        // Horizontal movement
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(
            move *
            currentSpeed *
            Time.deltaTime
        );

        // =====================================================
        // GROUND
        // =====================================================

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // =====================================================
        // JUMP
        // =====================================================

        if (jumpBufferTimer > 0f &&
            coyoteTimer > 0f)
        {
            velocity.y = jumpForce;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // =====================================================
        // VERTICAL MOVEMENT
        // =====================================================

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

        // Vertical movement
        controller.Move(
            Vector3.up *
            velocity.y *
            Time.deltaTime
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
        bool frontWall = Physics.Raycast(
            transform.position,
            transform.forward,
            climbDistance
        );

        bool leftWall = Physics.Raycast(
            transform.position,
            -transform.right,
            climbDistance
        );

        bool rightWall = Physics.Raycast(
            transform.position,
            transform.right,
            climbDistance
        );

        bool touchingWall =
            frontWall ||
            leftWall ||
            rightWall;

        if (touchingWall &&
            Input.GetKey(climbKey) &&
            !controller.isGrounded)
        {
            wallClimbing = true;

            velocity.y = 0f;

            controller.Move(
                Vector3.up *
                climbSpeed *
                Time.deltaTime
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
        if (Input.GetKeyDown(slideKey) &&
            controller.isGrounded &&
            !sliding)
        {
            StartSlide();
        }

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
    // START SLIDE
    // =========================================================

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;

        controller.height =
            normalHeight / 2f;

        controller.center =
            new Vector3(
                normalCenter.x,
                normalCenter.y / 2f,
                normalCenter.z
            );

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

        controller.height = normalHeight;
        controller.center = normalCenter;

        if (playerModel != null)
        {
            playerModel.localPosition =
                modelNormalPosition;

            playerModel.localRotation =
                modelNormalRotation;
        }
    }

    // =========================================================
    // DEBUG WALL RAYS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * climbDistance
        );

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