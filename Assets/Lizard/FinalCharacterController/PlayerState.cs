using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; set; } = PlayerMovementState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState()
    {   
    return CurrentPlayerMovementState == PlayerMovementState.Idling ||
           CurrentPlayerMovementState == PlayerMovementState.Walking ||
           CurrentPlayerMovementState == PlayerMovementState.Sprinting;
    }   
}

public enum PlayerMovementState
    {
        Idling = 0,
        Walking = 1,
        Sprinting = 2,
        Jumping = 3,
        Falling = 4,
        
    }
