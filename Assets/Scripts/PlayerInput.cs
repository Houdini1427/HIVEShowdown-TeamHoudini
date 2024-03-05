using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Com.MyCompany.MyGame
{
 
    public class PlayerInput : MonoBehaviour
    {
        // Start is called before the first frame update
        [Header("Character Input Values")]
		public Vector2 move;
		public bool jump;
		public bool sprint;
        public int punch;
        public bool kick;
        public bool roll;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnPunch1(InputValue value)
        {
            if (value.isPressed)
            {
                PunchInput(1);
                Debug.Log("Punch Value Set: 1");
            }
            else
            {
                PunchInput(0);
                Debug.Log("Punch Value Set: 0");
            }
        }

        public void OnPunch2(InputValue value)
        {
            if (value.isPressed)
            {
                PunchInput(2);
                Debug.Log("Punch Value Set: 2");
            }
            else
            {
                PunchInput(0);
                Debug.Log("Punch Value Set: 0");
            }
        }

        public void OnKick(InputValue value)
        {
            KickInput(value.isPressed);
        }

        public void OnRoll(InputValue value)
        {
            RollInput(value.isPressed);
        }
#endif

        public void RollInput(bool newRollState)
        {
            roll = newRollState;
        }

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

        public void PunchInput(int newPunch)
        {
            punch = newPunch;
        }

        public void KickInput(bool newKick)
        {
            kick = newKick;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
    }

}
