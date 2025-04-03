using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   // Properties
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    // Variables to mess around with
    [Header("Base Movement")]
    public float runAcceleration = 0.25f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 0.5f;
    public float sprintSpeed = 7f;
    public float drag = 0.1f;
    public float movingThreshold = 0.01f;
    public float gravity = -25f; // Gravity Force
    public float jumpSpeed = 1.0f;
    public float jumpCooldown = 0.1f; // Small delay before jumping again
    public float coyoteTime = 0.2f; // Allow jumping for 0.2 seconds after falling
   

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    // Instance to inputs
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    public float rotationSmoothTime = 0.05f; // Smoothing time
    private Vector2 _rotationVelocity = Vector2.zero;


    private float _verticalVelocity = 0f;   
    private float lastTimeGrounded;
    private float _jumpCooldownCounter = 0f;


    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        if (_playerLocomotionInput == null)
        {
            Debug.LogError("PlayerLocomotionInput is not attached to this Gameobjectt");
        }

        // Disable Rigidbody if attached to prevent conflicting physics calculations. meow.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  // Disable physics interaction
            rb.useGravity = false;  // Disable gravity on Rigidbody
        }
    }

    private void Update()
    {
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    // --- Updating the state of the Movement ---
    private void UpdateMovementState()
    {    
        if (_playerLocomotionInput == null || _playerState == null)
        {
            Debug.LogError("Missing: _playerLocomotionInput or _playerState is nulll");
            return;
        }

        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;    // order
        bool isMovingLaterally = IsMovingLaterally();                                   // matter
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && IsMovingLaterally(); // order matters
        bool isGrounded = IsGrounded();

        // Track the last time player was grounded for coyote time (essentially we're adding a buffer here)
        if (isGrounded)
        {
            lastTimeGrounded = Time.time;
        }

        // Now isMovingLaterally is a boolean, and can be used in the condition
        // Determines lateral movement state!
        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                    isMovingLaterally || isMovementInput ? PlayerMovementState.Walking : PlayerMovementState.Idling;
       

        _playerState.SetPlayerMovementState(lateralState);

        // Control Airbon State
        if (_playerLocomotionInput.JumpPressed && isGrounded)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        }
        else if (!isGrounded && _characterController.velocity.y < 0f)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
        }
    }


    // --- Handling the vertical movement on the controller ---
    private void HandleVerticalMovement()
    {
        bool isGrounded = _playerState.InGroundedState();

        // Reset velocity when grounded
        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = 0f;
        }

        // Reduce jump cooldown timer
        if (_jumpCooldownCounter > 0)
        {
            _jumpCooldownCounter -= Time.deltaTime;
        }

        _verticalVelocity += gravity * Time.deltaTime;

        // Jump logic - Now considers coyote time and jump cooldown
        bool canJump = _playerLocomotionInput.JumpPressed &&
                       (isGrounded || (Time.time - lastTimeGrounded <= coyoteTime)) &&
                       _jumpCooldownCounter <= 0;

        if (canJump)
        {
            // Jump immediately when pressed
            _verticalVelocity = Mathf.Sqrt(jumpSpeed * -2.0f * gravity);
            _jumpCooldownCounter = jumpCooldown; // Apply cooldown
        }
    }

    private void HandleLateralMovement()
    {
            // Create quick reference for current state
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.InGroundedState();

        // State dependent acceleration and speed
        float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;

        // Use input-based movement even if camera isn't rotating
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

        // If there's movement input, use it to determine direction
        Vector3 MovementDirection = Vector3.zero;
        if (_playerLocomotionInput.MovementInput != Vector2.zero)
        {
            MovementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;
        }

        // Apply movement input direction to velocity
        Vector3 movementDelta = MovementDirection * lateralAcceleration;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player - prevents continuous movement in direction / no stopping
        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;

        // Apply gravity to vertical velocity (Y-axis)
        if (!_characterController.isGrounded)
        {
            _verticalVelocity += gravity * Time.deltaTime; // Apply gravity when not grounded
        }
        else
        {
            _verticalVelocity = 0f; // Reset vertical velocity when grounded
        }

        // Add vertical velocity to the movement (Y-axis)
        newVelocity.y = _verticalVelocity;

        // Move character
        _characterController.Move(newVelocity * Time.deltaTime);
    }


    private void LateUpdate()
    {
        // Get input from mouse
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y -= lookSenseV * _playerLocomotionInput.LookInput.y;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -lookLimitV, lookLimitV);

        // Making the Smooth rotation
        _rotationVelocity = Vector2.Lerp(_rotationVelocity, _cameraRotation, rotationSmoothTime);
    
        // Rotate player (left/right)
        transform.rotation = Quaternion.Euler(0f, _rotationVelocity.x, 0f);

        // Rotate camera (up/down)
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationVelocity.y, 0f, 0f);
       
    }

    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }
}