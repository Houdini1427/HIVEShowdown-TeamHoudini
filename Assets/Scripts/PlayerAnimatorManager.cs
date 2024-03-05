using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimatorManager : MonoBehaviourPun
    {
        #region Private Fields

        private PlayerInput _input;

        [SerializeField]
        private float RunSpeed = 0.2f;

        [SerializeField]
        private float WalkSpeed = 0.1f;


        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _targetRotation = 0.0f;
        private float _terminalVelocity = 53.0f;


        private Animator animator;
        private GameObject _mainCamera;
        private CharacterController _controller;


        private const float PunchTimeout = 0.2f;
        private float _punchTimeout;
        private int lastPunchInput = 0;
        private const float KickTimeout = 0.2f;
        private float _kickTimeout;
        private float _fallTimeout;
        private float _jumpTimeout;
        private const float JumpTimeout = 1f;
        private float _rollTimeout;
        private const float RollTimeout = 0.2f;


        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        #endregion

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = 0.1f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.7f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        #region MonoBehaviour Callbacks

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        // Use this for initialization
        void Start()
        {
            animator = this.gameObject.GetComponent<Animator>();
            _controller = this.gameObject.GetComponent<CharacterController>();
            _input = this.gameObject.GetComponent<PlayerInput>();
            
            if (!animator)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
            }
            _punchTimeout = PunchTimeout;
            _kickTimeout = KickTimeout;
            _jumpTimeout = JumpTimeout;
            _rollTimeout = RollTimeout;
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("In the Animator");
            Debug.Log("Input: "+_input.sprint);
            Debug.Log("Input: "+_input.move);
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }
            if (!animator)
            {
                return;
            }
            Debug.Log("Only Photon View");
            Debug.Log("Input: "+_input.sprint);
            Debug.Log("Input: "+_input.move);


            // AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // only allow jumping if we are running.
            // if (stateInfo.IsName("Base Layer.Run"))
            // {
            //     // When using trigger parameter
            //     if (_input.jump)
            //     {
            //         Debug.Log("Jump triggered");
            //         animator.SetTrigger("Jump");
            //         if (_input.jump && _jumpTimeout >= 0)
            //         {
            //             _jumpTimeout -= Time.deltaTime;
            //         }
            //         else
            //         {
            //             _jumpTimeout = JumpTimeout;
            //             _input.jump = false;
            //         }
            //     }
            // }

            Move();
            JumpAndGround();
            GroundedCheck();
            Attack();
            Defense();
            // float h = _input.move.x;
            // float v = _input.move.y;
            // if (v < 0)
            // {
            //     v = 0;
            //     animator.SetTrigger("Dodge");
            // }
            // animator.SetFloat("Speed", h * h + v * v);
            // animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);

            // Debug.Log("Punch: " + _input.punch);
            
        }

        private void Defense()
        {
            animator.SetBool("Roll", _input.roll);
            if (_input.roll && _rollTimeout >= 0)
            {
                _rollTimeout -= Time.deltaTime;
            }
            else if (_input.roll)
            {
                _rollTimeout = RollTimeout;
                _input.roll = false;
            }
        }

        private void Attack()
        {
            if (_input.punch != lastPunchInput)
            {
                animator.SetInteger("Punch", _input.punch);
                lastPunchInput = _input.punch;
            }
            if (_input.punch != 0 && _punchTimeout >= 0 && lastPunchInput == _input.punch)
            {
                lastPunchInput = _input.punch;
                _punchTimeout -= Time.deltaTime;
            }
            else if (_input.punch != 0 && _punchTimeout >= 0)
            {
                _punchTimeout = PunchTimeout;
                lastPunchInput = _input.punch;
            }
            else if (_input.punch != 0)
            {
                _punchTimeout = PunchTimeout;
                _input.punch = 0;
            }

            animator.SetBool("Kick", _input.kick);
            if (_input.kick && _kickTimeout >= 0)
            {
                _kickTimeout -= Time.deltaTime;
            }
            else if (_input.kick){
                _kickTimeout = KickTimeout;
                _input.kick = false;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.UseGlobal);

            // Debug.Log("Sphere: " + spherePosition.ToString());

            animator.SetBool("Grounded", Grounded);
        }
        private void JumpAndGround()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeout = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeout <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    animator.SetBool("Jump", true);
                }

                // jump timeout
                if (_jumpTimeout >= 0.0f)
                {
                    _jumpTimeout -= Time.deltaTime;
                }

            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeout = JumpTimeout;

                // fall timeout
                if (_fallTimeout >= 0.0f)
                {
                    _fallTimeout -= Time.deltaTime;
                }
                else
                {
                    animator.SetBool("Grounded", false);
                }

                // if we are not grounded, do not jump
                _input.jump = false;
                animator.SetBool("Jump", false);
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
        private void Move()
        {
            float targetSpeed = _input.sprint ? RunSpeed : WalkSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;

            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (animator != null)
            {
                animator.SetFloat("Speed", _animationBlend);
            }

        }

        private bool setTimeout(float _timeoutDelta, float Timeout)
        {
            if (_timeoutDelta >= 0)
            {
                _timeoutDelta -= Time.deltaTime;
            }
            else
            {
                _timeoutDelta = Timeout;
                return true;
            }
            return false;
        }

        #endregion
    }
}
