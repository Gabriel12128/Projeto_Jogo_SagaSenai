using Godot;
using System;
using EnemyC;

namespace PlayerC
{
	public partial class Player : CharacterBody2D
	{
		private int life = 100;
		private float coletaveis = 0;
        private int damage = 20;


		private const float Speed = 250.0f;
		private const float JumpDuration = 0.6f;
		private const float JumpHeight = -30f;
		private float previousHeight;
		private float previousRadius;
		private float atackDuration = 0.7f;

		private float timeAtackIdle = 0f;
		private float timeJump = 0f;

		
		private bool takingDamage = false;
		private float damageTimer = 0f;
		private const float damageBlinkDuration = 0.25f;

		
		private float damageCooldownTimer = 0f;
		private const float damageCooldown = 1.2f;

	
		private Vector2 knockbackVelocity = Vector2.Zero;
		private const float knockbackForce = 320f;
		private const float knockbackFriction = 1100f;

		[Export] private AnimatedSprite2D anim;
		[Export] private CollisionShape2D collision;
		[Export] private CollisionShape2D hitBoxAtackIdle;
		[Export] private CpuParticles2D walkPar;
		[Export] private CpuParticles2D damagePar;
		


		enum PlayerState
		{
			idle,
			walk,
			jump,
			atack_idle
		}

		private PlayerState state;

		public override void _Ready()
		{
			Go_To_Idle();

			CapsuleShape2D capsule = (CapsuleShape2D)collision.Shape;
			previousHeight = capsule.Height;
			previousRadius = capsule.Radius;

			hitBoxAtackIdle.Disabled = true;
		}

		public override void _Process(double delta)
		{
			Hud.instance.UpdateLife(life);
			Hud.instance.UpdateGotas(coletaveis);
			Death();
        }

		public override void _PhysicsProcess(double delta)
		{
			float d = (float)delta;

		
			if (damageCooldownTimer > 0f)
				damageCooldownTimer -= d;

			UpdateDamageBlink(d);

		
			if (knockbackVelocity.Length() > 1f)
			{
				Velocity = knockbackVelocity;
				MoveAndSlide();

				knockbackVelocity = knockbackVelocity.MoveToward(
					Vector2.Zero,
					knockbackFriction * d
				);
				return;
			}

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

		
		public void TakeDamage(int value, Vector2 enemyPosition)
		{

			
			if (damagePar != null)
			{
				damagePar.Reparent(GetTree().CurrentScene);
				damagePar.GlobalPosition = GlobalPosition;
				damagePar.Emitting = true;
			}



			if (damageCooldownTimer > 0f)
				return;

			damageCooldownTimer = damageCooldown;

			life -= value;

			takingDamage = true;
			damageTimer = damageBlinkDuration;

			anim.Modulate = new Color(2f, 2f, 2f, 1f);

			Vector2 dir = (GlobalPosition - enemyPosition).Normalized();
			knockbackVelocity = dir * knockbackForce;
		}

	
		private void UpdateDamageBlink(float delta)
		{
			if (!takingDamage)
				return;

			damageTimer -= delta;

			if (damageTimer <= 0f)
			{
				takingDamage = false;
				anim.Modulate = Colors.White;
			}
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
			collision.Position = new Vector2(0, 10);
		}

		private void Idle()
		{
			Vector2 input = InputMap();
			ExitJumpSizeCollision();
			walkPar.Emitting = false;

			if (Input.IsActionJustPressed("jump"))
			{
				Go_To_Jump();
				return;
			}

			if (Input.IsActionJustPressed("atack"))
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

			if (Input.IsActionJustPressed("atack"))
			{
				Go_To_AtackIdle();
				return;
			}

			if (input == Vector2.Zero)
			{
				Go_To_Idle();
				return;
			}

			if(Velocity != Vector2.Zero)
    			walkPar.Emitting = true;
			
    			

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

			if (timeAtackIdle >= atackDuration)
			{
				hitBoxAtackIdle.Disabled = true;
				Go_To_Idle();
			}
		}

		public int GetLife()
		{
			return life;
		}

		public void SetLife(int value)
		{
			life = value;
		}

		private Vector2 InputMap()
		{
			return Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down").Normalized();
		}

		private void Flip(Vector2 input)
		{
			if (input.X > 0)
				anim.FlipH = false;
			else if (input.X < 0)
				anim.FlipH = true;

			Vector2 dir = walkPar.Direction;

			if (anim.FlipH)
				dir.X = 1;
			else
				dir.X = -1;

			walkPar.Direction = dir;

			FlipCollision(input.X > 0);
		}

		private void FlipCollision(bool facingRight)
		{
			if (facingRight)
				hitBoxAtackIdle.Position = new Vector2(25, 10);
			else
				hitBoxAtackIdle.Position = new Vector2(-25, 10);
		}

		public void UpdateGota(float g)
		{
            coletaveis += g;
			GD.Print(coletaveis);
            Hud.instance.UpdateGotas(coletaveis);
        }

		private void Death()
		{
			if(life <= 0)
			{
				if(GameOver.instance != null)
					GameOver.instance.ShowGameOver();

                QueueFree();
            }
				
        }

		public void win()
		{
			if(coletaveis > 6)
			{
				if(Win.instance != null)
					Win.instance.ShowWin();
			}
		}


        public void _on_hit_idle_body_entered(Node2D body)
		{
			if (body is Enemy enemy)
				enemy.TakeDamage(damage);
		}
	}
}