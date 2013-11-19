using UnityEngine;
using System.Collections;

public class HeroController : MonoBehaviour
{
	public float MoveForce = 365f;
	public float JumpForce = 50;
	public float MaxSpeed = 5f;

	// Flags
	private bool facingRight;
	private bool grounded;
	private bool jump;

	// Used to determine where the ground lies
	private Transform groundCheck;

	// Component references
	private Animator anim;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		this.facingRight = this.transform.localScale.x >= 0;
		this.anim = this.GetComponent<Animator>();
		this.groundCheck = this.transform.Find("GroundCheck");
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(this.transform.position, this.groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void FixedUpdate ()
	{
		float h = Input.GetAxis("Horizontal");
		
		// The Speed animator parameter is set to the absolute value of the horizontal input.
		this.anim.SetFloat("Speed", Mathf.Abs(h));

		// Add movement force
		if(h * this.rigidbody2D.velocity.x < MaxSpeed)
		{
			this.rigidbody2D.AddForce(Vector2.right * h * MoveForce);
		}

		// Limit to the max spee
		if(Mathf.Abs(this.rigidbody2D.velocity.x) > MaxSpeed)
		{
			this.rigidbody2D.velocity = new Vector2(
				Mathf.Sign(this.rigidbody2D.velocity.x) * this.MaxSpeed,
				this.rigidbody2D.velocity.y);
		}
		
		// Flip the player if necessary
		if(h > 0 && !this.facingRight || h < 0 && this.facingRight)
		{
			this.Flip();
		}

		// If the player should jump...
		if(jump)
		{
			// Add a vertical force to the player.
			this.rigidbody2D.AddForce(new Vector2(0f, this.JumpForce));
			
			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			this.jump = false;
		}
	}

	/// <summary>
	/// Flip the orientation.
	/// </summary>
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		this.facingRight = !this.facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = this.transform.localScale;
		theScale.x *= -1;
		this.transform.localScale = theScale;
	}
}
