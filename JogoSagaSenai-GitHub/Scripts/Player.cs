using Godot;
using System;

namespace PlayerC
{
	public partial class Player : CharacterBody2D
	{

		public int life = 100;
		private int damage = 100;

		private const float Speed = 250.0f;
		private const float JumpDuration = 0.6f;
		private const float JumpHeight = -30f;
		private float previousHeight;
		private float previousRadius;
		private float atackDuration = 0.7f;

		private float timeAtackIdle = 0f;
		private float timeAtackWalk = 0f;

		private float timeJump = 0f;

		[Export] private AnimatedSprite2D anim;
		[Export] private CollisionShape2D collision;
		[Export] private CollisionShape2D hitBoxAtackIdle;
		[Export] private CollisionShape2D hitBoxAtackWalk;

		enum PlayerState
		{
			idle,
			walk,
			jump,
			atack_idle,
			atack_walk
		}

	

		private PlayerState state;

		public override void _Ready()
		{
			Go_To_Idle();
			CapsuleShape2D capsule = (CapsuleShape2D)collision.Shape;
			previousHeight = capsule.Height;
			previousRadius = capsule.Radius;
			hitBoxAtackIdle.Disabled = true;
			hitBoxAtackWalk.Disabled = true;
		}

		public override void _Process(double _delta)
		{
			Hud.instance.UpdateLifeText(life);
		}

		public override void _PhysicsProcess(double delta)
		{
			switch (state)
			{
				case PlayerState.idle:
					Idle();
					break;
				case PlayerState.walk:
					Walk();
					break;
				case PlayerState.jump:
					Jump(delta);
					break;
				case PlayerState.atack_idle:
					AtackIdle(delta);
					break;
				case PlayerState.atack_walk:
					AtackWalk(delta);
					break;		
			}
		}

		private void Go_To_Idle()
		{
			state = PlayerState.idle;
			anim.Play("Idle");
		}

		private void Go_To_Walk()
		{
			state = PlayerState.walk;
			anim.Play("Walk");
		}

		private void Go_To_Jump()
		{
			state = PlayerState.jump;
			anim.Play("Jump");
			timeJump = 0f;
			JumpSizeCollision();
		}

		private void Go_To_AtackIdle()
		{
			state = PlayerState.atack_idle;
			anim.Play("Atack_idle");
			timeAtackIdle = 0f;
			hitBoxAtackIdle.Disabled = false;
		}

		private void Go_To_AtackWalk()
		{
			state = PlayerState.atack_walk;
			anim.Play("Atack_Walk");
			timeAtackWalk = 0f;
			hitBoxAtackWalk.Disabled = false;
		}

		public void Go_to_DamageTaken()
		{
			anim.Play("Damage");
		}

		private void JumpSizeCollision()
		{
			CapsuleShape2D original = (CapsuleShape2D)collision.Shape;
			CapsuleShape2D shape = (CapsuleShape2D)original.Duplicate();
			collision.Shape = shape;
			shape.Height = 20f;
			shape.Radius = 6f;
			collision.Position = new Vector2(0, -30);
		}

		private void ExitJumpSizeCollision()
		{
			CapsuleShape2D original = (CapsuleShape2D)collision.Shape;
			CapsuleShape2D shape = (CapsuleShape2D)original.Duplicate();
			collision.Shape = shape;
			shape.Height = previousHeight;
			shape.Radius = previousRadius;
			collision.Position = Vector2.Zero;
		}

		private void Idle()
		{
			Vector2 input = InputMap();
			ExitJumpSizeCollision();
			if (Input.IsActionJustPressed("jump"))
			{
				Go_To_Jump();
				return;
			}
			if(Input.IsActionJustPressed("atack"))
			{
				Go_To_AtackIdle();
				return;
			}
			if (input != Vector2.Zero)
				Go_To_Walk();
		}

		private void Walk()
		{
			Vector2 input = InputMap();
			ExitJumpSizeCollision();
			if (Input.IsActionJustPressed("jump"))
			{
				Go_To_Jump();
				return;
			}
			if(Input.IsActionJustPressed("atack"))
			{
				Go_To_AtackWalk();
				return;
			}
			if (input == Vector2.Zero)
			{
				Go_To_Idle();
				return;
			}
			Flip(input);
			Velocity = input * Speed;
			MoveAndSlide();
		}

		private void Jump(double delta)
		{
			timeJump += (float)delta;
			float height = Mathf.Sin(timeJump * Mathf.Pi / JumpDuration) * JumpHeight;
			anim.Position = new Vector2(0, height);
			Vector2 input = InputMap();
			Velocity = input * Speed;
			MoveAndSlide();
			Flip(input);
			if (timeJump >= JumpDuration)
			{
				anim.Position = Vector2.Zero;
				ExitJumpSizeCollision();
				if (input == Vector2.Zero)
					Go_To_Idle();
				else
					Go_To_Walk();
			}
		}

		private void AtackIdle(double delta)
		{
			timeAtackIdle += (float)delta;
			if(timeAtackIdle >= atackDuration)
			{
				hitBoxAtackIdle.Disabled = true;
				Go_To_Idle();
			}
		}

		private void AtackWalk(double delta)
		{
			timeAtackWalk += (float)delta;
			Vector2 input = InputMap();
			Velocity = input * Speed;
			MoveAndSlide();
			Flip(input);
			if(timeAtackWalk >= atackDuration)
			{
				hitBoxAtackWalk.Disabled = true;
				if (input == Vector2.Zero)
					Go_To_Idle();
				else
					Go_To_Walk();
			}
		}

		public int GetLife()
		{
			return life;
		}

		public void SetLife(int Value)
		{
			life = Value;
		}

		private Vector2 InputMap()
		{
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			return direction.Normalized();
		}

		private void Flip(Vector2 input)
		{
			bool flipCollision = false;

			if(input.X > 0)
			{
				anim.FlipH = false;
				flipCollision = true;
			}	
			else if(input.X < 0)
			{
				anim.FlipH = true;
				flipCollision = false;
			}
				

				FlipCollision(flipCollision);
			}

		private void FlipCollision(bool facingRight)
		{
    		if (facingRight)
        	{
				hitBoxAtackWalk.Position = new Vector2(25, 0);
				hitBoxAtackIdle.Position = new Vector2(20, 10); 
			}	
    		else
			{
				hitBoxAtackWalk.Position = new Vector2(-25, 0); 
				hitBoxAtackIdle.Position = new Vector2(-20, 10); 
			}
        	
		}

		public void _on_hit_idle_body_entered(Node2D body)
		{
			if(body.IsInGroup("enemies"))
			{
				GD.Print("acertou");
			}
		}

		public void _on_hit_walk_body_entered(Node2D body)
		{
			if(body.IsInGroup("enemies"))
			{
				GD.Print("acertou");
			}
		}

		
	}
}


