using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		PlayerInput playerInput;
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool fire;
		public bool fireHeld;
		public bool pickup;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private void Awake()
        {
            playerInput = new PlayerInput();
			playerInput.Player.Enable();
			playerInput.Player.Jump.performed += Jump;
			playerInput.Player.Sprint.performed += Sprint;
			playerInput.Player.Pickup.performed += Pickup;
        }

        private void Update()
        {
            move = playerInput.Player.Move.ReadValue<Vector2>();
			look = playerInput.Player.Look.ReadValue<Vector2>();
        }

		void Jump(InputAction.CallbackContext context)
		{
			JumpInput(context.performed);
        }

		void Sprint(InputAction.CallbackContext context)
        {
			sprint = !sprint;
		}

		void Pickup(InputAction.CallbackContext context)
        {
			pickup = !pickup;
        }
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void FireInput(bool newFireState)
        {
			fire = newFireState;
        }

		public void FireInputHeld(bool newFireState)
        {
			fireHeld = newFireState;
        }

		public void PickupInput(bool newPickupState)
        {
			pickup = newPickupState;
        }

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}
	
}