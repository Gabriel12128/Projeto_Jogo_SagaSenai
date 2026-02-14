using Godot;
using System;
using PlayerC;
using Interfaces;

namespace EnemyC
{
	public partial class Enemy : CharacterBody2D
	{
		public int life { get; set; } = 100;

		public Vector2 position { get; set; }

		public int damage { get; private set; } = 20;

		public float speed { get; set; } = 50f;

		

		[Export] private Player player;

		[Export] private Node behaviorNode;

		[Export] private AnimatedSprite2D anim;

		enum EnemyState
		{
			idle,
			walk,
			atack	
		}

		private EnemyState state;

		private IEnemyBehavior behavior;

		public override void _Ready()
		{
			behavior = behaviorNode as IEnemyBehavior;
			Go_To_Idle();
		}

		public override void _PhysicsProcess(double delta)
		{
			if(behaviorNode != null)
			{
				behavior.Execute(player, this, delta);
			}

			switch(state)
			{
				case EnemyState.idle:
					Idle();
					break;
				
				case EnemyState.walk:
					Walk();
					break;
				
			}
		}


		public void Go_To_Idle()
		{
			state = EnemyState.idle;
			anim.Play("idle");
		}

		public void Go_To_Walk()
		{
			state = EnemyState.walk;
			anim.Play("Walk");
		}

		private void Idle()
		{
			if (Velocity != Vector2.Zero)
			{
				Go_To_Walk();
				return;
			}	
		}

		private void Walk()
		{
			Flip();
			if(Velocity == Vector2.Zero)
			{
				Go_To_Idle();
				return;
			}
		}

		private void Flip()
		{
			if(Velocity.X > 0)
			{
				anim.FlipH = false;
				//flipCollision = true;
			}	
			else if(Velocity.X < 0)
			{
				anim.FlipH = true;
				//flipCollision = false;
			}
		}

	}
}

