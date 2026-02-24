using Godot;
using System;
using PlayerC;
using Interfaces;


namespace EnemyC
{
    

	public partial class Enemy : CharacterBody2D
	{
		private int life = 100;

		public int damage { get; private set; } = 20;

		public float speed { get; set; } = 50f;

		private float Speed = 2f;

		private float intencityIdle = 3f;

		private float intencityWalk = 3f;

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
					IdleAnimation(delta);
					break;
				
				case EnemyState.walk:
					Walk();
					WalkAnimation(delta);
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
				animt.FlipH = false;
				//flipCollision = true;
			}	
			else if(Velocity.X < 0)
			{
				animt.FlipH = true;
				//flipCollision = false;
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

		private void IdleAnimation(double delta)
        {
			float time = 0;
            time += (float)delta;

    		animt.RotationDegrees = Mathf.Sin(time * Speed) * intencityIdle;
        }

		private void WalkAnimation(double delta)
        {
			float time = 0;
            time += (float)delta;

    		animt.RotationDegrees = Mathf.Sin(time * Speed) * intencityWalk;
        }

	}

}
