namespace Paradox
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The collision state around the character.
    /// </summary>
    public class CharacterCollisionState
    {
        public bool Right { get; set; }
        public bool Left { get; set; }
        public bool Above { get; set; }
        public bool Below { get; set; }
        public bool BecameGroundedThisFrame { get; set; }

        public void Reset()
        {
            this.Right = false;
            this.Left = false;
            this.Above = false;
            this.Below = false;
            this.BecameGroundedThisFrame = false;
        }
    }

    /// <summary>
    /// 2D character controller.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterController : MonoBehaviour
    {
        private float horizontalRaySeparation;
        private float verticalRaySeparation;
        private BoxCollider2D boxCollider;
        private RaycastOrigins rayOrigins;

        public Vector3 Velocity { get; private set; }
        public CharacterCollisionState CollisionState { get; private set; }
        public bool IsGrounded { get { return this.CollisionState.Below; } }

        /// <summary>
        /// Indicates that the character collided with an obstacle.
        /// </summary>
        public event Action<RaycastHit2D> Collided;

        /// <summary>
        /// The distance within the box collider that rays should begin casting from. This is to prevent immediately
        /// colliding with a surface on which the character is standing, which will give an incorrect normal.
        /// </summary>
        [Range(0, 0.3f)]
        public float SkinWidth = 0.02f;

        /// <summary>
        /// The maximum slope angle that can be walked.
        /// </summary>
        [Range(0, 90f)]
        public float SlopeLimit = 30f;

        /// <summary>
        /// The speed multiplier for slope angles.
        /// </summary>
        public AnimationCurve SlopeSpeedMultiplier = new AnimationCurve(new Keyframe(0, 1), new Keyframe(90, 0));

        /// <summary>
        /// The number of horizontal rays to use for collision detection.
        /// </summary>
        [Range(2, 20)]
        public int HorizontalRays = 8;

        /// <summary>
        /// The number of vertical rays to use for collision detection.
        /// </summary>
        [Range(2, 20)]
        public int VerticalRays = 4;

        /// <summary>
        /// Mask with all layers that the character should interact with.
        /// </summary>
        public LayerMask PlatformMask;

        /// <summary>
        /// Mask with all layers that the character can jump but not fall through.
        /// </summary>
        public LayerMask OneWayPlatformMask;

        /// <summary>
        /// Called when the component is being instantiated.
        /// </summary>
        public void Awake()
        {
            this.boxCollider = this.GetComponent<BoxCollider2D>();
            this.CollisionState = new CharacterCollisionState();

            // Add one-way platforms to the normal platform mask so that we can land on them from above
            this.PlatformMask |= this.OneWayPlatformMask;

            // Calculate the ray separation
            float height = this.boxCollider.size.y * Mathf.Abs(this.transform.localScale.y) - (2 * this.SkinWidth);
            this.horizontalRaySeparation = height / (this.HorizontalRays - 1);
            float width = this.boxCollider.size.x * Mathf.Abs(this.transform.localScale.x) - (2 * this.SkinWidth);
            this.verticalRaySeparation = width / (this.VerticalRays - 1);
        }

        /// <summary>
        /// Perform movement.
        /// </summary>
        /// <param name="deltaMovement">The movement vector.</param>
	    public void Move(Vector3 deltaMovement)
	    {
            bool wasGroundedBeforeMoving = this.IsGrounded;

		    this.CollisionState.Reset();
            this.UpdateRayOrigins();

            // Perform horizontal movement
            if (deltaMovement.x != 0)
            {
                this.MoveHorizontally(ref deltaMovement);
            }

            // Perform vertical movement
            if (deltaMovement.y != 0)
            {
                this.MoveVertically(ref deltaMovement);
            }

            // Update the position and velocity
            this.transform.Translate(deltaMovement);
            this.Velocity = deltaMovement / Time.deltaTime;

		    // Set the becameGrounded state based on the previous and current collision state
            if (!wasGroundedBeforeMoving && this.IsGrounded)
            {
                this.CollisionState.BecameGroundedThisFrame = true;
            }
	    }

        /// <summary>
        /// Draws a ray in the unity editor.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        private void DrawRay(Vector3 start, Vector3 dir, Color color, float length)
        {
            Debug.DrawRay(start, dir * length, color);
        }

        /// <summary>
        /// Perform vertical movement.
        /// </summary>
        /// <param name="deltaMovement">The movement vector.</param>
        private void MoveVertically(ref Vector3 deltaMovement)
        {
            bool isGoingUp = deltaMovement.y > 0;
            float rayDistance = Mathf.Abs(deltaMovement.y) + this.SkinWidth;
            Vector2 rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
            Vector3 initialRayOrigin = isGoingUp ? this.rayOrigins.TopLeft : this.rayOrigins.BottomLeft;
            LayerMask mask = this.PlatformMask;
            if (isGoingUp)
            {
                // Ignore the one-way platform if we're going up
                mask &= ~this.OneWayPlatformMask;
            }

            for (var i = 0; i < this.VerticalRays; i++)
            {
                var ray = new Vector2(initialRayOrigin.x + i * this.horizontalRaySeparation, initialRayOrigin.y);

                // Cast the ray until a platform is hit
                RaycastHit2D hit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

                // Debug draw a ray
                this.DrawRay(ray, rayDirection, hit ? Color.red : Color.blue, rayDistance);

                if (hit)
                {
                    // Limit the character movement and update ray distance for the closest collision so far
                    deltaMovement.y = hit.point.y - ray.y;
                    rayDistance = Mathf.Abs(deltaMovement.y);

                    // Update the collision state. Also, the skin width is removed from the movement, since this
                    // was included as 'padding' in the ray origin and distance
                    if (isGoingUp)
                    {
                        deltaMovement.y -= this.SkinWidth;
                        this.CollisionState.Above = true;
                    }
                    else
                    {
                        deltaMovement.y += this.SkinWidth;
                        this.CollisionState.Below = true;
                    }

                    if (this.Collided != null)
                    {
                        this.Collided(hit);
                    }

                    // Stop checking for collisions if this is a direct impact
                    if (rayDistance < this.SkinWidth + 0.001f)
                        return;
                }
            }
        }

        /// <summary>
        /// Perform horizontal movement.
        /// </summary>
        /// <param name="deltaMovement">The movement vector.</param>
        private void MoveHorizontally(ref Vector3 deltaMovement)
        {
            bool isGoingRight = deltaMovement.x > 0;
            float rayDistance = Mathf.Abs(deltaMovement.x) + this.SkinWidth;
            Vector2 rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
            Vector3 initialRayOrigin = isGoingRight ? this.rayOrigins.BottomRight : this.rayOrigins.BottomLeft;
            LayerMask mask = this.PlatformMask & ~this.OneWayPlatformMask; // Ignore one-way platforms in horizontal movement

            for (var i = 0; i < this.HorizontalRays; i++)
            {
                var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * this.horizontalRaySeparation);

                // Cast the ray until a platform is hit
                RaycastHit2D hit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

                // Debug draw a ray
                this.DrawRay(ray, rayDirection, hit ? Color.red : Color.blue, rayDistance);

                if (hit)
                {
                    // Only the bottom ray can hit a slope. Try to perform slope processing
                    if (i == 0 &&
                        this.TryMoveSlope(ref deltaMovement, Vector2.Angle(hit.normal, Vector2.up), isGoingRight))
                    {
                        if (this.Collided != null)
                        {
                            this.Collided(hit);
                        }

                        break;
                    }

                    // Limit the character movement and update ray distance for the closest collision so far
                    deltaMovement.x = hit.point.x - ray.x;
                    rayDistance = Mathf.Abs(deltaMovement.x);

                    // Update the collision state. Also, the skin width is removed from the movement, since this
                    // was included as 'padding' in the ray origin and distance
                    if (isGoingRight)
                    {
                        deltaMovement.x -= this.SkinWidth;
                        this.CollisionState.Right = true;
                    }
                    else
                    {
                        deltaMovement.x += this.SkinWidth;
                        this.CollisionState.Left = true;
                    }

                    if (this.Collided != null)
                    {
                        this.Collided(hit);
                    }

                    // Stop checking for collisions if this is a direct impact
                    if (rayDistance < this.SkinWidth + 0.001f)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Perform slope movement.
        /// </summary>
        /// <param name="deltaMovement">The movement vector.</param>
        /// <param name="angle">The slope angle.</param>
        /// <param name="isGoingRight">The character direction.</param>
        /// <returns>True if the character is on a slope.</returns>
        private bool TryMoveSlope(ref Vector3 deltaMovement, float angle, bool isGoingRight)
        {
            // TODO:FIXME: This method needs to take into account the hit distance. Otherwise the character could start
            // 'moving up' the slope prior to actually reaching the slope
            if (angle < 90f)
            {
                if (angle < this.SlopeLimit)
                {
                    // Check that we aren't jumping
                    // TODO: this uses a magic number which isn't ideal!
                    if (deltaMovement.y < 0.07f)
                    {
                        // Apply the slopeModifier. Perhaps we should divide by deltaTime then modify the speed and
                        // multiply by deltaTime again to get the distance...
                        float slopeModifier = this.SlopeSpeedMultiplier.Evaluate(angle);
                        deltaMovement.x *= slopeModifier;

                        // Update the collision state. Also, the skin width is removed from the movement, since this
                        // was included as 'padding' in the ray origin and distance
                        if (isGoingRight)
                        {
                            deltaMovement.x -= this.SkinWidth;
                            this.CollisionState.Right = true;
                        }
                        else
                        {
                            deltaMovement.x += this.SkinWidth;
                            this.CollisionState.Left = true;
                        }

                        // Calculate climb/descent distance based on slope angle and x distance
                        deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);

                        this.CollisionState.Below = true;
                    }
                }
                else
                {
                    // Stop moving
                    deltaMovement.x = 0;
                }

                return true;
            }
            else
            {
                // Disregard 90 degree angles (walls)
                return false;
            }
        }

        /// <summary>
        /// Updates the origins from which rays are cast.
        /// </summary>
        private void UpdateRayOrigins()
        {
            var colliderSize = new Vector2(
                this.boxCollider.size.x * Mathf.Abs(this.transform.localScale.x),
                this.boxCollider.size.y * Mathf.Abs(this.transform.localScale.y)) / 2;
            this.rayOrigins.TopRight = transform.position + new Vector3(colliderSize.x, colliderSize.y);
            this.rayOrigins.TopRight.x -= this.SkinWidth;
            this.rayOrigins.TopRight.y -= this.SkinWidth;
            this.rayOrigins.TopLeft = transform.position + new Vector3(-colliderSize.x, colliderSize.y);
            this.rayOrigins.TopLeft.x += this.SkinWidth;
            this.rayOrigins.TopLeft.y -= this.SkinWidth;
            this.rayOrigins.BottomRight = transform.position + new Vector3(colliderSize.x, -colliderSize.y);
            this.rayOrigins.BottomRight.x -= this.SkinWidth;
            this.rayOrigins.BottomRight.y += this.SkinWidth;
            this.rayOrigins.BottomLeft = transform.position + new Vector3(-colliderSize.x, -colliderSize.y);
            this.rayOrigins.BottomLeft.x += this.SkinWidth;
            this.rayOrigins.BottomLeft.y += this.SkinWidth;
        }

        /// <summary>
        /// The corner points of the box from which rays are cast.
        /// </summary>
        private struct RaycastOrigins
        {
            public Vector3 TopRight;
            public Vector3 TopLeft;
            public Vector3 BottomRight;
            public Vector3 BottomLeft;
        }
    }
}