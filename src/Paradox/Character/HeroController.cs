﻿namespace Paradox.Character
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
                    }
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