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
    public float drag = 0.1f;

    [Header("Gravity & Vertical Velocity")]
    private float _verticalVelocity = 0f;
    private float gravity = -9.81f; // Gravity Force
    private float jumpHeight = 1.5f; // Jump height

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    // Instance to inputs
    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;


    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();

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
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 MovementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;
        Debug.Log("Movement Input: " + _playerLocomotionInput.MovementInput);


        Vector3 movementDelta = MovementDirection * runAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player - prevents continous movement in direction / no stopping
        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, runSpeed);

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


        // Move character (Unity suggests only calling this once per tick, or shit get wonky) 
        _characterController.Move(newVelocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Handle camera rotation
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        // Rotate the polayer (on the y-axis only)
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;
        transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);

        // Update camera rotation
        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
    }
}