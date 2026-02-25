using Godot;
using System;
using PlayerC;
using Interfaces;

namespace EnemyC
{
	public partial class Enemy : CharacterBody2D
	{

        [Export] private PackedScene coletavelScene;


        private int life = 100;

		public int damage { get; private set; } = 25;
		public float speed { get; set; } = 50f;

		private float Speed = 6f;
		private float intencityIdle = 10f;
		private float intencityWalk = 15f;

		private float animTime = 0f;

		private bool takingDamage = false;
		private float damageTimer = 0f;
		private const float damageBlinkDuration = 0.25f;
		private const float blinkSpeed = 25f;

		private float damageCooldownTimer = 0f;
		private const float damageCooldown = 1.2f;

		private Vector2 knockbackVelocity = Vector2.Zero;
		private const float knockbackForce = 260f;
		private const float knockbackFriction = 900f;

		[Export] private Player player;
		[Export] private Node behaviorNode;
		[Export] public Sprite2D animt;

		enum EnemyState
		{
			idle,
			walk,
		}

		private EnemyState state;
		private IEnemyBehavior behavior;

		public override void _Ready()
		{
			behavior = behaviorNode as IEnemyBehavior;
			Go_To_Idle();
		}

		public override void _Process(double _delta)
        {
			if(life <= 25)
				DropCollectavel();
        }

        public override void _PhysicsProcess(double delta)
		{

			

            if (player == null)
				return;

            float d = (float)delta;

			if (behavior != null && knockbackVelocity == Vector2.Zero)
				behavior.Execute(player, this, delta);

			animTime += d;

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
			}

			switch (state)
			{
				case EnemyState.idle:
					Idle();
					IdleAnimation();
					break;

				case EnemyState.walk:
					Walk();
					WalkAnimation();
					break;
			}
		}

		public void Go_To_Idle()
		{
			state = EnemyState.idle;
		}

		public void Go_To_Walk()
		{
			state = EnemyState.walk;
		}

		private void Idle()
		{
			if (Velocity != Vector2.Zero)
				Go_To_Walk();
		}

		private void Walk()
		{
			Flip();

			if (Velocity == Vector2.Zero)
				Go_To_Idle();
		}

		private void Flip()
		{
			if (animt == null)
				return;

			if (Velocity.X > 0)
				animt.FlipH = false;
			else if (Velocity.X < 0)
				animt.FlipH = true;
		}

		public int GetLife()
		{
			return life;
		}

		public void SetLife(int value)
		{
			life = value;
		}

		public void TakeDamage(int value)
		{
			if (damageCooldownTimer > 0f)
				return;

			damageCooldownTimer = damageCooldown;

			life -= value;

			takingDamage = true;
			damageTimer = damageBlinkDuration;

			Vector2 dir =
				(GlobalPosition - player.GlobalPosition).Normalized();

			knockbackVelocity = dir * knockbackForce;

                if(life <= 0)
				    QueueFree();
		}

		private void UpdateDamageBlink(float delta)
		{
			if (!takingDamage || animt == null)
				return;

			damageTimer -= delta;

			float blink = Mathf.Abs(Mathf.Sin(Time.GetTicksMsec() * 0.001f * blinkSpeed));

			animt.Modulate = new Color(1f, 1f, 1f, 1f).Lerp(
				new Color(2f, 2f, 2f, 1f),
				blink
			);

			if (damageTimer <= 0f)
			{
				takingDamage = false;
				animt.Modulate = Colors.White;
			}
		}

		private void IdleAnimation()
		{
			if (animt == null)
				return;

			animt.RotationDegrees =
				Mathf.Sin(animTime * Speed) * intencityIdle;
		}

		private void WalkAnimation()
		{
			if (animt == null)
				return;

			animt.RotationDegrees =
				Mathf.Sin(animTime * Speed) * intencityWalk;
		}

        private void DropCollectavel()
        {
            if (coletavelScene == null)
                return;





            var coletavel = coletavelScene.Instantiate<Node2D>();

            GetParent().AddChild(coletavel);
            coletavel.GlobalPosition = GlobalPosition;

        }
    }
}