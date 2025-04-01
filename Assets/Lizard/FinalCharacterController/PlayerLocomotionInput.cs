using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    // Properties
    [SerializeField] private bool holdToSprint = true;
   
    public bool SprintToggledOn { get; private set; }
   
    private PlayerControls _playerControls; 
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    private void OnEnable()
    {
        if (_playerControls == null)
        {
            _playerControls = new PlayerControls(); // Instantiate the input system
        }

        _playerControls.Enable();
        _playerControls.PlayerLocomotionMap.Enable();
        _playerControls.PlayerLocomotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        _playerControls.PlayerLocomotionMap.Disable();
        _playerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context) 
    {
        MovementInput = context.ReadValue<Vector2>();
        Debug.Log("Movement Input:"+ MovementInput);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    // other actions

    public void OnSprintToggle(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                SprintToggledOn = holdToSprint || !SprintToggledOn;
            }
            else if(context.canceled)
            {
                SprintToggledOn = !holdToSprint && SprintToggledOn;
            }
        }





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

        public void OnJump(InputAction.CallbackContext context)
        {
        
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        
        }
}
