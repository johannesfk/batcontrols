using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControllerNetwork : NetworkBehaviour
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
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();

        if (_playerLocomotionInput == null)
        {
            Debug.LogError("PlayerLocomotionInput is missing!");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Setup camera and input components only for the owner
        if (IsOwner)
        {
            // Enable camera for owner only
            if (_playerCamera != null)
            {
                _playerCamera.gameObject.SetActive(true);
            }
            
            // Enable input processing for owner only
            if (_playerLocomotionInput != null)
            {
                _playerLocomotionInput.enabled = true;
            }
        }
        else
        {
            // Disable camera for non-owners
            if (_playerCamera != null)
            {
                _playerCamera.gameObject.SetActive(false);
            }
            
            // Disable input processing for non-owners
            if (_playerLocomotionInput != null)
            {
                _playerLocomotionInput.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        if (!IsOwner) return;

        if (_playerLocomotionInput == null || _playerState == null)
        {
            Debug.LogError("Missing _playerLocomotionInput or _playerState!");
            return;
        }

        bool isMoving = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMoving;
        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            lastTimeGrounded = Time.time;
        }

        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
            isMoving ? PlayerMovementState.Walking : PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);

        if (isGrounded && _playerLocomotionInput.JumpPressed)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        }
        else if (!isGrounded && _verticalVelocity < 0)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
        }
    }

    private void HandleVerticalMovement()
    {
        if (!IsOwner) return;

        bool isGrounded = _characterController.isGrounded;

        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }

        if (_jumpCooldownCounter > 0)
        {
            _jumpCooldownCounter -= Time.deltaTime;
        }

        bool canJump = _playerLocomotionInput.JumpPressed && 
                       isGrounded && 
                       _jumpCooldownCounter <= 0;

        if (canJump)
        {
            Debug.Log("Jump registered!");
            _verticalVelocity = jumpSpeed;
            _jumpCooldownCounter = jumpCooldown;
            _playerLocomotionInput.ResetJumpInput();
        }

        _verticalVelocity += gravity * Time.deltaTime;
        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }

    private void HandleLateralMovement()
    {
        if (!IsOwner) return;

        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        float acceleration = isSprinting ? sprintAcceleration : runAcceleration;
        float maxSpeed = isSprinting ? sprintSpeed : runSpeed;

        Vector3 forward = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 right = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

        Vector3 movement = Vector3.zero;
        if (_playerLocomotionInput.MovementInput != Vector2.zero)
        {
            movement = right * _playerLocomotionInput.MovementInput.x + forward * _playerLocomotionInput.MovementInput.y;
        }

        Vector3 movementDelta = movement * acceleration;
        Vector3 velocity = _characterController.velocity + movementDelta;

        Vector3 currentDrag = velocity.normalized * drag * Time.deltaTime;
        velocity = (velocity.magnitude > drag * Time.deltaTime) ? velocity - currentDrag : Vector3.zero;
        
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        velocity.y = _verticalVelocity;
        _characterController.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y -= lookSenseV * _playerLocomotionInput.LookInput.y;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -lookLimitV, lookLimitV);

        _rotationVelocity = Vector2.Lerp(_rotationVelocity, _cameraRotation, 0.05f);

        transform.rotation = Quaternion.Euler(0f, _rotationVelocity.x, 0f);
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationVelocity.y, 0f, 0f);
    }

    private bool IsMovingLaterally()
    {
        if (!IsOwner) return false;

        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        if (!IsOwner) return false;

        return _characterController.isGrounded || (Time.time - lastTimeGrounded <= coyoteTime);
    }
}