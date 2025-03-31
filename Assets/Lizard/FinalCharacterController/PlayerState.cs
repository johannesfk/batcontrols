using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState {get; private set; } = PlayerMovementState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
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
