using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    // Properties
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

        public void OnSonar(InputAction.CallbackContext context)
        {
        
        }

        public void OnSprint(InputAction.CallbackContext context)
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
