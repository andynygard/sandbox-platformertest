namespace Paradox.Character
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The controller for the hero.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Animator))]
    public class HeroController : MonoBehaviour
    {
        private CharacterController2D characterController;
        private Animator animator;
        private bool jumpButtonPressed;
        private float jumpStartAirspeed;

        /// <summary>
        /// The gravity effecting the character.
        /// </summary>
        public float Gravity = -50;

        /// <summary>
        /// The run speed.
        /// </summary>
        public float RunSpeed = 25f;

        /// <summary>
        /// The maximum jump height.
        /// </summary>
        public float MaxJumpHeight = 8;

        /// <summary>
        /// The ground acceleration.
        /// </summary>
        public float GroundAcceleration = 2;

        /// <summary>
        /// The speed scaling when turning on the ground.
        /// </summary>
        public float GroundTurnScaling = 2f;

        /// <summary>
        /// The air acceleration.
        /// </summary>
        public float AirAcceleration = 1;

        /// <summary>
        /// The speed scaling when turning in the air.
        /// </summary>
        public float AirTurnScalar = 2f;

        /// <summary>
        /// Called when the component is being instantiated.
        /// </summary>
        public void Awake()
        {
            this.characterController = this.GetComponent<CharacterController2D>();
            this.animator = this.GetComponent<Animator>();
        }

        /// <summary>
        /// Called once per frame.
        /// </summary>
        public void Update()
        {
            Vector3 velocity = this.characterController.Velocity;
            
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float jump = Input.GetAxis("Jump");

            // Flip the object if direction has changed
            if (horizontal > 0 && this.transform.localScale.x < 0f)
            {
                this.transform.localScale =
                    new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
            }
            else if (horizontal < 0 && this.transform.localScale.x > 0f)
            {
                this.transform.localScale =
                    new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
            }

            // Calculate the target velocity
            float targetVelocity = horizontal * this.RunSpeed;

            // Calculate acceleration factor
            float accelerationFactor = this.characterController.IsGrounded ?
                this.GroundAcceleration : this.AirAcceleration;
            if (this.characterController.IsGrounded && velocity.x * targetVelocity < 0)
            {
                accelerationFactor *= this.GroundTurnScaling;
            }
            else if (!this.characterController.IsGrounded && Mathf.Abs(velocity.x) < this.jumpStartAirspeed)
            {
                accelerationFactor *= this.AirTurnScalar;
            }

            // Set the velocity by interpolating with the acceleration factor
            velocity.x = Mathf.Lerp(velocity.x, targetVelocity, Time.deltaTime * accelerationFactor);

            if (this.jumpButtonPressed)
            {
                // Check if jump button was released
                if (jump == 0)
                {
                    this.jumpButtonPressed = false;
                    if (velocity.y > 0)
                    {
                        // If the jump button is released on ascent, enact an 'invisible ceiling'
                        velocity.y = 0;
                    }
                }
            }
            else
            {
                // Check if jump button was pressed
                if (jump > 0)
                {
                    this.jumpButtonPressed = true;
                    if (this.characterController.IsGrounded)
                    {
                        // Apply the velocity required to reach the jump height
                        velocity.y = Mathf.Sqrt(-2 * this.MaxJumpHeight * this.Gravity);

                        // Retain the horizontal velocity at start of jump
                        this.jumpStartAirspeed = Mathf.Abs(velocity.x);
                    }
                }
            }

            // Apply gravity
            velocity.y += this.Gravity * Time.deltaTime;

            // Now attempt to move the character
            this.characterController.Move(velocity * Time.deltaTime);

            // Update the animation state
            float horizontalSpeed = Mathf.Abs(velocity.x);
            this.animator.speed = horizontalSpeed / this.RunSpeed;
            this.animator.SetFloat("HorizontalSpeed", horizontalSpeed);
            this.animator.SetBool("IsGrounded", this.characterController.IsGrounded);
        }
    }
}