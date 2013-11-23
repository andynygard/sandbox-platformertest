namespace Paradox.Camera
{
    using Paradox.Character;
    using UnityEngine;

    /// <summary>
    /// Follows a character.
    /// </summary>
    public class CharacterFollower : MonoBehaviour
    {
        private CharacterController2D targetCharacter;

        /// <summary>
        /// The target to follow.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// The follow speed.
        /// </summary>
        public float FollowSpeed = 2.0f;

        /// <summary>
        /// The camera offset.
        /// </summary>
        public Vector2 CameraOffset;

        /// <summary>
        /// Called when the component is being instantiated.
        /// </summary>
        public void Awake()
        {
            targetCharacter = this.Target.GetComponent<CharacterController2D>();
        }

        /// <summary>
        /// Called once per frame after the Update method.
        /// </summary>
        public void LateUpdate()
        {
            bool flipX = this.targetCharacter != null && this.targetCharacter.Velocity.x > 0;
            Vector3 dest = new Vector3(
                this.Target.position.x + (flipX ? this.CameraOffset.x : -this.CameraOffset.x),
                this.Target.position.y + this.CameraOffset.y,
                this.transform.position.z);

            this.transform.position = Vector3.Lerp(this.transform.position, dest, this.FollowSpeed * Time.deltaTime);
        }
    }
}