
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
    private bool wallRunning;
    private bool wallClimbing;

    private float slideTimer;
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
        if (controller == null || cameraTransform == null)
        {
            Debug.LogError("PlayerMovement: Missing references!");
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
    }

    void Update()
    {
        HandleJumpInput();

        CheckWall();

        if (jumpBufferTimer <= 0f)
            CheckWallClimb();

        HandleSlide();
        HandleSlideCamera();
        Movement();
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (controller.isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f &&
            (controller.isGrounded ||
             coyoteTimer > 0f ||
             wallClimbing ||
             wallRunning))
        {
            velocity.y = jumpForce;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            wallClimbing = false;
            wallRunning = false;
        }
    }

    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;

        if (move.magnitude > 1f)
            move.Normalize();

        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !sliding)
            currentSpeed = sprintSpeed;

        if (sliding)
            currentSpeed = slideSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (wallClimbing && velocity.y <= 0f)
        {
            velocity.y = 0f;
        }
        else if (wallRunning && !controller.isGrounded)
        {
            velocity.y = Mathf.Max(velocity.y, -wallRunFallSpeed);
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    void CheckWall()
    {
        bool wallRight = Physics.Raycast(
            transform.position, transform.right, wallDistance);

        bool wallLeft = Physics.Raycast(
            transform.position, -transform.right, wallDistance);

        bool movingForward = Input.GetAxisRaw("Vertical") > 0;

        wallRunning =
            (wallLeft || wallRight) &&
            !controller.isGrounded &&
            movingForward &&
            !wallClimbing &&
            velocity.y <= 0f;
    }

    void CheckWallClimb()
    {
        bool frontWall = Physics.Raycast(
            transform.position, transform.forward, climbDistance);

        bool leftWall = Physics.Raycast(
            transform.position, -transform.right, climbDistance);

        bool rightWall = Physics.Raycast(
            transform.position, transform.right, climbDistance);

        bool touchingWall = frontWall || leftWall || rightWall;

        if (touchingWall && Input.GetKey(climbKey))
        {
            wallClimbing = true;
            velocity.y = 0f;

            controller.Move(Vector3.up * climbSpeed * Time.deltaTime);
        }
        else
        {
            wallClimbing = false;
        }
    }

    void HandleSlide()
    {
        if (Input.GetKeyDown(slideKey) &&
            controller.isGrounded && !sliding)
        {
            StartSlide();
        }

        if (sliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f || !Input.GetKey(slideKey))
                StopSlide();
        }
    }

    void HandleSlideCamera()
    {
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraTargetPosition,
            cameraSlideSmoothness * Time.deltaTime);
    }

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;

        cameraTargetPosition =
            cameraNormalPosition + Vector3.down * slideCameraDrop;

        controller.height = normalHeight / 2f;

        controller.center = new Vector3(
            normalCenter.x,
            normalCenter.y / 2f,
            normalCenter.z);

        if (playerModel != null)
        {
            playerModel.localPosition =
                modelNormalPosition + new Vector3(0f, -0.5f, 0f);

            playerModel.localRotation = Quaternion.Euler(
                45f,
                modelNormalRotation.eulerAngles.y,
                0f);
        }
    }

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

    void OnDisable()
    {
        if (controller != null && normalHeight > 0f)
        {
            controller.height = normalHeight;
            controller.center = normalCenter;
        }

        if (cameraTransform != null)
            cameraTransform.localPosition = cameraNormalPosition;

        sliding = false;
        wallRunning = false;
        wallClimbing = false;
    }
}
