using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    // Properties
    [SerializeField] private bool holdToSprint = true; // Determines whether sprinting is toggleable or requires holding a button
   
    public bool SprintToggledOn { get; private set; } // Indicates if sprinting is currently active
   
    private PlayerControls _playerControls; // Hold PlayerControls input (like, the input system)
    public Vector2 MovementInput { get; private set; } // Stores movement input (WASD,joystick, arrows etc)
    public Vector2 LookInput { get; private set; } // Stores camera look input
    public bool JumpPressed { get; private set; } // Indicates if the lil jump button is pressed


    // Called when the script is enabled
    private void OnEnable()
    {
         // If the _playerControls is null, instantiate a new PlayerControls object (input system setup)
        if (_playerControls == null)
        {
            _playerControls = new PlayerControls(); // Instantiate the input system
        }

        // Enable the input system and bind the callbacks for the PlayerLocomotionMap actions
        _playerControls.Enable();
        _playerControls.PlayerLocomotionMap.Enable();
        _playerControls.PlayerLocomotionMap.SetCallbacks(this); // Register this script as the callback handler
    }

    // Called when the script is disabled
    private void OnDisable()
    {
        // Disable the input system and remove the callbacks to prevent memory leaks! No crying over spilt milk (or, spilt memory hehe)
        _playerControls.PlayerLocomotionMap.Disable();
        _playerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    
    // Called every frame, after Update
    private void LateUpdate()
    {
        // Reset the JumpPressed flag to false after it's been used (prevents jumping multiple times mid-air)
        if (JumpPressed)
        {
            JumpPressed = false;
        }
    }
    
    // This method resets the jump input flag after a jump has been performed (used by PlayerController)
    public void ResetJumpInput()
    {
        // This function will be called after the jump is used to reset the jump flag.
        JumpPressed = false;
    }

    // Called when the movement input (e.g., WASD, joystick) is updated
    public void OnMove(InputAction.CallbackContext context) 
    {
        // Read the movement input from the context and store it
        MovementInput = context.ReadValue<Vector2>();
        Debug.Log("Movement Input:"+ MovementInput);
    }

    // Called when the look input (e.g., mouse movement, right joystick) is updated
    public void OnLook(InputAction.CallbackContext context)
    {
        // Read the look input (camera movement) from the context and store it.
        LookInput = context.ReadValue<Vector2>();
    }

    // Called when the sprint toggle input is triggered (e.g., pressing shift)
    public void OnSprintToggle(InputAction.CallbackContext context)
    {
        // Check if the sprint input action was performed or canceled
        if(context.performed)
        {
            // Toggle sprint based on the 'holdToSprint' flag or toggle it otherwise
            SprintToggledOn = holdToSprint || !SprintToggledOn;
        }
        else if(context.canceled)
        {
            // If sprint input was canceled, toggle sprint based on the 'holdToSprint' flag
            SprintToggledOn = !holdToSprint && SprintToggledOn;
        }
    }

    // Called when the jump input is pressed
    public void OnJump(InputAction.CallbackContext context)
    {
        // If the jump button was pressed (context.performed means button press or action trigger)
        if (!context.performed)
                return;
        
        // Set the JumpPressed flag to true (indicating a jump request)
        JumpPressed = true;      
    }


   // other actions

        public void OnSonar(InputAction.CallbackContext context)
        {
        
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
        
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
        
        }

      

        public void OnPrevious(InputAction.CallbackContext context)
        {
        
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        
        }
}
