using Godot;
using Interfaces;
using PlayerC;
using EnemyC;

public partial class ChaseBehavior : Node, IEnemyBehavior
{
	private float attackTimer = 0f;
	private float speedC = 90f;
	private const float chaseDistance = 300f;
	private const float stopDistance = 40f;
	private const float attackInterval = 0.6f;
	
	
	
	public void Execute(Player player, Enemy enemy, double delta)
	{

		if (player == null || !GodotObject.IsInstanceValid(player))
    	{
        	enemy.Velocity = Vector2.Zero;
        	return;
    	}


		float d = (float)delta;

		enemy.speed = speedC;

		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		Vector2 dir = toPlayer.Normalized();

		if (distance < chaseDistance && distance > stopDistance)
		{
			enemy.Velocity = dir * enemy.speed;
			enemy.MoveAndSlide();
			attackTimer = 0f;
			return;
		}

		if (distance <= stopDistance)
		{
			enemy.Velocity = Vector2.Zero;

			attackTimer += d;

			if (attackTimer >= attackInterval)
			{
				Attack(player, enemy);
				
				attackTimer = 0f;
			}
			return;
		}

		enemy.Velocity = enemy.Velocity.Lerp(Vector2.Zero, 12f * d);
		attackTimer = 0f;
	}

	private void Attack(Player player, Enemy enemy)
	{
		
		player.TakeDamage(enemy.damage, enemy.GlobalPosition);
	}
}