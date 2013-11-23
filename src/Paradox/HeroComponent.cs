namespace Paradox
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The controller for the hero.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class HeroController : MonoBehaviour
    {
        private CharacterController characterController;
        private Animator animator;
        private bool jumpInProgress;

        /// <summary>
        /// The gravity effecting the character.
        /// </summary>
        public float Gravity = -9.8f;

        /// <summary>
        /// The run speed.
        /// </summary>
        public float RunSpeed = 8f;

        /// <summary>
        /// The maximum jump height.
        /// </summary>
        public float MaxJumpHeight = 5;

        /// <summary>
        /// The time (in seconds) that it takes to achieve maximum run speed on the ground.
        /// </summary>
        public float TimeToRunOnGround = 1;

        /// <summary>
        /// The time (in seconds) that it takes to achieve maximum run speed in the air.
        /// </summary>
        public float TimeToRunInAir = 5;

        /// <summary>
        /// Called when the component is being instantiated.
        /// </summary>
        public void Awake()
        {
            this.characterController = this.GetComponent<CharacterController>();
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

            if (!this.jumpInProgress)
            {
                // Check for jumping
                if (jump > 0 && this.characterController.IsGrounded)
                {
                    this.jumpInProgress = true;

                    // Apply the velocity required to reach the jump height
                    velocity.y = Mathf.Sqrt(-2 * this.MaxJumpHeight * this.Gravity);
                }
            }
            else
            {
                // If the jump button is released, enact an 'invisible ceiling'
                if (jump == 0 || this.characterController.IsGrounded)
                {
                    velocity.y = 0;
                    this.jumpInProgress = false;
                }
            }

            // Apply damping to velocity
            float damping = this.characterController.IsGrounded ? this.TimeToRunOnGround : this.TimeToRunInAir;
            velocity.x = Mathf.Lerp(velocity.x, horizontal * this.RunSpeed, Time.deltaTime / damping);

            // Apply gravity
            velocity.y += this.Gravity * Time.deltaTime;

            // Now attempt to move the character
            this.characterController.Move(velocity * Time.deltaTime);

            this.animator.SetFloat("HorizontalSpeed", Mathf.Abs(velocity.x));
            this.animator.SetBool("IsGrounded", this.characterController.IsGrounded);
        }
    }
}