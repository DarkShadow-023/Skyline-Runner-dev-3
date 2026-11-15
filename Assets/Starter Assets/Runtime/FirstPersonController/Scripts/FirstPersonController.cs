
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
    public float maxLookAngle = 89f;
    public float mouseSmoothness = 15f;

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

    private Vector3 cameraNormalPosition;
    private Vector3 cameraTargetPosition;

    void Start()
    {
        if (controller == null)
        {
            Debug.LogError("PlayerMovement: CharacterController is missing!");
            enabled = false;
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("PlayerMovement: Camera Transform is missing!");
            enabled = false;
            return;
        }

        normalHeight = controller.height;
        normalCenter = controller.center;

        if (playerModel != null)
        {
            modelNormalPosition = playerModel.localPosition;
            modelNormalRotation = playerModel.localRotation;
        }

        cameraNormalPosition = cameraTransform.localPosition;
        cameraTargetPosition = cameraNormalPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleJumpInput();

        CheckWall();
        CheckWallClimb();

        HandleSlide();
        HandleSlideCamera();

        Movement();
    }

    // JUMP INPUT

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    // MOVEMENT

    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Get horizontal camera directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Prevent looking up/down from affecting movement
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Move relative to camera direction
        Vector3 move = forward * z + right * x;

        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !sliding)
        {
            currentSpeed = sprintSpeed;
        }

        if (sliding)
        {
            currentSpeed = slideSpeed;
        }

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

        controller.Move(
            Vector3.up * velocity.y * Time.deltaTime
        );
    }

    // WALL RUN

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

    // WALL CLIMB

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
            frontWall || leftWall || rightWall;

        if (touchingWall && Input.GetKey(climbKey))
        {
            wallClimbing = true;

            velocity.y = 0f;

            controller.Move(
                Vector3.up * climbSpeed * Time.deltaTime
            );
        }
        else
        {
            wallClimbing = false;
        }
    }

    // SLIDE

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

    // SLIDE CAMERA

    void HandleSlideCamera()
    {
        if (cameraTransform == null)
            return;

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraTargetPosition,
            cameraSlideSmoothness * Time.deltaTime
        );
    }

    // START SLIDE

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;

        cameraTargetPosition =
            cameraNormalPosition +
            Vector3.down * slideCameraDrop;

        controller.height = normalHeight / 2f;

        controller.center = new Vector3(
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

    // STOP SLIDE

    void StopSlide()
    {
        sliding = false;

        cameraTargetPosition = cameraNormalPosition;

        controller.height = normalHeight;
        controller.center = normalCenter;

        if (playerModel != null)
        {
            playerModel.localPosition = modelNormalPosition;
            playerModel.localRotation = modelNormalRotation;
        }
    }

    // DEBUG WALL RAYS

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
