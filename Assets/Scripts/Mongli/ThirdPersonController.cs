using System;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
    {
        public MongliAnimatorNetworkController _animatorController;
        
        //public PlayerNetwork playerNetwork;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Tooltip("slide force")]
        public float frictionForce = 10f;

        [Tooltip("slide force")]
        public float slideForce = 30f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        public bool slide = false;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;        

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;        

        private Vector3 moveInput;
        //public Transform playerCamera;

        // player
        private float _speed;
        private float targetSpeed;
        private float _animationBlend;
        [Range(0f,1f)]
        public float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;       

        private Vector3 targetLookAt;
        private Vector3 inputDirection;

        public float maxInclineAngle = 35f;

        public CharacterController _controller;     
               

        private void Start()       
        {       
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        public void UpdatePlayer(Vector3 inputMove, bool inputJump, float deltaTime)
        {      
            JumpAndGravity(inputJump, deltaTime);
            GroundedCheck();
            //Deslizamiento(deltaTime);
            Move(inputMove, deltaTime);            
        }    

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_animatorController)
            {
                 _animatorController.SetGrounded(Grounded);
            }
        }
        

        private void Move(Vector2 inputVector, float deltaTime)
        {
            if (slide) return;
            // set target speed based on move speed, sprint speed and if sprint is pressed
            
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            
            if (Grounded)
            {
                targetSpeed = inputVector.magnitude * SprintSpeed;
            }
            else 
            {
                targetSpeed = currentHorizontalSpeed-frictionForce*deltaTime;
            }

            // a reference to the players current horizontal velocity
            

            float speedOffset = 0.1f;
            float inputMagnitude = inputVector.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;


            //TODO: Aqui creo qeu se puede ahorrar inputDirecction y meter directamente inputVector si el inPutVector viene ya con cero en y
            // normalise input direction
            inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (inputVector != Vector2.zero)
            {
                if (Grounded)
                {
                    targetLookAt = Vector3.Lerp(transform.position + transform.forward, transform.position + inputDirection, _rotationVelocity);
                    transform.LookAt(targetLookAt);
                }
                else 
                {
                    targetLookAt = Vector3.Lerp(transform.position + transform.forward, transform.position + inputDirection, _rotationVelocity/2f);
                    transform.LookAt(targetLookAt);
                }
            }
            else
            {
                Vector3 strightForward = transform.forward;
                strightForward.y = 0;

                targetLookAt = Vector3.Lerp(transform.position + transform.forward, transform.position + strightForward.normalized, _rotationVelocity / 2f);
                transform.LookAt(targetLookAt);
            }

            //Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player

            _controller.Move(inputDirection.normalized * (_speed * deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * deltaTime);


            // update animator if using character
            if (_animatorController)
            {
                _animatorController.SetSpeed(_animationBlend);
                _animatorController.SetMotionSpeed(inputMagnitude);
            }
        }

        private void JumpAndGravity(bool inputJump, float deltaTime)
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_animatorController)
                {
                    _animatorController.SetJump(false);
                    _animatorController.SetFreeFall( false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (inputJump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_animatorController)
                    {
                        _animatorController.SetJump(true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= deltaTime;
                }               
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_animatorController)
                    {
                        _animatorController.SetFreeFall(true);
                    }
                }

                // if we are not grounded, do not jump
                inputJump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * deltaTime;
            }
        }
        
    }
