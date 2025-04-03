using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Properties
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    // Movement Settings
    [Header("Base Movement")]
    public float runAcceleration = 0.25f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 0.5f;
    public float sprintSpeed = 7f;
    public float drag = 0.1f;
    public float movingThreshold = 0.01f;
    public float gravity = -25f; // Increased gravity for faster falling
    public float jumpSpeed = 8.0f; // Increased jump force
    public float jumpCooldown = 0.2f;
    public float coyoteTime = 0.2f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f; // Horizontal camera sensitivity
    public float lookSenseV = 0.1f; // Vertical camera sensitivity
    public float lookLimitV = 89f; // Limit for vertical camera rotation

    // Input and State
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero; // To store camera rotation values
    private Vector2 _rotationVelocity = Vector2.zero; // Smooth rotation

    private float _verticalVelocity = 0f; // Vertical velocity (for like, jump and gravity shit)
    private float lastTimeGrounded; // Time since player last touched the ground
    private float _jumpCooldownCounter = 0f; // Countdown for jump cooldown

    private void Awake()
    {
        // Get references to input and state management components
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();

        if (_playerLocomotionInput == null)
        {
            Debug.LogError("PlayerLocomotionInput is missing!");
        }

        // Disable Rigidbody if present, as we are using CharacterController for physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics-based movement
            rb.useGravity = false; // Disable Rigidbody gravity
        }
    }

    private void Update()
    {
        // Update the player state and handle both vertical and lateral movement
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        // Check if the input and state components are available
        if (_playerLocomotionInput == null || _playerState == null)
        {
            Debug.LogError("Missing _playerLocomotionInput or _playerState!");
            return;
        }

          // Determine the player's movement state
        bool isMoving = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMoving;
        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            lastTimeGrounded = Time.time;
        }

        // Set the player's movement state (walking, sprinting, or idling)
        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
            isMoving ? PlayerMovementState.Walking : PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);

        // If grounded and jump is pressed, set state to jumping
        if (isGrounded && _playerLocomotionInput.JumpPressed)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        }
        else if (!isGrounded && _verticalVelocity < 0)
        {
            // If falling (not grounded and moving downward), set state to falling
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
        }
    }

    private void HandleVerticalMovement()
    {
       bool isGrounded = _characterController.isGrounded;

        // Reset vertical velocity when grounded (this keeps the player grounded)
        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Small value to keep the player grounded
        }

        // Handle jump cooldown
        if (_jumpCooldownCounter > 0)
        {
            _jumpCooldownCounter -= Time.deltaTime;
        }

        // Check if the player can jump: must be grounded, or within coyote time, and no active cooldown
        bool canJump = _playerLocomotionInput.JumpPressed && 
                       isGrounded && // Only jump if the player is grounded
                       _jumpCooldownCounter <= 0;

        // Handle the jump action
        if (canJump)
        {
            Debug.Log("Jump registered!");
            _verticalVelocity = jumpSpeed;  // Apply jump force
            _jumpCooldownCounter = jumpCooldown;  // Reset cooldown

            // After jumping, we ensure that JumpPressed can be handled only once until grounded again
            _playerLocomotionInput.ResetJumpInput();
        }

        // Apply gravity
        _verticalVelocity += gravity * Time.deltaTime;

        // Move the player vertically (falling or jumping)
        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }

    private void HandleLateralMovement()
    {
        // Determine if the player is sprinting
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        // Set acceleration and max speed based on sprinting or walking
        float acceleration = isSprinting ? sprintAcceleration : runAcceleration;
        float maxSpeed = isSprinting ? sprintSpeed : runSpeed;

        Vector3 forward = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 right = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

        Vector3 movement = Vector3.zero;
        if (_playerLocomotionInput.MovementInput != Vector2.zero)
        {
            // Combine the input to create the final movement vector
            movement = right * _playerLocomotionInput.MovementInput.x + forward * _playerLocomotionInput.MovementInput.y;
        }

        // Calculate movement delta based on input and acceleration
        Vector3 movementDelta = movement * acceleration;
        Vector3 velocity = _characterController.velocity + movementDelta;

        // Apply drag to slow down movement over time
        Vector3 currentDrag = velocity.normalized * drag * Time.deltaTime;
        velocity = (velocity.magnitude > drag * Time.deltaTime) ? velocity - currentDrag : Vector3.zero;
        
        // Clamp velocity to ensure it doesn’t exceed max speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // Set the vertical velocity to ensure proper movement in the Y axis
        velocity.y = _verticalVelocity;
        _characterController.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Handle camera rotation based on input (smooth movement)
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y -= lookSenseV * _playerLocomotionInput.LookInput.y;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -lookLimitV, lookLimitV);

        // ´Then smoothly interpolate camera rotation
        _rotationVelocity = Vector2.Lerp(_rotationVelocity, _cameraRotation, 0.05f);

        // Apply the camera and player rotation
        transform.rotation = Quaternion.Euler(0f, _rotationVelocity.x, 0f);
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationVelocity.y, 0f, 0f);
    }

    private bool IsMovingLaterally()
    {
        // Check if the player is moving laterally (ignoring vertical movement)
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        // Check if the player is on the ground, or within a small time window after leaving the ground (coyote time) coyote? awoooo.
        return _characterController.isGrounded || (Time.time - lastTimeGrounded <= coyoteTime);
    }
}
